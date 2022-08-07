using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class Server : NetworkBehaviour
{
    private enum GameState { Pause, Normal, Loading };
    [SyncVar]
    [SerializeField]
    private GameState _currentGameState = GameState.Loading;

    [SerializeField]
    private GameObject aiClient;

    [SyncVar]
    private List<Client> clients = new List<Client>();

    private void Start()
    {
        if (!isServer) return;
    }

    private void Update()
    {
        if (!isServer) return;

        if(SceneManager.GetActiveScene().name == "Level")
        {
            if (this._currentGameState == GameState.Loading)
            {
                GameObject clients = GameObject.Find("Clients");
                if (this.clients.Count == clients.transform.childCount)
                {
                    this.setupCastles();
                    this._currentGameState = GameState.Normal;
                }
            }

            if (this._currentGameState == GameState.Normal)
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
        else if(SceneManager.GetActiveScene().name == "MainMenu")
        {

        }
    }
    public string getCurrentGameState()
    {
        return this._currentGameState.ToString();
    }

    public void registerClient(GameObject clientGameObject)
    {
        if (clientGameObject == null) throw new System.Exception("Client gameobject is null");
        Client client = clientGameObject.GetComponent<Client>();
        if (client == null) throw new System.Exception("Gameobject is not a client");

        this.clients.Add(client);
    }

    [ClientRpc]
    private void endGame()
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().activateEndUi();
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
