using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
public class Level : NetworkBehaviour
{
    public Tilemap obstacleTilemap;
    public Tilemap floorTileMap;
    public int maxPlayers;
    public int length;
    public int height;

    public GameObject castlePrefab;
    public Tile wallTile;
    public Tile floorTile;

    private CameraMovement cameraMovement;

    private void Start()
    {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
    }
    public void initLevel()
    {
        //Debug.Log("Setting Camera Bounds");
        cameraMovement.cameraMovement = true;
        cameraMovement.setCameraBounds(length, height);
        buildLevelEdgeWall();
    }

    private void buildLevelEdgeWall()
    {
        for (int x = -length / 2; x <= length / 2; x++)
        {
            obstacleTilemap.SetTile(new Vector3Int(x, height / 2, 0), wallTile);
            obstacleTilemap.SetTile(new Vector3Int(x, -height / 2, 0), wallTile);
        }
        for (int y = -height / 2; y <= height / 2; y++)
        {
            obstacleTilemap.SetTile(new Vector3Int(length / 2, y, 0), wallTile);
            obstacleTilemap.SetTile(new Vector3Int(-length / 2, y, 0), wallTile);
        }
    }
}
