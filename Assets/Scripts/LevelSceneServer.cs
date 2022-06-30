using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LevelSceneServer : NetworkBehaviour
{
    private enum GameState { Pause, Normal, Loading };
    [SyncVar]
    [SerializeField]
    private GameState _currentGameState = GameState.Loading;

    [SerializeField]
    private GameObject aiClient;

    [SyncVar]
    public List<Client> clients;

    private void Start()
    {
        if (!isServer) return;

    }

    private void Update()
    {
        if (!isServer) return;

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

    [Command]
    public void registerClient(GameObject clientGameObject)
    {
        if (clientGameObject == null) throw new System.Exception("Client gameobject is null");
        Client client = clientGameObject.GetComponent<Client>();
        if (client == null) throw new System.Exception("Gameobject is not a client");

        this.clients.Add(client);
    }
    /// <summary>
    /// This method will do the necessary server updates when a client is disconnected
    /// </summary>
    /// <param name="clientGameObject"></param>
    [Command]
    public void onClientDisconnect(GameObject clientGameObject)
    {
        Debug.Log("client " + this + " disconnected, setting castle owner to null");
        Client client = clientGameObject.GetComponent<Client>();
        client.castle.Owner = null;
    }
    /// <summary>
    /// Will change the game state
    /// </summary>
    /// <param name="gameState"></param> a string representing a game state
    [Command]
    public void changeGameState(string gameState)
    {
        //Debug.Log("Updating gamestate from " + this.gameState.ToString() + " to " + gameState);
        setGameState(gameState);
    }

    /// <summary>
    /// Will search for a castle on the server and link it to the given parameter
    /// </summary>
    /// <param name="clientGameObject"></param> a gameobject representing the client where the found castle will be linked to
    [Command]
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
    /// Will instantiate the level on the server
    /// </summary>
    [Server]
    private void instantiateLevelServer()
    {
        Level level = GameObject.Find("LevelInfo").GetComponent<Level>();
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        networkManager.maxConnections = level.maxPlayers;
    }

    /// <summary>
    /// Will update the game state on every client on the server
    /// </summary>
    /// <param name="gameState"></param> a string representing the new game state
    [Server]
    private void setGameState(string gameState)
    {
        if (gameState.Equals("Normal"))
        {
            foreach (Client client in FindObjectsOfType<Client>())
            {
                this._currentGameState = GameState.Normal;
            }
        }
        else if (gameState.Equals("Pause"))
        {
            foreach (Client client in FindObjectsOfType<Client>())
            {
                this._currentGameState = GameState.Pause;
            }
        }
        else
        {
            throw new System.Exception("There is no Game State called: " + gameState);
        }
    }

    /// <summary>
    /// Will destroy a gameobject
    /// </summary>
    /// <param name="obj"></param> the gameobject that will be destroyed
    [Server]
    private void destoryObject(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }

    [Server]
    private void checkGameDoneAfterDelay()
    {
        Invoke("checkGameDone", 0.5f);
    }

    [Server]
    private void setupCastles()
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

    [Server]
    private void checkGameDone()
    {
        Debug.Log("checking if game is done");
        if (GameObject.FindGameObjectsWithTag("castle").Length == 1)
        {
            Debug.Log("game is finished!");
            endGame();
        }
    }










    [ClientRpc]
    private void endGame()
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().activateEndUi();
    }
}
