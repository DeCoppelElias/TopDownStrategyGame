using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MouseInfoManager : MonoBehaviour
{
    GameObject currentGameObject;

    GameObject mouseInfoGameObject;
    TMP_Text mouseInfoText;

    public Vector3 offset = new Vector3(0, 0, 0);

    private void Start()
    {
        mouseInfoGameObject = GameObject.Find("MousePanelInfo");
        mouseInfoText = mouseInfoGameObject.GetComponentInChildren<TMP_Text>();
        mouseInfoGameObject.SetActive(false);
    }
    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            List<RaycastResult> eventSystemRaysastResults = GetEventSystemRaycastResults();
            for (int index = 0; index < eventSystemRaysastResults.Count; index++)
            {
                RaycastResult curRaysastResult = eventSystemRaysastResults[index];

                GameObject gameObject = curRaysastResult.gameObject;

                RectTransform rectTransform = mouseInfoGameObject.GetComponent<RectTransform>();
                this.mouseInfoGameObject.transform.position = Input.mousePosition + new Vector3(rectTransform.sizeDelta.x / 2, 0, 0) + offset;

                if (gameObject == this.currentGameObject) return;

                this.currentGameObject = gameObject;
                MouseInfo mouseInfo = gameObject.GetComponent<MouseInfo>();

                if (mouseInfo != null)
                {
                    display(mouseInfo.info);
                }
                else
                {
                    this.mouseInfoGameObject.SetActive(false);
                }
            }
        }
        else this.mouseInfoGameObject.SetActive(false);
    }

    private void display(string info)
    {
        mouseInfoText.text = info;
        this.mouseInfoGameObject.SetActive(true);
    }

    private List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
