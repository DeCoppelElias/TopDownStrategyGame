using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MainMenuManager : NetworkBehaviour
{
    private List<Castle> castles = new List<Castle>();
    private List<AiClient> aiClients = new List<AiClient>();

    [SerializeField]
    private GameObject aiPrefab;

    private bool setupCheck = false;

    [Server]
    private void setup()
    {
        GameObject localClient = GameObject.Find("LocalClient");
        if (localClient == null) return;
        Client client = localClient.GetComponent<Client>();

        GameObject castlesContainer = GameObject.Find("Castles");
        foreach (Castle castle in castlesContainer.GetComponentsInChildren<Castle>())
        {
            GameObject ai = Instantiate(aiPrefab);
            NetworkServer.Spawn(ai);
            AiClient aiClient = ai.GetComponent<AiClient>();
            castle.Owner = aiClient;
            castle.ServerClient = client;
            aiClient.castle = castle;
            aiClients.Add(aiClient);
            castles.Add(castle);
            Debug.Log("Added castle to castles");
        }

        setupCheck = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;
        if (!setupCheck)
        {
            setup();
        }
        else
        {
            updateCastles();
            updateAi();

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
    }

    [Server]
    public void updateCastles()
    {
        //Debug.Log(this.castles.Count);
        foreach (Castle castle in castles)
        {
            castle.updateCastle();
        }
    }

    [Server]
    public void updateAi()
    {
        foreach(AiClient ai in aiClients)
        {
            ai.aiUpdate();
        }
    }
}
