using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private int moveSpeed = 10;

    public float cameraMax_x = 0;
    public float cameraMax_y = 0;
    public float cameraMin_x = 0;
    public float cameraMin_y = 0;

    public bool bounded = false;
    public Vector2 bottomLeft;
    public Vector2 topRight;

    private Camera mainCamera;

    public bool cameraMovement = false;

    void Start()
    {
        mainCamera = this.GetComponent<Camera>();
    }

    void Update()
    {
        if (cameraMovement)
        {
            Vector3 movement = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.RightArrow))
            {
                movement.x += moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                movement.x -= moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                movement.y -= moveSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                movement.y += moveSpeed * Time.deltaTime;
            }
            transform.Translate(movement);
            if (bounded)
            {
                transform.position = new Vector3(Mathf.Clamp(transform.position.x, cameraMin_x, cameraMax_x), Mathf.Clamp(transform.position.y, cameraMin_y, cameraMax_y), transform.position.z);
            }
        }
    }

    public void setCameraBounds(Vector2 bottomLeft, Vector2 topRight)
    {
        this.bottomLeft = bottomLeft;
        this.topRight = topRight;
        createMaxZoom();
        bounded = true;
        refreshCameraBounds();
    }

    public void refreshCameraBounds()
    {
        if (!bounded) return;
        float vertExtent = mainCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;
        cameraMax_x = topRight.x - horzExtent;
        cameraMax_y = topRight.y - vertExtent;
        cameraMin_x = bottomLeft.x + horzExtent;
        cameraMin_y = bottomLeft.y + vertExtent;
    }

    private void createMaxZoom()
    {
        float distX = (this.topRight.x - this.bottomLeft.x) * 0.5f;
        float distY = (this.topRight.y - this.bottomLeft.y) * 0.5f;
        float dist = Mathf.Max(distX, distY);
        this.GetComponent<CameraZoom>().setMaxZoom(dist * Screen.height / Screen.width);
    }
}
