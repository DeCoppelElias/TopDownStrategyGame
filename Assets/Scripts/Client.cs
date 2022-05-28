using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class Client : Player
{
    public ClientStateManager clientStateManager;
    public enum GameState { Pause, Normal};
    public GameState gameState = GameState.Normal;

    public string selectedTroop;
    public UiManager uiManager;
    public Player aiClient;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isLocalPlayer)
        {
            this.transform.SetParent(GameObject.Find("Clients").transform);
            return;
        }

        this.name = "LocalClient";
        this.clientStateManager = new ClientStateManager(this);

        if (SceneManager.GetActiveScene().name == "MultiplayerScene" && isServer)
        {
            GameObject canvas = GameObject.Find("Canvas");
            canvas.GetComponent<MultiplayerSceneUi>().selectLevelUi.SetActive(true);
        }
        else if (SceneManager.GetActiveScene().name != "MultiplayerScene")
        {
            this.uiManager = GameObject.Find("Canvas").GetComponent<UiManager>();
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
        clientUpdate();

        if (!isServer) return;
        if (gameState.Equals(GameState.Normal))
        {
            serverUpdate();
        }
    }
    
    [Client]
    public void displayGold(int gold)
    {
        if (!isLocalPlayer) return;
        uiManager.displayGold(gold);
    }


    [Client]
    public void setupClientGameObjects()
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

    [Client]
    public void dyeAndNameCastle(Castle castle)
    {
        if (castle.Owner == castle.ServerClient)
        {
            castle.gameObject.name = "LocalCastle";
            GameObject.Find("Canvas").GetComponent<UiManager>().activateInGameUi(true);
            float r = 88;  // red component
            float g = 222;  // green component
            float b = 255;  // blue component
            castle.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (castle.Owner == aiClient)
        {
            castle.gameObject.name = "AiCastle";
            float r = 95;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            castle.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
        else if (castle.Owner != null)
        {
            castle.gameObject.name = "EnemyCastle";
            float r = 255;  // red component
            float g = 95;  // green component
            float b = 95;  // blue component
            castle.gameObject.GetComponent<SpriteRenderer>().color = new Color(r / 255, g / 255, b / 255, 1);
        }
    }

    [Client]
    public void initLevel()
    {
        Level level = GameObject.Find("LevelInfo").GetComponent<Level>();
        level.initLevel();

        if (isServer)
        {
            instantiateLevelServer();
        }
    }

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

    [Client]
    public void clientUpdate()
    {
        clientStateManager.stateActions();
    }

    [Client]
    public void createTroopEvent(string troopName)
    {
        selectedTroop = troopName;
        clientStateManager.changeState("DrawPathState");
    }

    [Client]
    public void createSelectedTroop(List<Vector2> path)
    {
        createTroop(selectedTroop, path);
    }

    [Client]
    public Vector2 getCastlePosition()
    {
        return this.castle.transform.position;
    }

    [Command]
    public void changeGameState(string gameState)
    {
        Debug.Log("Updating gamestate from " + this.gameState.ToString() + " to " + gameState);
        updateGameStateOnClients(gameState);
    }

    [Command]
    public void createTroop(string troopName, List<Vector2> path)
    {
        this.castle.createTroop(troopName, path);
    }

    [Command]
    public void findCastleForClient(GameObject clientGameObject)
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

    [ClientRpc]
    public void updateGameStateOnClients(string gameState)
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


    [ClientRpc]
    public void updateUiOnClients()
    {
        GameObject.Find("Canvas").GetComponent<UiManager>().activateInGameUi(true);
    }

    [Server]
    public void serverUpdate()
    {
        foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
        {
            Castle currentCastle = castleGameObject.GetComponent<Castle>();
            currentCastle.updateCastle();
        }
    }

    [Server]
    public void instantiateLevelServer()
    {
        Level level = GameObject.Find("LevelInfo").GetComponent<Level>();
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        networkManager.maxConnections = level.maxPlayers;

        foreach(Vector2 castlePosiiton in level.castlePositions)
        {
            GameObject castle = Instantiate(level.castlePrefab, castlePosiiton, Quaternion.identity, GameObject.Find("Castles").transform);
            NetworkServer.Spawn(castle);
        }
        
        updateUiOnClients();
    }

    [Server]
    public void destoryObject(GameObject obj)
    {
        NetworkServer.Destroy(obj);
    }
}
