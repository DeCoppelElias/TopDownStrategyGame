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
    private GameObject createTroopUi;
    private GameObject towerScrollView;
    private NetworkManager networkManager;
    private TextMeshProUGUI goldDisplay;
    private GameObject drawPathUi;
    private GameObject selectPositionUi;
    void Start()
    {
        GameObject networkManagerGameObject = GameObject.Find("NetworkManager");
        if (networkManagerGameObject != null)
        {
            networkManager = networkManagerGameObject.GetComponent<NetworkManager>();
        }
        inGameUi = GameObject.Find("InGameUi");
        goldDisplay = GameObject.Find("GoldDisplay").GetComponent<TextMeshProUGUI>();
        optionsUi = GameObject.Find("OptionsUi");
        endGameUi = GameObject.Find("EndGameUi");
        troopScrollView = GameObject.Find("TroopScrollView");
        createTroopUi = GameObject.Find("CreateTroopUi");
        towerScrollView = GameObject.Find("TowerScrollView");
        drawPathUi = GameObject.Find("DrawPathStateUi");
        selectPositionUi = GameObject.Find("SelectPositionStateUi");
        selectPositionUi.SetActive(false);
        drawPathUi.SetActive(false);
        towerScrollView.SetActive(false);
        troopScrollView.SetActive(false);
        endGameUi.SetActive(false);
        optionsUi.SetActive(false);
        inGameUi.SetActive(false);
    }

    public void setupStartGameUi()
    {
        inGameUi.SetActive(true);
        optionsUi.SetActive(false);
    }
    public void setupLevelSceneUi(Client client)
    {
        this.client = client;
    }

    public void displayDrawPathUi()
    {
        drawPathUi.SetActive(true);
    }

    public void displaySelectPositionUi()
    {
        selectPositionUi.SetActive(true);
    }

    public void displayViewingUi()
    {
        drawPathUi.SetActive(false);
        selectPositionUi.SetActive(false);
    }

    public void displayGold(int gold, int maxGold)
    {
        goldDisplay.text = "Gold: " + gold.ToString() + "/" + maxGold.ToString();
    }

    public void disableAllUi()
    {
        endGameUi.SetActive(false);
        optionsUi.SetActive(false);
        inGameUi.SetActive(false);
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
    public void activateTroopScrollView()
    {
        client.toViewingState();
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
        client.toViewingState();
        troopScrollView.SetActive(false);
        if (towerScrollView.activeSelf)
        {
            towerScrollView.SetActive(false);
        }
        else
        {
            towerScrollView.SetActive(true);
        }
    }

    public void toDrawPathState()
    {
        client.toDrawPathState();
    }

    public void createTower(string towerName)
    {
        client.createTowerEvent(towerName);
    }

    public void createTroop(string troopName)
    {
        client.toDrawPathState();
        /*client.changeClientState("ViewingState");
        activateTroopMethodUi();*/
        client.createTroopEvent(troopName);
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
        networkManager.ServerChangeScene("LevelSelectScene");
    }

    public void displayTowerInfo(string type)
    {
        Dictionary<string, object> info = client.getTowerInfo(type);
        IDictionaryEnumerator enumerator = info.GetEnumerator();

        string result = "";
        while (enumerator.MoveNext())
        {
            string key = (string)enumerator.Key;
            object value = enumerator.Value;
            result += key + ": " + value.ToString() + "\n";
        }

        InfoPanel.displayInfo(info["Name"].ToString(), result);
    }

    public void displayInfo(Dictionary<string, object> info)
    {
        IDictionaryEnumerator enumerator = info.GetEnumerator();

        string result = "";
        while (enumerator.MoveNext())
        {
            string key = (string)enumerator.Key;
            object value = enumerator.Value;
            if(value == null) { value = "Nothing"; }
            result += key + ": " + value.ToString() + "\n";
        }

        InfoPanel.displayInfo(info["Name"].ToString(), result);
    }

    public void displayTroopInfo(string type)
    {
        Dictionary<string, object> info = client.getTroopInfo(type);
        IDictionaryEnumerator enumerator = info.GetEnumerator();

        string result = "";
        while (enumerator.MoveNext())
        {
            string key = (string)enumerator.Key;
            object value = enumerator.Value;
            result += key + ": " + value.ToString() + "\n";
        }

        InfoPanel.displayInfo(info["Name"].ToString(), result);
    }
}
