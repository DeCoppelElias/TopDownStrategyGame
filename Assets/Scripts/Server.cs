using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Text;
using System.Linq;
using UnityEngine.UI;

public class Server : NetworkBehaviour
{
    private enum GameState { Pause, Normal, InitialWait, SearchingClients, LoadingLevel, SetupClients, WaitForSetupClients, SetupAi, WaitForSetupAi, ClientJoinesDuringGame};
    [SyncVar]
    [SerializeField]
    private GameState _currentGameState = GameState.InitialWait;

    [SerializeField]
    private GameObject aiClient;

    [SyncVar]
    private List<Client> clients = new List<Client>();

    private Dictionary<Client, (int, bool, bool, bool)> clientsJoining = new Dictionary<Client, (int, bool, bool, bool)>();

    private enum ClientJoiningState {SendingLevelInfo, InitializingLevel, SetupClient}
    private ClientJoiningState clientJoiningState = ClientJoiningState.SendingLevelInfo;

    private int amountClientsSetup = 0;

    private float startTime = 0;

    private int levelInfoByteCounter = 0;
    private bool levelInfoFullySend = false;
    private bool levelSetup = false;
    private bool setupCastlesDone = false;

    [SyncVar(hook = nameof(onLoadingBarProgressChanged))]
    private float loadingBarProgress = 0;
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
            // First initial wait so all clients can connect
            if (this._currentGameState == GameState.InitialWait)
            {
                loadingBarProgress = 1f / 8;
                loadingInfo = "Waiting for clients to connect";

                if (Time.time - startTime > 2)
                {
                    this._currentGameState = GameState.SearchingClients;
                }
            }

            // Find all clients and check if they are all registered
            else if (this._currentGameState == GameState.SearchingClients)
            {
                loadingBarProgress = 2f / 8;
                loadingInfo = "Searching for clients";

                GameObject clients = GameObject.Find("Clients");
                if (this.clients.Count == clients.transform.childCount)
                {
                    this._currentGameState = GameState.LoadingLevel;
                }
            }

            // Loads level
            else if (this._currentGameState == GameState.LoadingLevel)
            {
                loadingBarProgress = 3f / 8;
                loadingInfo = "Loading level";

                // Init server objects in level (ex castles)
                GameObject.Find("LevelInfo").GetComponent<Level>().initLevelServer();

                // Level file is really big so send in parts to clients
                sendPartOfLevelInfoToClients();

                // After all parts are send build level on clients
                if (levelInfoFullySend) initLevelClients();

                if (levelSetup) this._currentGameState = GameState.SetupClients;
            }

            // Setup all clients
            else if (this._currentGameState == GameState.SetupClients)
            {
                loadingBarProgress = 4f / 8;
                loadingInfo = "Setting up clients";

                foreach (Client client in this.clients)
                {
                    client.clientSetup(client.GetComponent<NetworkIdentity>().connectionToClient);
                }
                this._currentGameState = GameState.WaitForSetupClients;
            }

            // Wait until clients are all done
            else if (this._currentGameState == GameState.WaitForSetupClients)
            {
                loadingBarProgress = 5f / 8;
                loadingInfo = "Waiting for clients to setup";

                if (amountClientsSetup == this.clients.Count)
                {
                    this._currentGameState = GameState.SetupAi;
                }
            }

            // Setup all aiClients
            else if (this._currentGameState == GameState.SetupAi)
            {
                loadingBarProgress = 6f / 8;
                loadingInfo = "Setting up Ai";

                this.setupEmptyCastles();
                this._currentGameState = GameState.WaitForSetupAi;
            }

            // Wait until aiClients are all done
            else if (this._currentGameState == GameState.WaitForSetupAi)
            {
                loadingBarProgress = 7f / 8;
                loadingInfo = "Waiting for Ai to setup";

                if (setupCastlesDone)
                {
                    this._currentGameState = GameState.Normal;
                    loadingDone = true;
                    loadingBarProgress = 8f / 8;
                }
            }

            // Now the game can run normal
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

            // Client joines During Game
            else if (this._currentGameState == GameState.ClientJoinesDuringGame)
            {
                // When all clients setup return to normal gamestate
                if (clientsJoining.Count == 0)
                {
                    loadingInfo = "All clients have succesfully joined";
                    this._currentGameState = GameState.Normal;
                    this.loadingDone = true;
                    this.clientJoiningState = ClientJoiningState.SendingLevelInfo;
                }
                else
                {
                    // Getting client info
                    KeyValuePair<Client, (int, bool, bool, bool)> item = clientsJoining.ElementAt(0);
                    Client currentClient = item.Key;
                    (int, bool, bool, bool) tuple = item.Value;

                    int joiningClientLevelInfoByteCounter = tuple.Item1;
                    bool levelInfoFullySendToJoiningClient = tuple.Item2;
                    bool levelSetupOnJoiningClient = tuple.Item3;
                    bool joiningClientSetup = tuple.Item4;

                    // Send Level data
                    if (this.clientJoiningState == ClientJoiningState.SendingLevelInfo)
                    {
                        loadingInfo = "Client joined, sending level data";
                        // Level file is really big so send in parts to clients
                        sendPartOfLevelInfoToJoiningClient(currentClient);

                        if (levelInfoFullySendToJoiningClient)
                        {
                            this.clientJoiningState = ClientJoiningState.InitializingLevel;
                            loadingInfo = "Client joined, initializing level on client";
                        }
                    }

                    // After all parts are send, build level on client
                    else if (this.clientJoiningState == ClientJoiningState.InitializingLevel)
                    {
                        initLevelJoiningClient(currentClient);

                        if (levelSetupOnJoiningClient)
                        {
                            this.clientJoiningState = ClientJoiningState.SetupClient;
                            loadingInfo = "Client joined, Setting up client";
                        }
                    }

                    // Client setup
                    else if (this.clientJoiningState == ClientJoiningState.SetupClient)
                    {
                        currentClient.clientSetup(currentClient.GetComponent<NetworkIdentity>().connectionToClient);
                        clientsJoining.Remove(currentClient);
                    }
                }
            }
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu")
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

    private void onLoadingBarProgressChanged(float oldLoadingBarProgress, float newLoadingBarProgress)
    {
        GameObject loadingBarGameObject = GameObject.Find("LoadingBar");
        if (loadingBarGameObject == null) return;

        Slider slider = loadingBarGameObject.GetComponent<Slider>();
        if (slider == null) return;

        slider.value = newLoadingBarProgress;
    }

    /// <summary>
    /// If syncvar loadingInfo changed, it will display it on screen on all clients
    /// </summary>
    /// <param name="oldLoadingInfo"></param>
    /// <param name="newLoadingInfo"></param>
    private void onLoadingInfoChanged(string oldLoadingInfo, string newLoadingInfo)
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().setLoadingStatus(loadingInfo);
    }

    /// <summary>
    /// If syncvar loadingDone changed, it will remove the loading Ui on all clients
    /// </summary>
    /// <param name="oldLoadingDone"></param>
    /// <param name="newLoadingDone"></param>
    private void onLoadingDoneChanged(bool oldLoadingDone, bool newLoadingDone)
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().enableLoadingUi(!newLoadingDone);
    }

    /// <summary>
    /// Gets a string representing the gameState
    /// </summary>
    /// <returns></returns>
    public string getCurrentGameState()
    {
        return this._currentGameState.ToString();
    }


    /// <summary>
    /// This method is called by clients when they join the game
    /// </summary>
    /// <param name="clientGameObject"></param>
    [Server]
    public void registerClient(GameObject clientGameObject)
    {
        if (clientGameObject == null) throw new System.Exception("Client gameobject is null");
        Client client = clientGameObject.GetComponent<Client>();
        if (client == null) throw new System.Exception("Gameobject is not a client");
        this.clients.Add(client);

        if (SceneManager.GetActiveScene().name == "Level")
        {
            // Client joines during game
            if (this._currentGameState == GameState.Normal || this._currentGameState == GameState.Pause)
            {
                clientJoinesDuringGame(client);
            }
        }
        else if (SceneManager.GetActiveScene().name == "LevelSelectScene")
        {
            Invoke("refreshClientAmount", 0.5f);
        }
    }

    private void clientJoinesDuringGame(Client client)
    {
        this._currentGameState = GameState.ClientJoinesDuringGame;
        this.loadingDone = false;
        amountClientsSetup = 0;
        clientsJoining.Add(client, (0, false, false, false));
    }

    /// <summary>
    /// This method is called when the client is done with its setup phase
    /// </summary>
    [Server]
    public void clientSetupDone()
    {
        this.amountClientsSetup++;
    }

    [Server]
    public void refreshClientAmount()
    {
        setAmountOfPlayersOnAllClients(this.clients.Count);
    }

    [ClientRpc]
    private void setAmountOfPlayersOnAllClients(int amountOfPlayers)
    {
        GameObject.Find("Canvas").GetComponent<LevelSelectScene>().setAmountOfPlayers(amountOfPlayers);
    }

    [ClientRpc]
    public void attackingAnimationSync(GameObject troop, bool attacking)
    {
        if (troop == null) return;
        Animator animator = troop.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("Attacking", attacking);
        }
    }

    /// <summary>
    /// Ends the game for all clients
    /// </summary>
    [ClientRpc]
    private void endGame()
    {
        if(SceneManager.GetActiveScene().name == "Level")
        {
            GameObject.Find("Canvas").GetComponent<LevelSceneUi>().activateEndUi();
        }
        else if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            GameObject.Find("MainMenuManager").GetComponent<MainMenuManager>().resetBackGround();
        }
    }

    /// <summary>
    /// Will send a part of the levelInfo to the clients
    /// </summary>
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
        sendPartOfLevelInfoToAllClients(substring);

        // Updating the counter
        levelInfoByteCounter += n;
    }

    /// <summary>
    /// Will send a part of the levelInfo to specific client
    /// </summary>
    [Server]
    private void sendPartOfLevelInfoToJoiningClient(Client client)
    {
        if (client == null) return;
        (int, bool, bool, bool) tuple = clientsJoining[client];
        if (tuple.Item2) return;

        string levelInfo = GameObject.Find("LevelInfo").GetComponent<Level>().getLevelInfo();
        byte[] bytes = Encoding.ASCII.GetBytes(levelInfo);

        // Amount of bytes that are send every frame
        int n = 20000;

        // Last part of the string will be send
        int specificClientLevelInfoByteCounter = tuple.Item1;
        if (specificClientLevelInfoByteCounter + n > bytes.Length)
        {
            tuple.Item2 = true;
        }

        // Calculating chunk
        int upperBound = Mathf.Min(specificClientLevelInfoByteCounter + n, bytes.Length);
        byte[] chunk = new byte[upperBound - specificClientLevelInfoByteCounter];
        for (int i = specificClientLevelInfoByteCounter; i < upperBound; i++)
        {
            chunk[i - specificClientLevelInfoByteCounter] = bytes[i];
        }

        // Sending the substring to all clients
        string substring = Encoding.ASCII.GetString(chunk);
        sendPartOfLevelInfoToSpecificClient(client.GetComponent<NetworkIdentity>().connectionToClient, substring);

        // Updating the counter
        tuple.Item1 += n;
        clientsJoining[client] = tuple;
    }

    /// <summary>
    /// Sends a substring of the levelInfo to a specific client
    /// </summary>
    /// <param name="subString"></param>
    [TargetRpc]
    private void sendPartOfLevelInfoToSpecificClient(NetworkConnection networkConnection, string subString)
    {
        GameObject.Find("LevelInfo").GetComponent<Level>().addPartOfLevelInfo(subString);
    }

    /// <summary>
    /// Sends a substring of the levelInfo to all clients
    /// </summary>
    /// <param name="subString"></param>
    [ClientRpc]
    private void sendPartOfLevelInfoToAllClients(string subString)
    {
        GameObject.Find("LevelInfo").GetComponent<Level>().addPartOfLevelInfo(subString);
    }

    /// <summary>
    /// Initializes the level on all clients when every client has fully received levelInfo
    /// </summary>
    [Server]
    private void initLevelClients()
    {
        initLevelAllClients();
        levelSetup = true;
    }

    [Server]
    private void initLevelJoiningClient(Client client)
    {
        if (client == null) return;
        initLevelSpecificClient(client.GetComponent<NetworkIdentity>().connectionToClient);

        (int, bool, bool, bool) tuple = clientsJoining[client];
        tuple.Item3 = true;
        clientsJoining[client] = tuple;
    }

    /// <summary>
    /// Initializes the level on all clients when every client has fully received levelInfo
    /// </summary>
    [ClientRpc]
    private void initLevelAllClients()
    {
        GameObject.Find("LevelInfo").GetComponent<Level>().initLevelClient();
    }

    [TargetRpc]
    private void initLevelSpecificClient(NetworkConnection networkConnection)
    {
        GameObject.Find("LevelInfo").GetComponent<Level>().initLevelClient();
    }

    /// <summary>
    /// When a clients castle is destroyed, the ui will be disabled
    /// </summary>
    /// <param name="networkConnection"></param>
    [TargetRpc]
    public void clientCastleDestroyed(NetworkConnection networkConnection)
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().disableAllUi();
    }

    /// <summary>
    /// Changes troop visibility for a certain client
    /// </summary>
    /// <param name="networkConnection"></param>
    /// <param name="troopGameObject"></param>
    /// <param name="newVisibility"></param>
    [TargetRpc]
    public void setTroopVisibilityForClient(NetworkConnection networkConnection, GameObject troopGameObject, bool newVisibility)
    {
        if (troopGameObject == null) return;
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

    /// <summary>
    /// Checks if game is done afer a delay
    /// </summary>
    [Server]
    public void checkGameDoneAfterDelay()
    {
        Invoke("checkGameDone", 0.5f);
    }

    /// <summary>
    /// Puts Ai into empty castles
    /// </summary>
    [Server]
    public void setupEmptyCastles()
    {
        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            Castle currentCastle = castleGameObject.GetComponent<Castle>();
            if (currentCastle.Owner == null)
            {
                setupEmptyCastle(currentCastle);
            }
        }

        setupCastlesDone = true;
    }

    /// <summary>
    /// Puts an Ai into a castle
    /// </summary>
    /// <param name="castle"></param>
    private void setupEmptyCastle(Castle castle)
    {
        GameObject clients = GameObject.Find("AiClients");
        GameObject ai = Instantiate(aiClient, new Vector3(0, 0, 0), Quaternion.identity, clients.transform);
        NetworkServer.Spawn(ai);
        castle.Owner = ai.GetComponent<AiClient>();
        ai.GetComponent<AiClient>().castle = castle;
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

    /// <summary>
    /// When a client disconnects, it will fill its castle with an ai
    /// </summary>
    /// <param name="clientGameObject"></param>
    [Server]
    public void onClientDisconnect(GameObject clientGameObject)
    {
        //Debug.Log("client " + this + " disconnected, setting castle owner to null");
        Client client = clientGameObject.GetComponent<Client>();
        this.clients.Remove(client);

        if (SceneManager.GetActiveScene().name == "Level")
        {
            if (client.castle == null) return;

            setupEmptyCastle(client.castle);
        }
        else if (SceneManager.GetActiveScene().name == "LevelSelectScene")
        {
            refreshClientAmount();
        }
    }

    /// <summary>
    /// Checks if game is done and will do the neccasary actions
    /// </summary>
    [Server]
    private void checkGameDone()
    {
        if (GameObject.FindGameObjectsWithTag("castle").Length == 1)
        {
            this._currentGameState = GameState.Pause;
            endGame();
        }
    }
}
