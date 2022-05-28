using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiManager : MonoBehaviour
{
    private Client client;
    private GameObject inGameUi;
    private TextMeshProUGUI goldDisplay;
    void Start()
    {
        inGameUi = GameObject.Find("InGameUi");
        goldDisplay = GameObject.Find("GoldDisplay").GetComponent<TextMeshProUGUI>();
        inGameUi.SetActive(false);
    }

    public void displayGold(int gold)
    {
        goldDisplay.text = gold.ToString();
    }
    public void activateInGameUi(bool active)
    {
        inGameUi.SetActive(active);
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
}
