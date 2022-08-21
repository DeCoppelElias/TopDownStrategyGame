using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class LoadLevel : MonoBehaviour
{
    // Loading state
    private enum LoadingState { Idle, StartLoading, AskingLevelName, Loading, LoadingFinished, BackToIdle}
    private LoadingState loadingState = LoadingState.Idle;

    // Saving variables
    private int loadingStep = 0;
    private float loadingCooldown = 2;
    private float lastLoadingStep = 0;

    // UI
    private GameObject normalUi;
    private GameObject loadingLevelUi;

    private GameObject loadingLevel;
    private GameObject loadingLevelName;

    private TMP_Text loadingLevelInfo;
    private Slider loadingBar;
    private TMP_Text levelNameText;

    // Tilemaps
    private Tilemap wallTilemap;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;

    // Level name and if saved
    private string levelName = "";
    private bool levelNameSaved = false;

    private void Start()
    {
        // Tilemaps
        floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();

        // UI
        normalUi = GameObject.Find("NormalUi");
        loadingLevelUi = GameObject.Find("LoadingLevelUi");

        if(loadingLevelUi != null)
        {
            loadingLevel = loadingLevelUi.transform.Find("LoadingLevel").gameObject;
            loadingLevelName = loadingLevelUi.transform.Find("EnterLevelName").gameObject;

            loadingLevelInfo = loadingLevel.transform.Find("LoadingLevelInfo").GetComponent<TMP_Text>();
            loadingBar = loadingLevel.transform.Find("LoadingBar").GetComponent<Slider>();
            levelNameText = loadingLevelName.transform.Find("InputField").transform.Find("Text Area").transform.Find("Text").GetComponent<TMP_Text>();

            loadingLevelUi.SetActive(false);
        }
    }

    private void Update()
    {
        if (this.loadingState == LoadingState.Idle) return;
        loadLevelStep();
    }

    private void loadLevelStep()
    {
        if (loadingState == LoadingState.StartLoading)
        {
            normalUi.SetActive(false);
            loadingLevelUi.SetActive(true);

            loadingLevelName.SetActive(true);
            loadingLevel.SetActive(false);

            this.loadingState = LoadingState.AskingLevelName;
        }

        // Asking Level Name
        else if (loadingState == LoadingState.AskingLevelName)
        {
            if (levelNameSaved)
            {
                this.loadingLevel.SetActive(true);
                this.loadingLevelName.SetActive(false);

                this.loadingBar.value = 0;

                this.loadingStep = 0;

                this.loadingState = LoadingState.Loading;
            }
        }

        else if (Time.time - lastLoadingStep > loadingCooldown)
        {
            lastLoadingStep = Time.time;

            updateLoadingBar((float)loadingStep / (Enum.GetNames(typeof(LoadingState)).Length - 5));

            if (loadingState == LoadingState.Loading)
            {
                loadingLevelInfo.text = "Loading Level";
                loadLevel(this.levelName + ".txt");
                loadingState = LoadingState.LoadingFinished;

                loadingStep++;
            }

            else if (loadingState == LoadingState.LoadingFinished)
            {
                loadingLevelInfo.text = "Level is fully loaded";
                loadingState = LoadingState.BackToIdle;
                loadingStep++;
            }

            else if (loadingState == LoadingState.BackToIdle)
            {
                resetUi();
                loadingStep = 0;
                levelNameSaved = false;
                loadingState = LoadingState.Idle;
            }
        }
    }

    /// <summary>
    /// Will reset Ui to normal
    /// </summary>
    private void resetUi()
    {
        normalUi.SetActive(true);
        loadingLevelUi.SetActive(false);
        loadingLevelInfo.text = "";
    }

    /// <summary>
    /// Updates loading bar
    /// </summary>
    /// <param name="progress"></param>
    private void updateLoadingBar(float progress)
    {
        progress = Mathf.Clamp(progress, 0, 1);
        loadingBar.value = progress;
    }

    /// <summary>
    /// Will load level
    /// </summary>
    /// <param name="levelName"></param>
    public void loadLevel(string levelName, bool border = false)
    {
        string levelInfoString = getLevelInfoString(levelName);
        string s = levelInfoString.Replace("\r", "");
        string[] lines = s.Split("\n"[0]);

        loadLevel(lines, border);
    }
    public void loadLevel()
    {
        this.loadingState = LoadingState.StartLoading;
    }
    public void loadLevel(string[] lines, bool border = false)
    {
        resetLevel();

        // Castles
        GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/BuildablePrefabs/PlayerCastle");
        GameObject castles = GameObject.Find("Castles");
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

        if (!border) return;
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
        GameObject castlePrefab = (GameObject)Resources.Load("Prefabs/BuildablePrefabs/PlayerCastle");
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
            result.Add(groundTilemap, tuple1);
        }

        // Wall Tilemap
        Tilemap wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        foreach (string tileString in lines[5].Split('/'))
        {
            (Tile, Vector3Int) tuple2 = translateTileString(tileString);
            result.Add(wallTilemap, tuple2);
        }

        // Decoration Tilemap
        Tilemap decorationTileMap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        foreach (string tileString in lines[7].Split('/'))
        {
            (Tile, Vector3Int) tuple3 = translateTileString(tileString);
            result.Add(decorationTileMap, tuple3);
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
    private (Vector2, Vector2) findBorders()
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
        if (tileString == "") return;
        (Tile, Vector3Int) tuple = translateTileString(tileString);
        tilemap.SetTile(tuple.Item2, tuple.Item1);
    }

    /// <summary>
    /// Resets all tilemaps and deletes all gameobjects
    /// </summary>
    private void resetLevel()
    {
        this.floorTilemap.ClearAllTiles();
        this.wallTilemap.ClearAllTiles();
        this.decorationTilemap.ClearAllTiles();

        deleteCastles();
    }

    /// <summary>
    /// Translates a tilestring to a tile and position
    /// </summary>
    /// <param name="tileString"></param>
    /// <returns></returns>
    private (Tile, Vector3Int) translateTileString(string tileString)
    {
        if (tileString == "") throw new Exception("Not a valid tileString: " + tileString);
        try
        {
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
        catch (Exception e)
        {
            throw new Exception("Not a valid tileString: " + tileString);
        }
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

    /// <summary>
    /// Cancels the level saving process
    /// </summary>
    public void cancelSavingName()
    {
        this.loadingState = LoadingState.Idle;
        levelNameSaved = false;
        levelNameText.text = "";

        normalUi.SetActive(true);
        loadingLevelUi.SetActive(false);
    }

    /// <summary>
    /// Saves the level name
    /// </summary>
    public void saveLevelName()
    {
        if (levelNameText.text.Length == 0) return;
        if (!checkIfLevelExists(levelNameText.text + ".txt")) return;

        this.levelName = levelNameText.text;
        levelNameSaved = true;
    }

    public bool checkIfLevelExists(string levelName)
    {
        try
        {
            string s = File.ReadAllText(Application.persistentDataPath + "/Levels/" + levelName);
            return true;
        }
        catch
        {
            try
            {
                UnityEngine.Object officialLevelTxtFile = Resources.Load("Levels/" + levelName);
                TextAsset textAsset = (TextAsset)officialLevelTxtFile;
                string s = textAsset.text.Replace("\r", "");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
