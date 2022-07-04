using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using System;

public class Level : NetworkBehaviour
{
    public static int level = 0;

    private Tilemap wallTilemap;
    private Tilemap floorTileMap;
    private Tilemap decorationTilemap;

    public int maxPlayers;

    private Vector2 bottomLeft;
    private Vector2 topRight;

    public GameObject castlePrefab;
    public Tile wallTile;
    public Tile floorTile;

    private CameraMovement cameraMovement;

    private void Start()
    {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        floorTileMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
    }
    public void initLevel()
    {
        loadLevel();
        Invoke("findBorders", 0.1f);
        Invoke("buildLevelEdgeWall", 0.2f);
        Invoke("setupCamera", 0.3f);
    }

    private void buildLevelEdgeWall()
    {
        for (int x = (int)bottomLeft.x; x <= topRight.x; x++)
        {
            wallTilemap.SetTile(new Vector3Int(x, (int)bottomLeft.y, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int(x, (int)topRight.y, 0), wallTile);
        }
        for (int y = (int)bottomLeft.y; y <= topRight.y; y++)
        {
            wallTilemap.SetTile(new Vector3Int((int)bottomLeft.x, y, 0), wallTile);
            wallTilemap.SetTile(new Vector3Int((int)topRight.x, y, 0), wallTile);
        }
    }

    private void findBorders()
    {
        Vector2 wallBottomLeft = wallTilemap.localBounds.center - wallTilemap.localBounds.extents;
        Vector2 wallTopRight = wallTilemap.localBounds.center + wallTilemap.localBounds.extents;

        Vector2 floorBottomLeft = floorTileMap.localBounds.center - floorTileMap.localBounds.extents;
        Vector2 floorTopRight = floorTileMap.localBounds.center + floorTileMap.localBounds.extents;

        Vector2 decoBottomLeft = decorationTilemap.localBounds.center - decorationTilemap.localBounds.extents;
        Vector2 decoTopRight = decorationTilemap.localBounds.center + decorationTilemap.localBounds.extents;

        bottomLeft = Vector2.Min(Vector2.Min(wallBottomLeft, floorBottomLeft), decoBottomLeft);
        topRight = Vector2.Max(Vector2.Max(wallTopRight, floorTopRight), decoTopRight);
    }

    private void loadLevel()
    {
        this.GetComponent<SaveLoadLevel>().loadLevel("Level-" + Level.level);
    }

    private void setupCamera()
    {
        cameraMovement.cameraMovement = true;
        cameraMovement.setCameraBounds(bottomLeft, topRight);
    }
}
