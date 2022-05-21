using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Client : NetworkBehaviour
{
    public ClientStateManager clientStateManager;
    public enum GameState { Pause, Normal};
    public GameState gameState = GameState.Normal;
    public GameObject castlePrefab;
    public Castle castle;
    public string selectedTroop;
    public UiManager uiManager;
    public GameObject levels;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isLocalPlayer) return;

        this.name = "LocalClient";
        this.uiManager = GameObject.Find("Canvas").GetComponent<UiManager>();
        this.uiManager.setClient(this);
        this.levels = GameObject.Find("Levels");
        this.clientStateManager = new ClientStateManager(this);

        if (GameObject.FindGameObjectsWithTag("client").Length == 1)
        {
            uiManager.activateSelectLevelUi(true);
        }

        //Find a castle for the new client and update other clients
        clientsFindCastle();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (gameState.Equals(GameState.Normal))
        {
            clientStateManager.stateActions();
            updateGame();
        }
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
    public void updateGame()
    {
        serverUpdateGame();
    }

    [Command]
    public void selectLevelEvent(int levelID)
    {
        instantiateLevel(levelID);
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
        GameObject troop = this.castle.createTroop(troopName, path, this.castle);
        updateTroopOnClients(this.castle.gameObject, troop);
    }

    [Command]
    public void clientsFindCastle()
    {
        GameObject castles = GameObject.Find("Castles");
        foreach (Client client in FindObjectsOfType<Client>())
        {
            foreach (Castle castle in castles.GetComponentsInChildren<Castle>())
            {
                if (castle.client == null && client.castle == null)
                {
                    castle.client = client;
                    client.castle = castle;
                    updateCastleOnClients(client.gameObject, castle.gameObject);
                }
            }
        }
    }

    [ClientRpc]
    public void updateCastleOnClients(GameObject clientGameObject, GameObject castleGameObject)
    {
        Client client = clientGameObject.GetComponent<Client>();
        Castle castle = castleGameObject.GetComponent<Castle>();
        client.castle = castle;
        castle.client = client;

        GameObject localClient = GameObject.Find("LocalClient");
        if (castle.client.Equals(localClient.GetComponent<Client>()))
        {
            castleGameObject.name = "localCastle";
            GameObject.Find("Canvas").GetComponent<UiManager>().activateInGameUi(true);
        }
    }

    [ClientRpc]
    public void updateTroopOnClients(GameObject castle, GameObject troop)
    {
        troop.GetComponent<Troop>().castle = castle.GetComponent<Castle>();

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
    public void serverUpdateGame()
    {
        foreach (Castle castle in GameObject.Find("Castles").GetComponentsInChildren<Castle>())
        {
            castle.updateTroops();
        }
    }

    [Server]
    public void instantiateLevel(int levelID)
    {
        Level level = GameObject.Find("Levels").transform.GetChild(levelID).GetComponent<Level>();
        NetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        networkManager.maxConnections = level.maxPlayers;
        foreach(Vector2 castleLocation in level.castleLocations)
        {
            createCastle(castleLocation);
        }
        updateUiOnClients();
        clientsFindCastle();
    }

    [Server]
    public Castle createCastle(Vector2 castleLocation)
    {
        GameObject castle = Instantiate(castlePrefab, castleLocation, Quaternion.identity, GameObject.Find("Castles").transform);
        NetworkServer.Spawn(castle);

        return castle.GetComponent<Castle>();
    }
}
