using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    private bool _boundedMovement = false;
    public bool BoundedMovement { get => _boundedMovement; set => _boundedMovement = value; }
    [SerializeField]
    private bool _boundedZoom = false;
    public bool BoundedZoom { get => _boundedZoom; set => _boundedZoom = value; }

    [SerializeField]
    private bool _movableCamera = false;
    public bool MovableCamera { get => _movableCamera; set => _movableCamera = value; }
    [SerializeField]
    private bool _zoomableCamera = false;
    public bool ZoomableCamera { get => _zoomableCamera; set => _zoomableCamera = value; }

    private float targetZoom = 20;
    [SerializeField]
    private float zoomSpeed = 5;
    [SerializeField]
    private float minZoom = 1;
    [SerializeField]
    private float maxZoom = 40;


    [SerializeField]
    private int cameraMoveSpeed = 10;

    private float cameraMax_x = 0;
    private float cameraMax_y = 0;
    private float cameraMin_x = 0;
    private float cameraMin_y = 0;

    private Vector2 bottomLeft;
    private Vector2 topRight;

    void Update()
    {
        if (MovableCamera)
        {
            moveCamera();
        }

        if (ZoomableCamera)
        {
            zoomCamera();
        }
    }

    /// <summary>
    /// Will check current level and setup the camera bounds and max zoom
    /// </summary>
    public void setupCameraBounds()
    {
        if (BoundedMovement) setCameraBounds();
        targetZoom = maxZoom;
    }

    /// <summary>
    /// Handles moving the camera
    /// </summary>
    private void moveCamera()
    {
        // Move speed changes is lower when zoomed in
        int currentMoveSpeed = cameraMoveSpeed;
        if (BoundedZoom) currentMoveSpeed = (int)(currentMoveSpeed * (targetZoom / maxZoom));
        currentMoveSpeed = Mathf.Clamp(currentMoveSpeed, (int)(cameraMoveSpeed / 1.5f), cameraMoveSpeed);

        // Get movement
        Vector3 movement = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movement.x += currentMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x -= currentMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            movement.y -= currentMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            movement.y += currentMoveSpeed * Time.deltaTime;
        }

        // Move Camera
        transform.Translate(movement);

        // Clamp value when bounded
        if (BoundedMovement)
        {
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, cameraMin_x, cameraMax_x), Mathf.Clamp(transform.position.y, cameraMin_y, cameraMax_y), transform.position.z);
        }
    }

    /// <summary>
    /// Handles zooming the camera
    /// </summary>
    private void zoomCamera()
    {
        // If over Ui element don't zoom
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Check if scrolling
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scrollData * zoomSpeed;

        // If bounded, clamp value
        if(BoundedZoom) targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        if (targetZoom < 0) targetZoom = 0;

        // Change zoom
        setZoomSubtle(targetZoom);
    }

    /// <summary>
    /// Find borders by checking wall, floor and decoration tilemap
    /// </summary>
    /// <returns></returns>
    private (Vector2, Vector2) findBorders()
    {
        Tilemap wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        Tilemap floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        Tilemap decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();

        Vector2 wallBottomLeft = wallTilemap.localBounds.center - wallTilemap.localBounds.extents;
        Vector2 wallTopRight = wallTilemap.localBounds.center + wallTilemap.localBounds.extents;

        Vector2 floorBottomLeft = floorTilemap.localBounds.center - floorTilemap.localBounds.extents;
        Vector2 floorTopRight = floorTilemap.localBounds.center + floorTilemap.localBounds.extents;

        Vector2 decoBottomLeft = decorationTilemap.localBounds.center - decorationTilemap.localBounds.extents;
        Vector2 decoTopRight = decorationTilemap.localBounds.center + decorationTilemap.localBounds.extents;

        int minX = (int)Math.Floor(Math.Min(Math.Min(wallBottomLeft.x, floorBottomLeft.x), decoBottomLeft.x));
        int minY = (int)Math.Floor(Math.Min(Math.Min(wallBottomLeft.y, floorBottomLeft.y), decoBottomLeft.y));

        int maxX = (int)Math.Ceiling(Math.Max(Math.Max(wallTopRight.x, floorTopRight.x), decoTopRight.x));
        int maxY = (int)Math.Ceiling(Math.Max(Math.Max(wallTopRight.y, floorTopRight.y), decoTopRight.y));

        Vector2 bottomLeft = new Vector2(minX - 1, minY - 1);
        Vector2 topRight = new Vector2(maxX, maxY);

        return (bottomLeft, topRight);
    }

    /// <summary>
    /// sets camera bounds
    /// </summary>
    private void setCameraBounds()
    {
        // Finding the edges of the level and setting bottomLeft and topRight
        (Vector2, Vector2) tuple = findBorders();
        this.bottomLeft = tuple.Item1;
        this.topRight = tuple.Item2;

        createMaxZoom();
        refreshCameraBounds();
    }

    /// <summary>
    /// Checks the current zoom and changes bounds based on that
    /// </summary>
    public void refreshCameraBounds()
    {
        if (!BoundedMovement) return;
        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;
        cameraMax_x = topRight.x - horzExtent - 1;
        cameraMax_y = topRight.y - vertExtent - 1;
        cameraMin_x = bottomLeft.x + horzExtent + 2;
        cameraMin_y = bottomLeft.y + vertExtent + 2;
    }

    /// <summary>
    /// Finds the max amound of zoom
    /// </summary>
    private void createMaxZoom()
    {
        float distX = (this.topRight.x - this.bottomLeft.x) * 0.5f;
        float distY = (this.topRight.y - this.bottomLeft.y) * 0.5f;
        float dist = Mathf.Max(distX, distY);
        float maxZoom = (dist * Screen.height / Screen.width) - 5;
        setMaxZoom(maxZoom);
    }

    /// <summary>
    /// Sets max zoom
    /// </summary>
    /// <param name="zoom"></param>
    public void setMaxZoom(float zoom)
    {
        Camera.main.orthographicSize = zoom;
        this.maxZoom = zoom;
        refreshCameraBounds();
    }

    /// <summary>
    /// Will change the zoom sublte every frame
    /// </summary>
    /// <param name="zoom"></param>
    private void setZoomSubtle(float zoom)
    {
        targetZoom = zoom;
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, zoom, Time.deltaTime * 5);
        refreshCameraBounds();
    }

    /// <summary>
    /// Will immedially set zoom
    /// </summary>
    /// <param name="zoom"></param>
    public void setZoom(float zoom)
    {
        targetZoom = zoom;
        Camera.main.orthographicSize = zoom;
        refreshCameraBounds();
    }
}
