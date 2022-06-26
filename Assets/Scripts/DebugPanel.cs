using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugPanel : MonoBehaviour
{
    private TextMeshProUGUI debugMessages;
    [SerializeField]
    private float textDecayTime = 2;
    [SerializeField]
    private float textStayActive = 2;
    private float startWait;

    [SerializeField]
    private float changeDecayInterval = 0.25f;
    private float decayPerInterval;
    private float lastDecay = 0f;
    private float currentDecay = 1;

    private bool waiting = false;
    private bool decaying = false;

    private void Start()
    {
        decayPerInterval = changeDecayInterval / textDecayTime;
        debugMessages = this.transform.Find("DebugMessages").GetComponent<TextMeshProUGUI>();
        this.Hide();
    }

    /// <summary>
    /// Will automatically fade out the text in the debug message text block
    /// </summary>
    private void Update()
    {
        if(waiting && Time.time - startWait > textStayActive)
        {
            decaying = true;
            waiting = false;
            lastDecay = Time.time;
        }
        if (decaying && Time.time - lastDecay > changeDecayInterval)
        {
            if (currentDecay <= 0)
            {
                resetDecay();
            }
            else
            {
                updateDecay();
            }
        }
    }

    private void setText(string debugText)
    {
        debugMessages.text = debugText;
    }
    private void updateDecay()
    {
        currentDecay -= decayPerInterval;
        lastDecay = Time.time;

        debugMessages.gameObject.GetComponent<CanvasRenderer>().SetAlpha(currentDecay);
    }

    private void startDecay()
    {
        resetDecay();
        waiting = true;
        startWait = Time.time;
        Show();
    }

    private void resetDecay()
    {
        currentDecay = 1f;
        debugMessages.gameObject.GetComponent<CanvasRenderer>().SetAlpha(currentDecay);
        waiting = false;
        decaying = false;
        Hide();
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

    /// <summary>
    /// This is an easy static way to update the debug message text block
    /// </summary>
    /// <param name="debugMessage"></param>
    public static void displayDebugMessage(string debugMessage)
    {
        GameObject.Find("DebugPanel").GetComponent<DebugPanel>().setText(debugMessage);
        GameObject.Find("DebugPanel").GetComponent<DebugPanel>().startDecay();
    }
}
