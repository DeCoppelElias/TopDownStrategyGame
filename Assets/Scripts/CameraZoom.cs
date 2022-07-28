using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CameraZoom : MonoBehaviour
{
    private float targetZoom = 20;
    [SerializeField]
    private float zoomSpeed = 5;
    [SerializeField]
    private float minZoom = 1;
    [SerializeField]
    private float maxZoom = 40;


    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            float scrollData = Input.GetAxis("Mouse ScrollWheel");
            targetZoom -= scrollData * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            setZoomSubtle(targetZoom);

            /*var scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheelInput != 0)
            {
                currentZoom += Mathf.RoundToInt(scrollWheelInput * 10);
                if(currentZoom < 1)
                {

                }
                currentZoom = Mathf.Clamp(currentZoom, 1, 5);
                PixelPerfectCamera pixelPerfectCamera = this.GetComponent<PixelPerfectCamera>();
                pixelPerfectCamera.refResolutionX = Mathf.FloorToInt(Screen.width / currentZoom);
                pixelPerfectCamera.refResolutionY = Mathf.FloorToInt(Screen.height / currentZoom);
            }*/
        }
    }

    public void setMaxZoom(float zoom)
    {
        Camera.main.orthographicSize = zoom;
        this.maxZoom = zoom;
        this.GetComponent<CameraMovement>().refreshCameraBounds();
    }

    public void setZoomSubtle(float zoom)
    {
        targetZoom = zoom;
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, zoom, Time.deltaTime * 5);
        this.GetComponent<CameraMovement>().refreshCameraBounds();
    }

    public void setZoom(float zoom)
    {
        targetZoom = zoom;
        Camera.main.orthographicSize = zoom;
        this.GetComponent<CameraMovement>().refreshCameraBounds();
    }
}
