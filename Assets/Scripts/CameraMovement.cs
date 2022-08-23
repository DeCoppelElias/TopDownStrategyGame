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

    private bool setup = false;


    [SerializeField]
    private int cameraMoveSpeed = 10;

    [SerializeField]
    private float cameraMax_x = 0;
    [SerializeField]
    private float cameraMax_y = 0;
    [SerializeField]
    private float cameraMin_x = 0;
    [SerializeField]
    private float cameraMin_y = 0;

    private Vector2 bottomLeft;
    private Vector2 topRight;

    void Update()
    {
        if((BoundedMovement && setup) || !BoundedMovement)
        {
            if (MovableCamera)
            {
                moveCamera();
            }
        }
        
        if((BoundedZoom && setup) || !BoundedZoom)
        {
            if (ZoomableCamera)
            {
                zoomCamera();
            }
        }
    }

    /// <summary>
    /// Will check current level and setup the camera bounds and max zoom
    /// </summary>
    public void setupCameraBounds()
    {
        if (BoundedMovement)
        {
            // Finding borders of level and storing them
            setCameraBounds();

            // Creating the max possible zoom without showing empty space
            createMaxZoom();
        }
        setup = true;
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
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            movement.x += currentMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            movement.x -= currentMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            movement.y -= currentMoveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
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
        // Move camera to target zoom
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetZoom, Time.deltaTime * 5);

        // If over Ui element don't take input
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Check if scrolling
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scrollData * zoomSpeed;

        // If bounded, clamp value
        if(BoundedZoom) targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        if (targetZoom < 0) targetZoom = 0;

        // Change zoom
        if(scrollData != 0) setZoomSubtle(targetZoom);
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
        int maxX = (int)Math.Floor(Math.Max(Math.Max(wallTopRight.x, floorTopRight.x), decoTopRight.x));
        if(minX + 2 <= maxX)
        {
            minX += 1;
            maxX -= 1;
        }

        int minY = (int)Math.Floor(Math.Min(Math.Min(wallBottomLeft.y, floorBottomLeft.y), decoBottomLeft.y));
        int maxY = (int)Math.Floor(Math.Max(Math.Max(wallTopRight.y, floorTopRight.y), decoTopRight.y));
        if (minY + 2 <= maxY)
        {
            minY += 1;
            maxY -= 1;
        }

        Vector2 bottomLeft = new Vector2(minX, minY);
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
    }

    /// <summary>
    /// Checks the current zoom and changes bounds based on that
    /// </summary>
    public void refreshCameraBounds()
    {
        if (!BoundedMovement) return;
        float vertExtent = Camera.main.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;
        cameraMax_x = topRight.x - horzExtent;
        cameraMax_y = topRight.y - vertExtent;
        cameraMin_x = bottomLeft.x + horzExtent;
        cameraMin_y = bottomLeft.y + vertExtent;

        if(Math.Abs(cameraMax_x - cameraMin_x) < 0.5)
        {
            float middle = (cameraMax_x + cameraMin_x) * 0.5f;
            cameraMax_x = middle;
            cameraMin_x = middle;
        }
        if (Math.Abs(cameraMax_y - cameraMin_y) < 0.5)
        {
            float middle = (cameraMax_y + cameraMin_y) * 0.5f;
            cameraMax_y = middle;
            cameraMin_y = middle;
        }
    }

    /// <summary>
    /// Finds the max amound of zoom
    /// </summary>
    private void createMaxZoom()
    {
        float levelWidth = (float)(this.topRight.x - this.bottomLeft.x) * 0.5f;
        float levelHeight = (float)(this.topRight.y - this.bottomLeft.y) * 0.5f;

        float zoomX = levelWidth * (float)Screen.height / Screen.width;
        float zoomY = levelHeight;

        float maxZoom = Mathf.Min(zoomX, zoomY);
        setMaxZoom(maxZoom);
    }

    /// <summary>
    /// Sets max zoom
    /// </summary>
    /// <param name="zoom"></param>
    public void setMaxZoom(float zoom)
    {
        this.maxZoom = zoom;
        setZoom(zoom);
    }

    /// <summary>
    /// Will change the zoom sublte every frame
    /// </summary>
    /// <param name="zoom"></param>
    private void setZoomSubtle(float zoom)
    {
        targetZoom = zoom;
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
