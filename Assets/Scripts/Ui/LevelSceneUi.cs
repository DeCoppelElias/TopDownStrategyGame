using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class LevelSceneUi : MonoBehaviour
{
    private Client client;
    private GameObject inGameUi;
    private GameObject optionsUi;
    private GameObject endGameUi;
    private GameObject troopScrollView;
    private GameObject troopCreationMethods;
    private GameObject createTroopUi;
    private GameObject towerScrollView;
    private NetworkManager networkManager;
    private TextMeshProUGUI goldDisplay;
    void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        inGameUi = GameObject.Find("InGameUi");
        goldDisplay = GameObject.Find("GoldDisplay").GetComponent<TextMeshProUGUI>();
        optionsUi = GameObject.Find("OptionsUi");
        endGameUi = GameObject.Find("EndGameUi");
        troopCreationMethods = GameObject.Find("TroopCreationMethods");
        troopScrollView = GameObject.Find("TroopScrollView");
        createTroopUi = GameObject.Find("CreateTroopUi");
        towerScrollView = GameObject.Find("TowerScrollView");
        towerScrollView.SetActive(false);
        troopScrollView.SetActive(false);
        troopCreationMethods.SetActive(false);
        endGameUi.SetActive(false);
        optionsUi.SetActive(false);
        inGameUi.SetActive(false);
    }

    public void displayGold(int gold)
    {
        goldDisplay.text = gold.ToString();
    }

    public void disableAllUi()
    {
        endGameUi.SetActive(false);
        optionsUi.SetActive(false);
        inGameUi.SetActive(false);
        troopCreationMethods.SetActive(false);
    }
    public void activateInGameUi()
    {
        client.changeGameState("Normal");
        optionsUi.SetActive(false);
        inGameUi.SetActive(true);
    }

    public void activateOptionsUi()
    {
        client.changeGameState("Pause");
        optionsUi.SetActive(true);
        inGameUi.SetActive(false);
    }

    public void activateEndUi()
    {
        string text;
        if (this.client.castle)
        {
            text = "You have won";
        }
        else
        {
            text = "You have lost";
        }
        disableAllUi();
        endGameUi.SetActive(true);
        endGameUi.GetComponentInChildren<TextMeshProUGUI>().text = text;
        if (!this.client.isServer)
        {
            endGameUi.GetComponentInChildren<Button>().gameObject.SetActive(false);
        }
    }

    public void activateTroopMethodUi()
    {
        troopCreationMethods.SetActive(true);
    }
    public void activateTroopScrollView()
    {
        client.changeClientState("ViewingState");
        towerScrollView.SetActive(false);
        if (troopScrollView.activeSelf)
        {
            troopScrollView.SetActive(false);
        }
        else
        {
            troopScrollView.SetActive(true);
        }
    }

    public void activateTowerScrollView()
    {
        client.changeClientState("ViewingState");
        troopScrollView.SetActive(false);
        if (troopScrollView.activeSelf)
        {
            towerScrollView.SetActive(false);
        }
        else
        {
            towerScrollView.SetActive(true);
        }
    }

    public void setClient(Client client)
    {
        this.client = client;
    }

    public void createTower(string towerName)
    {
        client.createTowerEvent(towerName);
    }

    public void createTroop(string troopName)
    {
        client.changeClientState("ViewingState");
        client.createTroopEvent(troopName);
        activateTroopMethodUi();
    }

    public void selectCastle()
    {
        client.changeToSelectState("Castle");
        troopCreationMethods.SetActive(false);
    }

    public void selectTroop()
    {
        client.changeToSelectState("Troop");
        troopCreationMethods.SetActive(false);
    }

    public void changeClientState(string state)
    {
        client.changeClientState(state);
        troopCreationMethods.SetActive(false);
    }

    public void pauseGame()
    {
        client.changeGameState("Pause");
    }

    public void unPauseGame()
    {
        client.changeGameState("Normal");
    }

    public void clientLeave()
    {
        client.leaveGame();
    }

    public void returnToLevelSelect()
    {
        networkManager.ServerChangeScene("MultiplayerScene");
    }
}
