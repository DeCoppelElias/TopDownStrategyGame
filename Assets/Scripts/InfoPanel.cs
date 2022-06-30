using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoPanel : MonoBehaviour
{
    private float lastShown = 0;
    private float hideCooldown = 1;
    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time > lastShown + hideCooldown)
        {
            this.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = "";
            this.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = "";
            Hide();
        }
    }
    public static void displayInfo(string name, string info)
    {
        GameObject infoPanel = GameObject.Find("InfoPanel");
        infoPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;
        infoPanel.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = info;
        infoPanel.GetComponent<InfoPanel>().Show();
        infoPanel.GetComponent<InfoPanel>().lastShown = Time.time;
    }

    private void Hide()
    {
        this.GetComponent<CanvasGroup>().alpha = 0f; //this makes everything transparent
        this.GetComponent<CanvasGroup>().blocksRaycasts = false; //this prevents the UI element to receive input events
    }

    private void Show()
    {
        this.GetComponent<CanvasGroup>().alpha = 1f;
        this.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
