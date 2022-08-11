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

    private SaveLoadLevel saveLoadLevel;

    private void Start()
    {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        floorTileMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        saveLoadLevel = this.GetComponent<SaveLoadLevel>();
    }

    

    [Server]
    public void initLevelServer()
    {
        saveLoadLevel.loadLevelServer(levelName);
    }

    public string getLevelInfo()
    {
        return saveLoadLevel.getLevelInfoString(levelName);
    }

    public void addPartOfLevelInfo(string levelInfoPart)
    {
        this.levelInfo += levelInfoPart;
    }

    public void initLevelClient()
    {
        if (levelInfo == null) return;
        saveLoadLevel = this.GetComponent<SaveLoadLevel>();
        saveLoadLevel.loadLevelClient(levelInfo);
    }
}
