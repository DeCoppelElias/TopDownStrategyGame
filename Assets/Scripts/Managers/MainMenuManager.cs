using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using kcp2k;

public class MainMenuManager : MonoBehaviour
{
    private List<Castle> castles = new List<Castle>();
    private List<AiClient> aiClients = new List<AiClient>();

    [SerializeField]
    private GameObject aiPrefab;

    private bool setupDone = false;

    private void Start()
    {
        // Setting up server to simulate background
        if (NetworkServer.connections.Count == 0)
        {
            // Trying different ports to host
            int port = 7777;
            KcpTransport transport = NetworkManager.singleton.GetComponent<KcpTransport>();

            bool found = false;
            int counter = 0;
            while (!found && counter < 10)
            {
                try
                {
                    transport.Port = (ushort)port;

                    NetworkManager.singleton.StartHost();
                    NetworkManager.singleton.maxConnections = 1;
                    found = true;
                }
                catch (System.Exception e)
                {
                    port++;
                }
                counter++;
            }
        }

        // Set up background battle
        Invoke("setupBackground", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            checkClick();
        }
    }

    /// <summary>
    /// Setups background battle
    /// </summary>
    [Server]
    private void setupBackground()
    {
        // Searching local client
        GameObject localClient = GameObject.Find("LocalClient");
        Client client = localClient.GetComponent<Client>();
        if (localClient == null) return;

        // Spawning Castles
        GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
        GameObject castleContainer = GameObject.Find("Castles");

        // Setting up first castle
        GameObject castle1GameObject = Instantiate(castlePrefab, new Vector3(-30, 0, 0), Quaternion.identity, castleContainer.transform);
        NetworkServer.Spawn(castle1GameObject);
        Castle castle1 = castle1GameObject.GetComponent<Castle>();
        GameObject ai1 = Instantiate(aiPrefab);
        NetworkServer.Spawn(ai1);
        AiClient aiClient1 = ai1.GetComponent<AiClient>();
        castle1.Owner = aiClient1;
        castle1.ServerClient = client;
        // Random gold gain cooldown
        castle1.GoldCooldown = Random.Range(2, 6);
        aiClient1.castle = castle1;
        aiClients.Add(aiClient1);
        castles.Add(castle1);

        // Setting up second castle
        GameObject castle2GameObject = Instantiate(castlePrefab, new Vector3(30, 0, 0), Quaternion.identity, castleContainer.transform);
        NetworkServer.Spawn(castle2GameObject);
        Castle castle2 = castle2GameObject.GetComponent<Castle>();
        GameObject ai2 = Instantiate(aiPrefab);
        NetworkServer.Spawn(ai2);
        AiClient aiClient2 = ai2.GetComponent<AiClient>();
        castle2.Owner = aiClient2;
        castle2.ServerClient = client;
        aiClient2.castle = castle2;
        // Random gold gain cooldown
        castle2.GoldCooldown = Random.Range(2, 6);
        aiClients.Add(aiClient2);
        castles.Add(castle2);

        // Setting check
        setupDone = true;
    }

    /// <summary>
    /// Resets background battle
    /// </summary>
    [Server]
    public void resetBackGround()
    {
        // Deleting all troops
        foreach (Troop troop in GameObject.Find("Troops").GetComponentsInChildren<Troop>())
        {
            NetworkServer.Destroy(troop.gameObject);
        }

        // Deleting all castles
        foreach (Castle castle in GameObject.Find("Castles").GetComponentsInChildren<Castle>())
        {
            NetworkServer.Destroy(castle.gameObject);
        }

        // Deleting all towers
        foreach (Tower tower in GameObject.Find("Towers").GetComponentsInChildren<Tower>())
        {
            NetworkServer.Destroy(tower.gameObject);
        }

        // Deleting all Ai Clients
        foreach (AiClient aiClient in GameObject.Find("AiClients").GetComponentsInChildren<AiClient>())
        {
            NetworkServer.Destroy(aiClient.gameObject);
        }

        setupBackground();
    }

    /// <summary>
    /// Checks if click was on troop and displays path if true
    /// </summary>
    private void checkClick()
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
