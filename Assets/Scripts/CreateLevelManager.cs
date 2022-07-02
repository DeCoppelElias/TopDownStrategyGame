using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Mirror;
using UnityEngine.UI;

public class CreateLevelManager : MonoBehaviour
{
    private Tile selectedTile;
    private Tilemap selectedTileMap;

    private Tilemap floorTilemap;
    private Tilemap wallsTilemap;
    private Tilemap decorationTilemap;

    private TMP_Text selectedTileText;
    private TMP_Text selectedTilemapText;
    private void Start()
    {
        this.floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        this.wallsTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        this.decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        this.selectedTileText = GameObject.Find("Current Tile").GetComponent<TMP_Text>();
        this.selectedTilemapText = GameObject.Find("Current Tilemap").GetComponent<TMP_Text>();
        this.selectedTileMap = floorTilemap;
        this.selectErasor();

        GameObject tileScrollView = GameObject.Find("Tile Scroll View").transform.Find("Viewport").transform.Find("Content").gameObject;
        GameObject tileButtonPrefab = Resources.Load("Tiles/TileButton") as GameObject;
        foreach (UnityEngine.Object obj in Resources.LoadAll("Tiles"))
        {
            if(obj is Tile tile)
            {
                GameObject buttonGameobject = Instantiate(tileButtonPrefab, tileScrollView.transform);
                buttonGameobject.GetComponent<Image>().sprite = tile.sprite;
                buttonGameobject.GetComponent<Image>().color = tile.color;
                buttonGameobject.GetComponent<Button>().onClick.AddListener(delegate { setTile(tile); });
                GameObject tileNameGameObject = buttonGameobject.transform.Find("TileName").gameObject;
                tileNameGameObject.GetComponent<TMP_Text>().text = tile.name;
            }
        }

    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector3Int mousePosition = Vector3Int.FloorToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            mousePosition.z = 0;
            Debug.Log(selectedTileMap.GetTile(mousePosition));
            selectedTileMap.SetTile(mousePosition, selectedTile);
            Debug.Log(selectedTileMap.GetTile(mousePosition));
        }
    }
    public void saveLevel()
    {
        string levelString = "";

        // Castles
        levelString += "Castle Positions:\n";
        foreach(Castle castle in GameObject.Find("Castles").GetComponentsInChildren<Castle>())
        {
            levelString += castle.transform.position + "/";
        }
        levelString += "\n";

        // Ground TileMap
        levelString += "Ground Tile Positions: \n";
        Tilemap groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        foreach (Vector3Int position in groundTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = groundTilemap.GetTile(position);
            if (tile != null)
            {
                levelString += tile.name + ":" + position + "/";
            }
        }
        levelString += "\n";

        // Walls Tilemap
        levelString += "Wall Tile Positions: \n";
        Tilemap wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        foreach (Vector3Int position in wallTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = wallTilemap.GetTile(position);
            if (tile != null)
            {
                levelString += tile.name + ":" + position + "/";
            }
        }
        levelString += "\n";

        // Decoration Tilemap
        levelString += "Decoration Tile Positions: \n";
        Tilemap decorationTileMap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        foreach (Vector3Int position in decorationTileMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = decorationTileMap.GetTile(position);
            if (tile != null)
            {
                levelString += tile.name + ":" + position + "/";
            }
        }
        levelString += "\n";

        string levelName = GameObject.Find("SaveButtonInputField").GetComponent<TMP_InputField>().text;
        StreamWriter writer = new StreamWriter("D:/UnityProjects/TopDownStrategyGame/Assets/Levels/" + levelName + ".txt", false);
        writer.WriteLine(levelString);
        writer.Close();
    }

    public void loadLevel()
    {
        try
        {
            string levelName = GameObject.Find("LoadButtonInputField").GetComponent<TMP_InputField>().text;
            string[] lines = File.ReadAllLines("D:/UnityProjects/TopDownStrategyGame/Assets/Levels/" + levelName + ".txt");

            // Castles
            GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
            GameObject castles = GameObject.Find("Castles");
            foreach (Castle castle in castles.GetComponentsInChildren<Castle>())
            {
                NetworkServer.Destroy(castle.gameObject);
            }
            foreach (string positionString in lines[1].Split('/'))
            {
                if (positionString == "") break;
                string currentPositionString = positionString;

                // Remove the parentheses
                if (currentPositionString.StartsWith("(") && currentPositionString.EndsWith(")"))
                {
                    currentPositionString = currentPositionString.Substring(1, currentPositionString.Length - 2);
                }

                // split the items
                string[] sArray = currentPositionString.Split(',');

                // store as a Vector3
                Debug.Log(sArray);
                Vector3 castlePosition = new Vector3(
                    float.Parse(sArray[0]),
                    float.Parse(sArray[1]),
                    float.Parse(sArray[2]));

                Instantiate(castlePrefab, castlePosition, Quaternion.identity, castles.transform);
            }

            // Ground Tilemap
            Tilemap groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
            groundTilemap.ClearAllTiles();
            foreach (string tileString in lines[3].Split('/'))
            {
                spawnTile(tileString, groundTilemap);
            }

            // Wall Tilemap
            Tilemap wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
            wallTilemap.ClearAllTiles();
            foreach (string tileString in lines[5].Split('/'))
            {
                spawnTile(tileString, wallTilemap);
            }

            // Decoration Tilemap
            Tilemap decorationTileMap = GameObject.Find("Decoration").GetComponent<Tilemap>();
            decorationTileMap.ClearAllTiles();
            foreach (string tileString in lines[7].Split('/'))
            {
                spawnTile(tileString, decorationTileMap);
            }
        }
        catch(Exception e)
        {
            Debug.Log(e);
            Debug.Log(e.StackTrace);
            return;
        }
    }

    private void spawnTile(string tileString, Tilemap tilemap)
    {
        if (tileString == "") return;
        string[] name_position = tileString.Split(':');
        string tileName = name_position[0];
        string tilePositionString = name_position[1];

        // Remove the parentheses
        if (tilePositionString.StartsWith("(") && tilePositionString.EndsWith(")"))
        {
            tilePositionString = tilePositionString.Substring(1, tilePositionString.Length - 2);
        }

        // split the items
        string[] sArray = tilePositionString.Split(',');

        // store as a Vector3
        Vector3Int tilePosition = new Vector3Int(
            int.Parse(sArray[0]),
            int.Parse(sArray[1]),
            int.Parse(sArray[2]));

        Tile tile = (Tile)Resources.Load("Tiles/" + tileName);
        tilemap.SetTile(tilePosition, tile);
    }

    public void setTile(Tile tile)
    {
        this.selectedTile = tile;
        this.selectedTileText.text = "Tile: " + tile.name;
    }

    public void selectFloor()
    {
        this.selectedTileMap = floorTilemap;
        this.selectedTilemapText.text = "Tilemap: " + floorTilemap.name;
    }

    public void selectWalls()
    {
        this.selectedTileMap = wallsTilemap;
        this.selectedTilemapText.text = "Tilemap: " + wallsTilemap.name;
    }

    public void selectDecoration()
    {
        this.selectedTileMap = decorationTilemap;
        this.selectedTilemapText.text = "Tilemap: " + decorationTilemap.name;
    }

    public void selectErasor()
    {
        this.selectedTile = null;
        this.selectedTileText.text = "Tile: Nothing";
    }
}
