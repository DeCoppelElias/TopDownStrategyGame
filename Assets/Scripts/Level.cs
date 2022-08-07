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
    }

    private void loadLevel()
    {
        this.GetComponent<SaveLoadLevel>().loadLevel("Level-" + Level.level);
    }
}
