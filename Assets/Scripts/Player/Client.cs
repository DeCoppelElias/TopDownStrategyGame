using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;

public class Client : Player
{
    private ClientStateManager clientStateManager;
    public enum GameState { Pause, Normal};
    [SyncVar][SerializeField]
    private GameState gameState = GameState.Pause;
    [SerializeField]
    private string selectedTroop;
    [SerializeField]
    private string selectedTower;
    private LevelSceneUi uiManager;
    [SerializeField]
    private Player aiClient;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isLocalPlayer)
        {
            this.transform.SetParent(GameObject.Find("Clients").transform);
            return;
        }

        this.name = "LocalClient";

        if (SceneManager.GetActiveScene().name == "MultiplayerScene")
        {
            MultiplayerSceneUi multiplayerSceneUi = GameObject.Find("Canvas").GetComponent<MultiplayerSceneUi>();
            multiplayerSceneUi.setClient(this);
            if (isServer)
            {
                multiplayerSceneUi.activateSelectLevelUi();
            }
        }
        else if (SceneManager.GetActiveScene().name != "MultiplayerScene")
        {
            this.clientStateManager = new ClientStateManager(this);

            this.uiManager = GameObject.Find("Canvas").GetComponent<LevelSceneUi>();
            this.uiManager.setClient(this);

            initLevel();

            setupClientGameObjects();

            findServerClient();

            findCastleForClient(this.gameObject);
        }
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if(SceneManager.GetActiveScene().name != "MultiplayerScene")
        {
            if (gameState.Equals(GameState.Normal))
            {
                clientUpdate();
                if (!isServer) return;
                serverUpdate();
            }
        }
    }

    /// <summary>
    /// This method is called when a client wants to leave the game
    /// </summary>
    public void leaveGame()
    {
        if (!isLocalPlayer) return;
        if (SceneManager.GetActiveScene().name != "MultiplayerScene")
        {
            this.changeGameState("Normal");
            onClientDisconnect(this.gameObject);
        }
        Invoke("disconnect", 0.5f);
    }

    /// <summary>
    /// This method is called on the local client when it wants to disconnect
    /// </summary>
    private void disconnect()
    {
        if (isServer)
        {
            Debug.Log("Stop host");
            NetworkManager.singleton.StopHost();
        }
        else
        {
            Debug.Log("Stop client");
            NetworkManager.singleton.StopClient();
        }
    }
    
    /// <summary>
    /// Will display gold on screen when local client
    /// </summary>
    /// <param name="gold"></param>
    [Client]
    public void displayGold(int gold, int maxGold)
    {
        if (!isLocalPlayer) return;
        uiManager.displayGold(gold, maxGold);
    }

    [Client]
    public void displayDrawPathUi()
    {
        uiManager.displayDrawPathUi();
    }

    [Client]
    public void displaySelectPositionUi()
    {
        uiManager.displaySelectPositionUi();
    }

    [Client]
    public void displayViewingUi()
    {
        uiManager.displayViewingUi();
    }

    [Client]
    public Dictionary<string, object> getTowerInfo(string type)
    {
        return this.castle.getTowerInfo(type);
    }

    [Client]
    public Dictionary<string, object> getTroopInfo(string type)
    {
        return this.castle.getTroopInfo(type);
    }

    /// <summary>
    /// Will search and organise gameobjects on client
    /// </summary>
    [Client]
    private void setupClientGameObjects()
    {
        GameObject castles = GameObject.Find("Castles");
        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            castleGameObject.transform.SetParent(castles.transform);
        }
        GameObject clients = GameObject.Find("Clients");
        foreach(GameObject clientGameObject in GameObject.FindGameObjectsWithTag("client"))
        {
            clientGameObject.transform.SetParent(clients.transform);
        }
    }

    /// <summary>
    /// Will initialize the level
    /// </summary>
    [Client]
    private void initLevel()
    {
        Level level = GameObject.Find("LevelInfo").GetComponent<Level>();
        level.initLevel();

        if (isServer)
        {
            instantiateLevelServer();
        }
    }

    /// <summary>
    /// Will search for the server client and set the server client parameter of every castle
    /// </summary>
    [Client]
    public void findServerClient()
    {
        Client client = GameObject.Find("LocalClient").GetComponent<Client>();
        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            Castle castle = castleGameObject.GetComponent<Castle>();
            castle.ServerClient = client;
        }
    }

    /// <summary>
    /// The update method for the client
    /// </summary>
    [Client]
    private void clientUpdate()
    {
        clientStateManager.stateActions();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.activateOptionsUi();
        }
    }

    /// <summary>
    /// Will handle the event of tower creation
    /// </summary>
    /// <param name="towerName"></param> a string representing the type of tower that will be created
    [Client]
    public void createTowerEvent(string towerName)
    {
        selectedTower = towerName;
        toSelectPositionState();
    }

    /// <summary>
    /// Will handle the event of troop creation
    /// </summary>
    /// <param name="troopName"></param> a string representing the type of troop that will be created
    [Client]
    public void createTroopEvent(string troopName)
    {
        selectedTroop = troopName;
    }

    /// <summary>
    /// Changes the client state to viewing state
    /// </summary>
    [Client]
    public void toViewingState()
    {
        clientStateManager.toViewingState();
    }

    /// <summary>
    /// Changes the client state to draw path state
    /// </summary>
    [Client]
    public void toDrawPathState()
    {
        clientStateManager.toDrawPathState();
    }

    /// <summary>
    /// Changes the client state to select entity state
    /// </summary>
    /// <param name="target"></param> the type of target to be selected
    [Client]
    public void toSelectEntityState(string target)
    {
        clientStateManager.toSelectEntityState(target);
    }

    /// <summary>
    /// Changes the client state to select position state
    /// </summary>
    [Client]

    public void toSelectPositionState()
    {
        clientStateManager.toSelectPositionState();
    }

    /// <summary>
    /// Will create the troop that was selected with createTroopEvent()
    /// </summary>
    /// <param name="path"></param> The path that the troop will take
    [Client]
    public void createSelectedTroop(List<Vector2> path)
    {
        createTroop(selectedTroop, path);
    }

    /// <summary>
    /// Will create the troop that was selected with createTroopEvent()
    /// </summary>
    /// <param name="path"></param> The path that the troop will take
    [Client]
    public void createSelectedTroop(Entity target)
    {
        if(target is Castle castle)
        {
            PathFinding pathFinding = GameObject.Find("PathFinding").GetComponent<PathFinding>();
            List<Vector3> path = pathFinding.findPath(Vector3Int.FloorToInt(this.castle.transform.position), Vector3Int.FloorToInt(castle.transform.position));
            List<Vector2> result = new List<Vector2>();
            foreach(Vector3 position in path)
            {
                result.Add(position);
            }
            createTroop(selectedTroop, result);
        }
        if(target is Troop troop)
        {
            PathFinding pathFinding = GameObject.Find("PathFinding").GetComponent<PathFinding>();
            List<Vector3> path = pathFinding.findPath(Vector3Int.FloorToInt(this.castle.transform.position), Vector3Int.FloorToInt(troop.transform.position));
            List<Vector3> path2 = pathFinding.findPath(Vector3Int.FloorToInt(troop.transform.position), Vector3Int.FloorToInt(troop.Owner.castle.transform.position));
            List<Vector2> result = new List<Vector2>();
            foreach (Vector3 position in path)
            {
                result.Add(position);
            }
            foreach (Vector3 position in path2)
            {
                result.Add(position);
            }
            createTroop(selectedTroop, result);
        }
    }

    [Client]
    public void createSelectedTower(Vector2 position)
    {
        createTower(selectedTower, position);
    }

    /// <summary>
    /// Will get the position of the castle of this client
    /// </summary>
    /// <returns></returns>
    [Client]
    public Vector2 getCastlePosition()
    {
        return this.castle.transform.position;
    }


    /// <summary>
    /// This method will do the necessary server updates when a client is disconnected
    /// </summary>
    /// <param name="clientGameObject"></param>
    [Command]
    private void onClientDisconnect(GameObject clientGameObject)
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
        Debug.Log("Updating gamestate from " + this.gameState.ToString() + " to " + gameState);
        updateGameStateOnServer(gameState);
    }

    /// <summary>
    /// Will create a troop on the server
    /// </summary>
    /// <param name="troopName"></param> a string representing the troop
    /// <param name="path"></param> the path that the troop will follow
    [Command]
    private void createTroop(string troopName, List<Vector2> path)
    {
        this.castle.createTroop(troopName, path);
    }

    [Command]
    private void createTower(string towerName, Vector2 position)
    {
        this.castle.createTower(towerName, position);
    }

    /// <summary>
    /// Will search for a castle on the server and link it to the given parameter
    /// </summary>
    /// <param name="clientGameObject"></param> a gameobject representing the client where the found castle will be linked to
    [Command]
    private void findCastleForClient(GameObject clientGameObject)
    {
        GameObject castles = GameObject.Find("Castles");
        Client client = clientGameObject.GetComponent<Client>();
        bool found = false;
        foreach (Castle castle in castles.GetComponentsInChildren<Castle>())
        {
            if ((castle.Owner == null || castle.Owner == aiClient) && !found)
            {
                found = true;
                castle.Owner = client;
                client.castle = castle;
            }
        }
    }

    /// <summary>
    /// Will update the ui on all clients to in game ui
    /// </summary>
    [ClientRpc]
    private void updateUiOnClients()
    {
        GameObject.Find("Canvas").GetComponent<LevelSceneUi>().activateInGameUi();
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

    /// <summary>
    /// Will update the server
    /// </summary>
    [Server]
    private void serverUpdate()
    {
        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            Castle currentCastle = castleGameObject.GetComponent<Castle>();
            currentCastle.updateCastle();
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
        updateUiOnClients();
    }

    /// <summary>
    /// Will update the game state on every client on the server
    /// </summary>
    /// <param name="gameState"></param> a string representing the new game state
    [Server]
    private void updateGameStateOnServer(string gameState)
    {
        if (gameState.Equals("Normal"))
        {
            foreach (Client client in FindObjectsOfType<Client>())
            {
                client.gameState = GameState.Normal;
            }
        }
        else if (gameState.Equals("Pause"))
        {
            foreach (Client client in FindObjectsOfType<Client>())
            {
                client.gameState = GameState.Pause;
            }
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

    private void checkGameDone()
    {
        Debug.Log("checking if game is done");
        if (GameObject.FindGameObjectsWithTag("castle").Length == 1)
        {
            Debug.Log("game is finished!");
            endGame();
        }
    }
}
