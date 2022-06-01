using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelSceneUi : MonoBehaviour
{
    private Client client;
    private GameObject inGameUi;
    private GameObject optionsUi;
    private TextMeshProUGUI goldDisplay;
    void Start()
    {
        inGameUi = GameObject.Find("InGameUi");
        goldDisplay = GameObject.Find("GoldDisplay").GetComponent<TextMeshProUGUI>();
        optionsUi = GameObject.Find("OptionsUi");
        optionsUi.SetActive(false);
        inGameUi.SetActive(false);
    }

    public void displayGold(int gold)
    {
        goldDisplay.text = gold.ToString();
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

    public void setClient(Client client)
    {
        this.client = client;
    }
    public void createTroop(string troopName)
    {
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
}
