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
    private int width = 150;
    private int height = 150;

    Vector3 oldPosition;
    float oldOrthographicSize;

    private Tilemap wallTilemap;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;

    private GameObject normalUi;
    private GameObject savingLevelUi;
    private TMP_Text savingLevelInfo;

    private StreamWriter writer;
    private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    private void Start()
    {
        normalUi = GameObject.Find("NormalUi");
        savingLevelUi = GameObject.Find("SavingLevelUi");
        if(savingLevelUi != null)
        {
            savingLevelUi.SetActive(false);
            savingLevelInfo = savingLevelUi.transform.Find("SavingLevelInfo").GetComponent<TMP_Text>();
        }

        floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
    }
    private void Update()
    {
        StartCoroutine(saveLevelStep());
    }

    /// <summary>
    /// Will save in an amount of steps
    /// </summary>
    /// <returns></returns>
    private IEnumerator saveLevelStep()
    {
        if (saving && Time.time - lastSavingStep > savingCooldown)
        {
            lastSavingStep = Time.time;

            // Wait till the last possible moment before screen rendering to hide the UI
            yield return null;

            // Wait for screen rendering to complete
            yield return frameEnd;

            string levelString = "";
            if (savingStep == 0)
            {
                savingLevelInfo.text = "Starting to save level";

                // Castles
                levelString += "Castle Positions:\n";
                foreach (Castle castle in GameObject.Find("Castles").GetComponentsInChildren<Castle>())
                {
                    levelString += castle.transform.position + "/";
                }

                writer.WriteLine(levelString);
                savingLevelInfo.text = "Castle positions have been saved";

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
                savingLevelInfo.text = "Ground Tilemap has been saved";

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
                savingLevelInfo.text = "Walls Tilemap has been saved";

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
                savingLevelInfo.text = "Decoration Tilemap has been saved";

                savingStep += 1;
            }
            else if (savingStep == 4)
            {
                // Adjust camera position for screenshot
                // old camera values
                oldPosition = Camera.main.transform.position;
                oldOrthographicSize = Camera.main.orthographicSize;

                (Vector2, Vector2) tuple = findBorders();
                Vector2 bottomLeft = tuple.Item1;
                Vector2 topRight = tuple.Item2;

                float levelHeight = topRight.y - bottomLeft.y;
                float levelWidth = topRight.x - bottomLeft.x;

                float newOrthographicSizeNormalScreen = 0;
                float newOrthographicSizeAdjusted = 0;

                if (levelHeight > levelWidth)
                {
                    newOrthographicSizeNormalScreen = levelHeight / 2;
                    newOrthographicSizeAdjusted = newOrthographicSizeNormalScreen * ((float)Screen.height / height);
                }
                else
                {
                    newOrthographicSizeNormalScreen = (levelWidth / 2) * ((float)Screen.height / Screen.width);
                    newOrthographicSizeAdjusted = newOrthographicSizeNormalScreen * ((float)Screen.width / width);
                }

                CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
                cameraMovement.ZoomableCamera = false;
                cameraMovement.MovableCamera = false;
                cameraMovement.setZoom(newOrthographicSizeAdjusted);

                float vertExtent = Camera.main.orthographicSize;
                float horzExtent = vertExtent * Screen.width / Screen.height;

                Vector3 newCameraPosition = new Vector3(bottomLeft.x + horzExtent, bottomLeft.y + vertExtent, Camera.main.transform.position.z);
                Camera.main.transform.position = newCameraPosition;


                // Disable Ui
                this.savingLevelUi.SetActive(false);

                savingLevelInfo.text = "Adjusted camera for screenshot";

                savingStep += 1;
            }
            else if (savingStep == 5)
            {

                // PNG image of level for level selection
                levelString += "PNG: \n";

                // Create a texture the size of the screen, RGB24 format
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

                // Read screen contents into the texture
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);

                // Enable Ui again
                this.savingLevelUi.SetActive(true);

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

                /*CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
                cameraMovement.ZoomableCamera = true;
                cameraMovement.MovableCamera = true;*/

                Camera.main.transform.position = oldPosition;
                Camera.main.orthographicSize = oldOrthographicSize;

                savingLevelInfo.text = "Level is fully saved";

                saving = false;
                savingStep = 0;
                resetUi();
            }
        }
    }

    /// <summary>
    /// Will reset Ui to normal
    /// </summary>
    public void resetUi()
    {
        normalUi.SetActive(true);
        savingLevelUi.SetActive(false);
        savingLevelInfo.text = "";
    }
    
    /// <summary>
    /// Will save level
    /// </summary>
    /// <param name="levelName"></param>
    public void saveLevel(string levelName)
    {
        normalUi.SetActive(false);
        savingLevelUi.SetActive(true);

        saving = true;
        writer = new StreamWriter(Application.persistentDataPath + "/Levels/" + levelName + ".txt", false);
    }

    /// <summary>
    /// Will load level
    /// </summary>
    /// <param name="levelName"></param>
    public void loadLevel(string levelName)
    {
        string levelInfoString = getLevelInfoString(levelName);
        string s = levelInfoString.Replace("\r", "");
        string[] lines = s.Split("\n"[0]);

        loadLevel(lines);
    }
    public void loadLevel()
    {
        string levelName = inputField.text;
        loadLevel(levelName);
    }
    public void loadLevel(string[] lines)
    {
        // Castles
        GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
        GameObject castles = GameObject.Find("Castles");
        deleteCastles();
        foreach (string positionString in lines[1].Split('/'))
        {
            if (positionString == "" || positionString == "\r") break;
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

        // Building a wall around the map
        Tile wallTile = getTile("Wall Tile");
        buildLevelEdgeWall(wallTile);
    }

    /// <summary>
    /// Will load all server gameobjects of level
    /// </summary>
    /// <param name="levelName"></param>
    public void loadLevelServer(string levelName)
    {
        string s = getLevelInfoString(levelName).Replace("\r", "");
        string[] lines = s.Split("\n"[0]);

        // Castles
        GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
        GameObject castles = GameObject.Find("Castles");
        deleteCastles();
        foreach (string positionString in lines[1].Split('/'))
        {
            if (positionString == "" || positionString == "\r") break;
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
    }

    /// <summary>
    /// Will load all tiles and set camera
    /// </summary>
    /// <param name="levelInfo"></param>
    public void loadLevelClient(string levelInfo)
    {
        string s = levelInfo.Replace("\r", "");
        string[] lines = s.Split("\n"[0]);

        // Ground Tilemap
        Tilemap groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        groundTilemap.ClearAllTiles();
        string[] groundTileStrings = lines[3].Split('/');
        foreach (string tileString in groundTileStrings)
        {
            if (tileString == "") continue;
            spawnTile(tileString, groundTilemap);
        }

        // Wall Tilemap
        Tilemap wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        wallTilemap.ClearAllTiles();
        foreach (string tileString in lines[5].Split('/'))
        {
            if (tileString == "") continue;
            spawnTile(tileString, wallTilemap);
        }

        // Decoration Tilemap
        Tilemap decorationTileMap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        decorationTileMap.ClearAllTiles();
        foreach (string tileString in lines[7].Split('/'))
        {
            if (tileString == "") continue;
            spawnTile(tileString, decorationTileMap);
        }

        // Building a wall around the map
        Tile wallTile = getTile("Wall Tile");
        buildLevelEdgeWall(wallTile);

        CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
        if (cameraMovement)
        {
            cameraMovement.setupCameraBounds();
        }
    }

    /// <summary>
    /// Gets a string representing the level
    /// </summary>
    /// <param name="levelName"></param>
    /// <returns></returns>
    public string getLevelInfoString(string levelName)
    {
        try
        {
            string s = File.ReadAllText(Application.persistentDataPath + "/Levels/" + levelName);
            return s;
        }
        catch
        {
            UnityEngine.Object officialLevelTxtFile = Resources.Load("Levels/" + levelName);
            TextAsset textAsset = (TextAsset)officialLevelTxtFile;
            string s = textAsset.text.Replace("\r", "");
            return s;
        }
    }

    /// <summary>
    /// Creates a dict that stores info of the level
    /// </summary>
    /// <param name="levelName"></param>
    /// <returns></returns>
    public Dictionary<Tilemap, (Tile, Vector3Int)> getLevelInfoClient(string levelName)
    {
        Dictionary<Tilemap, (Tile, Vector3Int)> result = new Dictionary<Tilemap, (Tile, Vector3Int)>();

        string levelInfoString = getLevelInfoString(levelName);
        string s = levelInfoString.Replace("\r", "");
        string[] lines = s.Split("\n"[0]);

        // Ground Tilemap
        Tilemap groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        foreach (string tileString in lines[3].Split('/'))
        {
            (Tile, Vector3Int) tuple1 = translateTileString(tileString);
            result.Add(groundTilemap,tuple1);
        }

        // Wall Tilemap
        Tilemap wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        foreach (string tileString in lines[5].Split('/'))
        {
            (Tile, Vector3Int) tuple2 = translateTileString(tileString);
            result.Add(wallTilemap,tuple2);
        }

        // Decoration Tilemap
        Tilemap decorationTileMap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        foreach (string tileString in lines[7].Split('/'))
        {
            (Tile, Vector3Int) tuple3 = translateTileString(tileString);
            result.Add(decorationTileMap,tuple3);
        }

        // Wall Around the map
        Tile wallTile = getTile("WallTile");
        (Vector2, Vector2) tuple = findBorders();
        Vector2 bottomLeft = tuple.Item1;
        Vector2 topRight = tuple.Item2;

        for (int x = (int)bottomLeft.x; x <= topRight.x; x++)
        {
            result.Add(wallTilemap, (wallTile, new Vector3Int(x, (int)bottomLeft.y, 0)));
            result.Add(wallTilemap, (wallTile, new Vector3Int(x, (int)topRight.y, 0)));
        }
        for (int y = (int)bottomLeft.y; y <= topRight.y; y++)
        {
            result.Add(wallTilemap, (wallTile, new Vector3Int((int)bottomLeft.x, y, 0)));
            result.Add(wallTilemap, (wallTile, new Vector3Int((int)topRight.x, y, 0)));
        }

        return result;
    }

    /// <summary>
    /// Builds a wall around the edges of the level
    /// </summary>
    /// <param name="wallTile"></param>
    private void buildLevelEdgeWall(Tile wallTile)
    {
        (Vector2, Vector2) tuple = findBorders();
        Vector2 bottomLeft = tuple.Item1;
        Vector2 topRight = tuple.Item2;

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

    /// <summary>
    /// Finds the borders of the level by checking the floor, wall and decoration tilemap
    /// </summary>
    /// <returns></returns>
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
        Vector2 topRight = new Vector2(maxX, maxY);

        return (bottomLeft, topRight);
    }

    /// <summary>
    /// Spawns a tile
    /// </summary>
    /// <param name="tileString"></param>
    /// <param name="tilemap"></param>
    private void spawnTile(string tileString, Tilemap tilemap)
    {
        (Tile, Vector3Int) tuple = translateTileString(tileString);
        tilemap.SetTile(tuple.Item2, tuple.Item1);
    }

    /// <summary>
    /// Translates a tilestring to a tile and position
    /// </summary>
    /// <param name="tileString"></param>
    /// <returns></returns>
    private (Tile, Vector3Int) translateTileString(string tileString)
    {
        if (tileString == "") throw new Exception("Not a valid tileString");
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
        return (tile, tilePosition);
    }

    /// <summary>
    /// Gets a tile from resources
    /// </summary>
    /// <param name="tileName"></param>
    /// <returns></returns>
    private Tile getTile(string tileName)
    {
        return (Tile)Resources.Load("Tiles/BuildableTiles/" + tileName);
    }

    /// <summary>
    /// Deletes all castles
    /// </summary>
    private void deleteCastles()
    {
        GameObject castles = GameObject.Find("Castles");
        foreach (Castle castle in castles.GetComponentsInChildren<Castle>())
        {
            NetworkServer.Destroy(castle.gameObject);
        }
    }
}
