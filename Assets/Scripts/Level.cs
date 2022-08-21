using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using System;

public class Level : NetworkBehaviour
{
    public static string levelName = "";

    private Tilemap wallTilemap;
    private Tilemap floorTileMap;
    private Tilemap decorationTilemap;

    private string levelInfo = "";

    public int maxPlayers;

    public GameObject castlePrefab;
    public Tile wallTile;
    public Tile floorTile;

    private CameraMovement cameraMovement;

    private LoadLevel loadLevel;

    private void Start()
    {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        floorTileMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        loadLevel = this.GetComponent<LoadLevel>();
    }

    

    [Server]
    public void initLevelServer()
    {
        loadLevel.loadLevelServer(levelName);
    }

    public string getLevelInfo()
    {
        return loadLevel.getLevelInfoString(levelName);
    }

    public void addPartOfLevelInfo(string levelInfoPart)
    {
        this.levelInfo += levelInfoPart;
    }

    public void initLevelClient()
    {
        if (levelInfo == null) return;
        loadLevel = this.GetComponent<LoadLevel>();
        loadLevel.loadLevelClient(levelInfo);
    }
}
