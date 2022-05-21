using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    private Client client;
    private GameObject inGameUi;
    private GameObject selectLevelUi;
    void Start()
    {
        inGameUi = GameObject.Find("InGameUi");
        inGameUi.SetActive(false);
        selectLevelUi = GameObject.Find("SelectLevelUi");
        selectLevelUi.SetActive(false);
    }

    public void activateSelectLevelUi(bool active)
    {
        selectLevelUi.SetActive(active);
    }
    public void activateInGameUi(bool active)
    {
        inGameUi.SetActive(active);
    }

    public void selectLevelEvent(int level)
    {
        client.selectLevelEvent(level);
        activateSelectLevelUi(false);
        activateInGameUi(true);
    }

    public void setClient(Client client)
    {
        this.client = client;
    }
    public void createSwordMan()
    {
        client.createTroopEvent("SwordManTroop");
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
