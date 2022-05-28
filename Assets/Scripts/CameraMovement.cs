using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public int speed = 1;
    private float targetZoom;
    private float zoomFactor = 3f;
    private float lerpSpeed = 10;
    public float cameraMax_x;
    public float cameraMax_y;
    private Camera mainCamera;

    public bool cameraMovement = false;

    private void Start()
    {
        mainCamera = this.GetComponent<Camera>();
        targetZoom = mainCamera.orthographicSize;
    }
    void Update()
    {
        if (cameraMovement)
        {
            Vector3 movement = new Vector3(0,0,0);
            if (Input.GetKey(KeyCode.RightArrow))
            {
                movement.x += speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                movement.x -= speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                movement.y -= speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                movement.y += speed * Time.deltaTime;
            }
            transform.Translate(movement);

            if (!(cameraMax_x == 0 && cameraMax_y == 0))
            {
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, -cameraMax_x, cameraMax_x), Mathf.Clamp(transform.position.y, -cameraMax_y, cameraMax_y), transform.position.z);
            }


            float scrollData = Input.GetAxis("Mouse ScrollWheel");
            targetZoom -= scrollData * zoomFactor;
            targetZoom = Mathf.Clamp(targetZoom, 4.5f, 15f);
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetZoom, Time.deltaTime * lerpSpeed);
        }
    }

    public void setCameraBounds(int length, int height)
    {
        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;
        cameraMax_x = (length / 2) - horzExtent;
        cameraMax_y = (height / 2) - vertExtent;
    }
}
