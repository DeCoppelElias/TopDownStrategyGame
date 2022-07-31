using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SaveLoadLevel : NetworkBehaviour
{
    public InputField inputField;

    private bool saving = false;
    private int savingStep = 0;
    private float savingCooldown = 2;
    private float lastSavingStep = 0;

    // Width and height of screenshot starting at bottom
    private int width = 100;
    private int height = 100;

    Vector3 oldPosition;
    float oldOrthographicSize;

    private Tilemap wallTilemap;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;

    private Canvas canvas;
    private StreamWriter writer;
    private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    private void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
    }
    private void Update()
    {
        StartCoroutine(saveLevelStep());
    }

    private IEnumerator saveLevelStep()
    {
        if (saving && Time.time - lastSavingStep > savingCooldown)
        {
            lastSavingStep = Time.time;

            // Wait till the last possible moment before screen rendering to hide the UI
            yield return null;
            canvas.enabled = false;

            // Wait for screen rendering to complete
            yield return frameEnd;

            string levelString = "";
            if (savingStep == 0)
            {
                Debug.Log("Starting to save level");

                // Castles
                levelString += "Castle Positions:\n";
                foreach (Castle castle in GameObject.Find("Castles").GetComponentsInChildren<Castle>())
                {
                    levelString += castle.transform.position + "/";
                }

                writer.WriteLine(levelString);
                Debug.Log("Castle positions have been saved");

                savingStep += 1;
            }
            else if (savingStep == 1)
            {
                // Ground TileMap
                levelString += "Ground Tile Positions: \n";
                foreach (Vector3Int position in floorTilemap.cellBounds.allPositionsWithin)
                {
                    TileBase tile = floorTilemap.GetTile(position);
                    if (tile != null)
                    {
                        levelString += tile.name + ":" + position + "/";
                    }
                }

                writer.WriteLine(levelString);
                Debug.Log("Ground Tilemap has been saved");

                savingStep += 1;
            }
            else if (savingStep == 2)
            {
                // Walls Tilemap
                levelString += "Wall Tile Positions: \n";
                foreach (Vector3Int position in wallTilemap.cellBounds.allPositionsWithin)
                {
                    TileBase tile = wallTilemap.GetTile(position);
                    if (tile != null)
                    {
                        levelString += tile.name + ":" + position + "/";
                    }
                }

                writer.WriteLine(levelString);
                Debug.Log("Walls Tilemap has been saved");

                savingStep += 1;
            }
            else if (savingStep == 3)
            {
                // Decoration Tilemap
                levelString += "Decoration Tile Positions: \n";
                foreach (Vector3Int position in decorationTilemap.cellBounds.allPositionsWithin)
                {
                    TileBase tile = decorationTilemap.GetTile(position);
                    if (tile != null)
                    {
                        levelString += tile.name + ":" + position + "/";
                    }
                }
                levelString += "\n";

                writer.WriteLine(levelString);
                Debug.Log("Decoration Tilemap has been saved");

                savingStep += 1;
            }
            else if (savingStep == 4)
            {
                // PNG image of level for level selection
                levelString += "PNG: ";
                // Create a texture the size of the screen, RGB24 format

                // Adjust camera position for screenshot
                // old camera values
                oldPosition = Camera.main.transform.position;
                oldOrthographicSize = Camera.main.orthographicSize;

                (Vector2, Vector2) tuple = findBorders();
                Vector2 bottomLeft = tuple.Item1;
                Vector2 topRight = tuple.Item2;

                float newOrthographicSizeNormalScreen = (topRight.y - bottomLeft.y) / 2;
                float newOrthographicSizeAdjusted = newOrthographicSizeNormalScreen * (Screen.height / height);
                CameraZoom cameraZoom = Camera.main.GetComponent<CameraZoom>();
                cameraZoom.setMaxZoom(newOrthographicSizeAdjusted);
                cameraZoom.setZoom(newOrthographicSizeAdjusted);

                float vertExtent = Camera.main.orthographicSize;
                float horzExtent = vertExtent * Screen.width / Screen.height;

                Vector3 newCameraPosition = new Vector3(bottomLeft.x + horzExtent, bottomLeft.y + vertExtent, Camera.main.transform.position.z);
                Camera.main.transform.position = newCameraPosition;

                writer.WriteLine(levelString);
                Debug.Log("Adjusted camera for screenshot");

                savingStep += 1;
            }
            else if (savingStep == 5)
            {
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

                // Read screen contents into the texture
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

                Camera.main.transform.position = oldPosition;
                Camera.main.GetComponent<CameraZoom>().setMaxZoom(oldOrthographicSize);

                // Encode texture into PNG
                byte[] bytes = tex.EncodeToPNG();
                Destroy(tex);

                levelString += "width: " + tex.width + "\n";
                levelString += "height: " + tex.height + "\n";

                foreach (byte b in bytes)
                {
                    levelString += b + " ";
                }
                levelString += "\n";

                writer.WriteLine(levelString);
                writer.Close();

                canvas.enabled = true;
                Debug.Log("Level is fully saved");

                saving = false;
                savingStep = 0;
            }
        }
    }
    
    public void saveLevel(string levelName)
    {
        saving = true;
        writer = new StreamWriter(Application.persistentDataPath + "/Levels/" + levelName + ".txt", false);
    }

    public void loadLevel(string levelName)
    {
        string[] lines;
        try
        {
            lines = File.ReadAllLines(Application.persistentDataPath + "/Levels/" + levelName + ".txt");
        }
        catch
        {
            lines = File.ReadAllLines(Application.dataPath + "/Levels/" + levelName + ".txt");
        }

        // Castles
        GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
        GameObject castles = GameObject.Find("Castles");
        deleteCastles();
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
            Vector3 castlePosition = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

            GameObject castle = Instantiate(castlePrefab, castlePosition, Quaternion.identity, castles.transform);
            NetworkServer.Spawn(castle);
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

    public void loadLevel()
    {
        string[] lines;

        string levelName = inputField.text;

        try
        {
            lines = File.ReadAllLines(Application.persistentDataPath + "/Levels/" + levelName + ".txt");
        }
        catch
        {
            lines = File.ReadAllLines(Application.dataPath + "/Levels/" + levelName + ".txt");
        }

        // Castles
        GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
        GameObject castles = GameObject.Find("Castles");
        deleteCastles();
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
            Vector3 castlePosition = new Vector3(
                float.Parse(sArray[0]),
                float.Parse(sArray[1]),
                float.Parse(sArray[2]));

            GameObject castle = Instantiate(castlePrefab, castlePosition, Quaternion.identity, castles.transform);
            NetworkServer.Spawn(castle);
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

        Tile tile = (Tile)Resources.Load("Tiles/BuildableTiles/" + tileName);
        tilemap.SetTile(tilePosition, tile);
    }

    private void deleteCastles()
    {
        GameObject castles = GameObject.Find("Castles");
        foreach (Castle castle in castles.GetComponentsInChildren<Castle>())
        {
            NetworkServer.Destroy(castle.gameObject);
        }
    }

    private (Vector2,Vector2) findBorders()
    {
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
        Vector2 topRight = new Vector2(maxX + 1, maxY + 1);

        return (bottomLeft, topRight);
    }
}
