using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Text;

public class Server : NetworkBehaviour
{
    private enum GameState { Pause, Normal, InitialWait, SearchingClients, LoadingLevel, SetupClients, WaitForSetupClients, SetupAi, WaitForSetupAi};
    [SyncVar]
    [SerializeField]
    private GameState _currentGameState = GameState.InitialWait;

    [SerializeField]
    private GameObject aiClient;

    [SyncVar]
    private List<Client> clients = new List<Client>();

    private int amountClientsSetup = 0;

    private float startTime = 0;

    private int levelInfoByteCounter = 0;
    private bool levelInfoFullySend = false;
    private bool levelSetup = false;
    private bool setupCastlesDone = false;

    [SyncVar(hook = nameof(onLoadingInfoChanged))]
    private string loadingInfo = "";
    [SyncVar(hook = nameof(onLoadingDoneChanged))]
    private bool loadingDone = false;

    private void Start()
    {
        if (!isServer) return;
        startTime = Time.time;
    }
    private void Update()
    {
        if (!isServer) return;

        if (SceneManager.GetActiveScene().name == "Level")
        {
            LevelSceneUi levelSceneUi = GameObject.Find("Canvas").GetComponent<LevelSceneUi>();

            if (this._currentGameState == GameState.InitialWait)
            {
                loadingInfo = "Waiting for clients to connect";

                if (Time.time - startTime > 2)
                {
                    this._currentGameState = GameState.SearchingClients;
                }
            }

            else if (this._currentGameState == GameState.SearchingClients)
            {
                loadingInfo = "Searching for clients";

                GameObject clients = GameObject.Find("Clients");
                if (this.clients.Count == clients.transform.childCount)
                {
                    this._currentGameState = GameState.LoadingLevel;
                }
            }

            else if (this._currentGameState == GameState.LoadingLevel)
            {
                loadingInfo = "Loading level";

                // Init server objects in level (ex castles)
                GameObject.Find("LevelInfo").GetComponent<Level>().initLevelServer();

                // Level file is really big so send in parts to clients
                sendPartOfLevelInfoToClients();

                // After all parts are send build level on clients
                if (levelInfoFullySend) initLevelClients();

                if (levelSetup) this._currentGameState = GameState.SetupClients;
            }

            else if (this._currentGameState == GameState.SetupClients)
            {
                loadingInfo = "Setting up clients";

                foreach (Client client in this.clients)
                {
                    client.clientSetup(client.GetComponent<NetworkIdentity>().connectionToClient);
                }
                this._currentGameState = GameState.WaitForSetupClients;
            }

            else if (this._currentGameState == GameState.WaitForSetupClients)
            {
                loadingInfo = "Waiting for clients to setup";

                if (amountClientsSetup == this.clients.Count)
                {
                    this._currentGameState = GameState.SetupAi;
                }
            }

            else if (this._currentGameState == GameState.SetupAi)
            {
                loadingInfo = "Setting up Ai";

                this.setupCastles();
                this._currentGameState = GameState.WaitForSetupAi;
            }

            else if (this._currentGameState == GameState.WaitForSetupAi)
            {
                loadingInfo = "Waiting for Ai to setup";

                if (setupCastlesDone)
                {
                    this._currentGameState = GameState.Normal;
                    loadingDone = true;
                }
            }

            else if (this._currentGameState == GameState.Normal)
            {
                // Updating Ai
                foreach (AiClient aiClient in GameObject.Find("AiClients").transform.GetComponentsInChildren<AiClient>())
                {
                    aiClient.aiUpdate();
                }

                // Updating castles
                foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
                {
                    Castle currentCastle = castleGameObject.GetComponent<Castle>();
                    currentCastle.updateCastle();
                }
            }
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu")
        {

        }
    }

    private void onLoadingInfoChanged(string oldLoadingInfo, string newLoadingInfo)
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().setLoadingStatus(loadingInfo);
    }

    private void onLoadingDoneChanged(bool oldLoadingDone, bool newLoadingDone)
    {
        if(newLoadingDone) GameObject.Find("Canvas").GetComponent<LevelSceneUi>().deactivateLoadingUi();
    }

    public string getCurrentGameState()
    {
        return this._currentGameState.ToString();
    }

    [Server]
    private void changeToNormalState()
    {
        this._currentGameState = GameState.Normal;
    }

    [Server]
    public void registerClient(GameObject clientGameObject)
    {
        if (clientGameObject == null) throw new System.Exception("Client gameobject is null");
        Client client = clientGameObject.GetComponent<Client>();
        if (client == null) throw new System.Exception("Gameobject is not a client");

        this.clients.Add(client);
    }

    [Server]
    public void clientSetupDone()
    {
        this.amountClientsSetup++;
    }

    [ClientRpc]
    private void endGame()
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().activateEndUi();
    }

    [Server]
    private void sendPartOfLevelInfoToClients()
    {
        if (levelInfoFullySend) return;

        string levelInfo = GameObject.Find("LevelInfo").GetComponent<Level>().getLevelInfo();
        byte[] bytes = Encoding.ASCII.GetBytes(levelInfo);

        // Amount of bytes that are send every frame
        int n = 20000;

        // Last part of the string will be send
        if (levelInfoByteCounter + n > bytes.Length)
        {
            levelInfoFullySend = true;
        }

        // Calculating chunk
        int upperBound = Mathf.Min(levelInfoByteCounter + n, bytes.Length);
        byte[] chunk = new byte[upperBound - levelInfoByteCounter];
        for (int i = levelInfoByteCounter; i < upperBound; i++)
        {
            chunk[i - levelInfoByteCounter] = bytes[i];
        }

        // Sending the substring to all clients
        string substring = Encoding.ASCII.GetString(chunk);
        sendPartOfLevelInfo(substring);

        // Updating the counter
        levelInfoByteCounter += n;
    }

    [ClientRpc]
    private void sendPartOfLevelInfo(string subString)
    {
        GameObject.Find("LevelInfo").GetComponent<Level>().addPartOfLevelInfo(subString);
    }

    [Server]
    private void initLevelClients()
    {
        initLevel();
        levelSetup = true;
    }

    [ClientRpc]
    private void initLevel()
    {
        GameObject.Find("LevelInfo").GetComponent<Level>().initLevelClient();
    }

    [TargetRpc]
    public void clientCastleDestroyed(NetworkConnection networkConnection)
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().disableAllUi();
    }

    [TargetRpc]
    public void setTroopVisibilityForClient(NetworkConnection networkConnection, GameObject troopGameObject, bool newVisibility)
    {
        Troop troop = troopGameObject.GetComponent<Troop>();
        troop.setVisibility(newVisibility);
    }

    /// <summary>
    /// Will update the server
    /// </summary>
    [Server]
    private void serverUpdate()
    {
        foreach (AiClient aiClient in GameObject.Find("AiClients").transform.GetComponentsInChildren<AiClient>())
        {
            aiClient.aiUpdate();
        }

        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            Castle currentCastle = castleGameObject.GetComponent<Castle>();
            currentCastle.updateCastle();
        }
    }

    /// <summary>
    /// Will update the game state on every client on the server
    /// </summary>
    /// <param name="gameState"></param> a string representing the new game state
    [Server]
    public void updateGameStateOnServer(string gameState)
    {
        if (gameState.Equals("Normal"))
        {
            this._currentGameState = GameState.Normal;
        }
        else if (gameState.Equals("Pause"))
        {
            this._currentGameState = GameState.Pause;
        }
        else
        {
            Debug.Log("There is no gameState called: " + gameState);
        }
    }

    /// <summary>
    /// Will destroy a gameobject
    /// </summary>
    /// <param name="obj"></param> the gameobject that will be destroyed
    [Server]
    public void destoryObject(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }

    [Server]
    public void checkGameDoneAfterDelay()
    {
        Invoke("checkGameDone", 0.5f);
    }

    [Server]
    public void setupCastles()
    {
        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            Castle currentCastle = castleGameObject.GetComponent<Castle>();
            if (currentCastle.Owner == null)
            {
                GameObject clients = GameObject.Find("AiClients");
                GameObject ai = Instantiate(aiClient, new Vector3(0, 0, 0), Quaternion.identity, clients.transform);
                NetworkServer.Spawn(ai);
                currentCastle.Owner = ai.GetComponent<AiClient>();
                ai.GetComponent<AiClient>().castle = currentCastle;
            }
        }

        setupCastlesDone = true;
    }

    /// <summary>
    /// Will search for a castle on the server and link it to the given parameter
    /// </summary>
    /// <param name="clientGameObject"></param> a gameobject representing the client where the found castle will be linked to
    [Server]
    public void findCastleForClient(GameObject clientGameObject)
    {
        GameObject castles = GameObject.Find("Castles");
        Client client = clientGameObject.GetComponent<Client>();
        foreach (Castle castle in castles.GetComponentsInChildren<Castle>())
        {
            if (castle.Owner == null)
            {
                castle.Owner = client;
                client.castle = castle;
                return;
            }
            if (castle.Owner is AiClient ai)
            {
                Destroy(ai.gameObject);

                castle.Owner = client;
                client.castle = castle;
                return;
            }
        }
    }

    [Server]
    public void onClientDisconnect(GameObject clientGameObject)
    {
        Debug.Log("client " + this + " disconnected, setting castle owner to null");
        Client client = clientGameObject.GetComponent<Client>();
        client.castle.Owner = null;
    }

    [Server]
    private void checkGameDone()
    {
        Debug.Log("checking if game is done");
        if (GameObject.FindGameObjectsWithTag("castle").Length == 1)
        {
            Debug.Log("game is finished!");
            this._currentGameState = GameState.Pause;
            endGame();
        }
    }
}
