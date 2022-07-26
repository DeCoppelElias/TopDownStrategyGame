using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SaveLoadLevel : MonoBehaviour
{
    private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
    public IEnumerator saveLevel(string levelName)
    {
        // Wait till the last possible moment before screen rendering to hide the UI
        yield return null;
        GameObject.Find("Canvas").GetComponent<Canvas>().enabled = false;

        // Wait for screen rendering to complete
        yield return frameEnd;

        string levelString = "";

        // Castles
        levelString += "Castle Positions:\n";
        foreach (Castle castle in GameObject.Find("Castles").GetComponentsInChildren<Castle>())
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
        levelString += "\n\n";

        // PNG image of level for level selection
        levelString += "PNG: \n";
        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;

        Debug.Log(width);
        Debug.Log(height);

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        GameObject.Find("Canvas").GetComponent<Canvas>().enabled = true;

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Debug.Log(bytes.Length);
        Destroy(tex);

        levelString += "width: " + tex.width + "\n";
        levelString += "height: " + tex.height + "\n";

        foreach (byte b in bytes)
        {
            levelString += b + " ";
        }
        levelString += "\n";

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/Levels/" + levelName + ".txt", false);
        writer.WriteLine(levelString);
        writer.Close();
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
}
