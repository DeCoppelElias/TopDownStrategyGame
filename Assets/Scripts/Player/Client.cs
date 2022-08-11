using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using System;
using UnityEngine.EventSystems;

public class Client : Player
{
    private ClientStateManager clientStateManager;
    [SerializeField]
    private string selectedTroop;
    [SerializeField]
    private string selectedTower;
    private LevelSceneUi uiManager;

    private Server server;

    public override void OnStartClient()
    {
        base.OnStartClient(); 
        this.transform.SetParent(GameObject.Find("Clients").transform);
        if (!isLocalPlayer) return;

        this.name = "LocalClient";

        if (SceneManager.GetActiveScene().name == "LevelSelectScene")
        {
            LevelSelectScene levelSelectSceneSceneUi = GameObject.Find("Canvas").GetComponent<LevelSelectScene>();
            levelSelectSceneSceneUi.setClient(this);
            clientJoinedLevelSelect();
            if (isServer)
            {
                levelSelectSceneSceneUi.activateSelectLevelUi();
            }
        }

        else if (SceneManager.GetActiveScene().name == "Level")
        {
            NetworkManager.singleton.offlineScene = "BackToMainMenuScene";
            this.server = GameObject.Find("Server").GetComponent<Server>();
            registerClient();
        }

        else if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            this.server = GameObject.Find("Server").GetComponent<Server>();
        }
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (SceneManager.GetActiveScene().name == "Level")
        {
            if (this.server.getCurrentGameState() == "Normal")
            {
                clientUpdate();
            }
        }
    }

    [TargetRpc]
    public void clientSetup(NetworkConnection target)
    {
        if (SceneManager.GetActiveScene().name == "Level")
        {
            this.clientStateManager = new ClientStateManager(this);
            this.uiManager = GameObject.Find("Canvas").GetComponent<LevelSceneUi>();
            this.uiManager.setupLevelSceneUi(this);

            setMaxPlayers();

            uiManager.setupStartGameUi();

            setupClientGameObjects();

            findServerClient();

            findCastleForClient();

            Invoke("clientSetupDone", 0.5f);
        }
    }

    [Command]
    public void clientSetupDone()
    {
        Server server = GameObject.Find("Server").GetComponent<Server>();
        server.clientSetupDone();
    }

    [Command]
    public void registerClient()
    {
        Server server = GameObject.Find("Server").GetComponent<Server>();
        server.registerClient(this.gameObject);
    }

    [Client]
    public void findCastleForClient()
    {
        findCastleForClient(this.gameObject);
    }

    /// <summary>
    /// This method is called when a client wants to leave the game
    /// </summary>
    [Client]
    public void leaveGame()
    {
        if (!isLocalPlayer) return;
        if (SceneManager.GetActiveScene().name == "Level")
        {
            this.changeGameState("Normal");
            this.onClientDisconnect(this.gameObject);
        }
        Invoke("disconnect", 0.5f);
    }

    /// <summary>
    /// This method is called on the local client when it wants to disconnect
    /// </summary>
    [Client]
    private void disconnect()
    {
        if (isServer)
        {
            Debug.Log("Stop host");
            NetworkManager.singleton.ServerChangeScene("LevelSelectScene");
        }
        else
        {
            Debug.Log("Stop client");
            NetworkManager.singleton.StopClient();
        }
    }

    [Client]
    public void destoryObject(GameObject obj)
    {
        this.server.destoryObject(obj);
    }

    [Client]
    public void clientCastleDestroyed(NetworkConnection networkConnection)
    {
        this.server.clientCastleDestroyed(networkConnection);
    }

    [Client]
    public void checkGameDoneAfterDelay()
    {
        this.server.checkGameDoneAfterDelay();
    }

    [Client]
    public string getClientState()
    {
        return clientStateManager.getClientState();
    }

    [Client]
    public void displayInfo(Dictionary<string, object> info)
    {
        this.uiManager.displayInfo(info);
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
        foreach (GameObject clientGameObject in GameObject.FindGameObjectsWithTag("client"))
        {
            clientGameObject.transform.SetParent(clients.transform);
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
        if (clientStateManager == null) return;
        clientStateManager.stateActions();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.activateOptionsUi();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(mousePosition, 1);
            foreach (Collider2D collider in colliders)
            {
                Entity entity = collider.GetComponent<Entity>();
                if (entity != null)
                {
                    entity.detectClick();
                }
            }
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

    [Client]
    public void createSelectedTower(Vector2 position)
    {
        //Debug.Log(position);
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

    [Client]
    public void updateAttackRingOfGameObject(GameObject gameObject, float alpha)
    {
        if (gameObject == null) return;
        if (gameObject.transform.Find("AttackRing") == null) return;
        GameObject attackRing = gameObject.transform.Find("AttackRing").gameObject;
        SpriteRenderer spriteRenderer = attackRing.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        Color alphaColor = spriteRenderer.color;
        alphaColor.a = alpha;
        spriteRenderer.color = alphaColor;
    }

    [Client]
    public void updateAttackRingOfGameObject(GameObject gameObject, Vector3 scale)
    {
        if (gameObject == null) return;
        if (gameObject.transform.Find("AttackRing") == null) return;
        GameObject attackRing = gameObject.transform.Find("AttackRing").gameObject;
        SpriteRenderer spriteRenderer = attackRing.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        attackRing.transform.localScale = scale;
    }

    [Client]
    public void updateDetectRingOfGameObject(GameObject gameObject, float alpha)
    {
        if (gameObject == null) return;
        if (gameObject.transform.Find("DetectRing") == null) return;
        GameObject detectRing = gameObject.transform.Find("DetectRing").gameObject;
        SpriteRenderer spriteRenderer = detectRing.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        Color alphaColor = spriteRenderer.color;
        alphaColor.a = alpha;
        spriteRenderer.color = alphaColor;
    }

    [Client]
    public void updateDetectRingOfGameObject(GameObject gameObject, Vector3 scale)
    {
        if (gameObject == null) return;
        GameObject detectRing = gameObject.transform.Find("DetectRing").gameObject;
        if (detectRing == null) return;
        SpriteRenderer spriteRenderer = detectRing.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;
        detectRing.transform.localScale = scale;
    }




    [Command]
    private void clientJoinedLevelSelect()
    {
        if (!isServer) return;
        LevelSelectScene levelSelectSceneSceneUi = GameObject.Find("Canvas").GetComponent<LevelSelectScene>();
        levelSelectSceneSceneUi.clientJoined();
    }

    /// <summary>
    /// This method will do the necessary server updates when a client is disconnected
    /// </summary>
    /// <param name="clientGameObject"></param>
    [Command]
    public void onClientDisconnect(GameObject clientGameObject)
    {
        Server levelSceneServer = GameObject.Find("Server").GetComponent<Server>();
        levelSceneServer.onClientDisconnect(clientGameObject);
    }

    /// <summary>
    /// Will change the game state
    /// </summary>
    /// <param name="gameState"></param> a string representing a game state
    [Command]
    public void changeGameState(string gameState)
    {
        //Debug.Log("Updating gamestate from " + this.gameState.ToString() + " to " + gameState);
        Server levelSceneServer = GameObject.Find("Server").GetComponent<Server>();
        levelSceneServer.updateGameStateOnServer(gameState);
    }

    /// <summary>
    /// Will search for a castle on the server and link it to the given parameter
    /// </summary>
    /// <param name="clientGameObject"></param> a gameobject representing the client where the found castle will be linked to
    [Command]
    public void findCastleForClient(GameObject clientGameObject)
    {
        Server levelSceneServer = GameObject.Find("Server").GetComponent<Server>();
        levelSceneServer.findCastleForClient(clientGameObject);
    }

    /// <summary>
    /// Will instantiate the level on the server
    /// </summary>
    [Command]
    public void setMaxPlayers()
    {
        Level level = GameObject.Find("LevelInfo").GetComponent<Level>();
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        networkManager.maxConnections = level.maxPlayers;
    }

    [Command]
    public void setTroopVisibility(Troop troop, Client client, bool newVisibility)
    {
        if (troop == null || client == null) return;
        this.server.setTroopVisibilityForClient(client.connectionToClient, troop.gameObject, newVisibility);
    }
}
