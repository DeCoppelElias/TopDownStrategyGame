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

    public List<Vector2> castlePositions = new List<Vector2>();
    private CameraMovement cameraMovement;

    private void Start()
    {
        cameraMovement = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        initLevel();
    }

    private void Update()
    {
        cameraMovement.setCameraBounds(length, height);
    }
    public void initLevel()
    {
        for (int x = -length/2; x <= length/2; x++)
        {
            obstacleTilemap.SetTile(new Vector3Int(x, height/2, 0), wallTile);
            obstacleTilemap.SetTile(new Vector3Int(x, -height/2, 0), wallTile);
        }
        for (int y = -height / 2; y <= height / 2; y++)
        {
            obstacleTilemap.SetTile(new Vector3Int(length/2, y, 0), wallTile);
            obstacleTilemap.SetTile(new Vector3Int(-length/2, y, 0), wallTile);
        }
        for (int x = -length / 2; x <= length / 2; x++)
        {
            for (int y = -height / 2; y <= height / 2; y++)
            {
                floorTileMap.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }

        cameraMovement.cameraMovement = true;
        cameraMovement.setCameraBounds(length, height);
    }
}
