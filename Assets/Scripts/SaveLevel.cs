using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SaveLevel : MonoBehaviour
{
    // Saving State
    private enum SavingState { Idle, StartSaving, AskingLevelName, SavingCastles, SavingFloor, SavingWalls, SavingDecoration, AdjustingCamera, TakingScreenShot, SavingFinished, BackToIdle }
    private SavingState savingState = SavingState.Idle;

    // Saving variables
    private int savingStep = 0;
    private float savingCooldown = 2;
    private float lastSavingStep = 0;

    // Width and height of screenshot
    private int width = 300;
    private int height = 300;
    
    // Camera Position
    private Vector3 oldCameraPosition;
    private float oldCameraOrthographicSize;

    // Tilemaps
    private Tilemap wallTilemap;
    private Tilemap floorTilemap;
    private Tilemap decorationTilemap;

    // UI
    private GameObject normalUi;
    private GameObject savingLevelUi;

    private GameObject savingLevel;
    private GameObject savingLevelName;

    private TMP_Text savingLevelInfo;
    private Slider loadingBar;
    private TMP_Text levelNameText;

    // Level name and if saved
    private string levelName = "";
    private bool levelNameSaved = false;

    // Writer
    private StreamWriter writer;

    // Frame end
    private WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    // Observers
    private List<SaveLevelObserver> observers = new List<SaveLevelObserver>();

    private void Start()
    {
        // UI
        normalUi = GameObject.Find("NormalUi");
        savingLevelUi = GameObject.Find("SavingLevelUi");

        savingLevel = savingLevelUi.transform.Find("SavingLevel").gameObject;
        savingLevelName = savingLevelUi.transform.Find("EnterLevelName").gameObject;

        savingLevelInfo = savingLevel.transform.Find("SavingLevelInfo").GetComponent<TMP_Text>();
        loadingBar = savingLevel.transform.Find("LoadingBar").GetComponent<Slider>();
        levelNameText = savingLevelName.transform.Find("InputField").transform.Find("Text Area").transform.Find("Text").GetComponent<TMP_Text>();

        savingLevelUi.SetActive(false);

        // Tilemaps
        floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        wallTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
    }
    private void Update()
    {
        if (this.savingState == SavingState.Idle) return;
        StartCoroutine(saveLevelStep());
    }

    /// <summary>
    /// Will save in an amount of steps
    /// </summary>
    /// <returns></returns>
    private IEnumerator saveLevelStep()
    {
        // Start Saving
        if (savingState == SavingState.StartSaving)
        {
            normalUi.SetActive(false);
            savingLevelUi.SetActive(true);

            savingLevelName.SetActive(true);
            savingLevel.SetActive(false);

            this.savingState = SavingState.AskingLevelName;
        }

        // Asking Level Name
        else if (savingState == SavingState.AskingLevelName)
        {
            if (levelNameSaved)
            {
                this.savingLevel.SetActive(true);
                this.savingLevelName.SetActive(false);

                this.loadingBar.value = 0;

                this.savingStep = 0;

                if (!Directory.Exists(Application.persistentDataPath + "/Levels/"))
                {
                    //if it doesn't, create it
                    Directory.CreateDirectory(Application.persistentDataPath + "/Levels/");

                }
                writer = new StreamWriter(Application.persistentDataPath + "/Levels/" + levelName + ".txt", false);

                this.savingState = SavingState.SavingCastles;
            }
        }
        else if (Time.time - lastSavingStep > savingCooldown)
        {
            lastSavingStep = Time.time;

            updateLoadingBar((float)savingStep / (Enum.GetNames(typeof(SavingState)).Length - 5));

            // Wait till the last possible moment before screen rendering to hide the UI
            yield return null;

            // Wait for screen rendering to complete
            yield return frameEnd;

            string levelString = "";

            if (savingState == SavingState.SavingCastles)
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
                this.savingState = SavingState.SavingFloor;
            }

            else if (savingState == SavingState.SavingFloor)
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
                this.savingState = SavingState.SavingWalls;
            }

            else if (savingState == SavingState.SavingWalls)
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
                this.savingState = SavingState.SavingDecoration;
            }

            else if (savingState == SavingState.SavingDecoration)
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
                this.savingState = SavingState.AdjustingCamera;
            }

            else if (savingState == SavingState.AdjustingCamera)
            {
                // Adjust camera position for screenshot
                // old camera values
                oldCameraPosition = Camera.main.transform.position;
                oldCameraOrthographicSize = Camera.main.orthographicSize;

                (Vector2, Vector2) tuple = findBorders();
                Vector2 bottomLeft = tuple.Item1;
                Vector2 topRight = tuple.Item2;

                float levelHeight = topRight.y - bottomLeft.y;
                float levelWidth = topRight.x - bottomLeft.x;


                float newOrthographicSizeNormalScreenWidth = levelHeight / 2;
                float newOrthographicSizeAdjustedWidth = newOrthographicSizeNormalScreenWidth * ((float)Screen.height / height);

                float newOrthographicSizeNormalScreenHeight = (levelWidth / 2) * ((float)Screen.height / Screen.width);
                float newOrthographicSizeAdjustedHeight = newOrthographicSizeNormalScreenHeight * ((float)Screen.width / width);

                float newOrthographicSizeAdjusted = Mathf.Min(newOrthographicSizeAdjustedWidth, newOrthographicSizeAdjustedHeight);

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
                this.savingState = SavingState.TakingScreenShot;
            }

            else if (savingState == SavingState.TakingScreenShot)
            {
                savingLevelInfo.text = "Screenshot done";

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

                Camera.main.transform.position = oldCameraPosition;
                Camera.main.orthographicSize = oldCameraOrthographicSize;
                CameraMovement cameraMovement = Camera.main.GetComponent<CameraMovement>();
                cameraMovement.ZoomableCamera = true;
                cameraMovement.MovableCamera = true;

                savingStep++;
                this.savingState = SavingState.SavingFinished;
            }

            else if (savingState == SavingState.SavingFinished)
            {
                savingLevelInfo.text = "Level is fully saved";
                savingState = SavingState.BackToIdle;
                savingStep++;
            }

            else if (savingState == SavingState.BackToIdle)
            {
                notifyObservers();
                resetUi();
                savingStep = 0;
                levelNameSaved = false;
                savingState = SavingState.Idle;
            }
        }
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
    /// Will reset Ui to normal
    /// </summary>
    private void resetUi()
    {
        normalUi.SetActive(true);
        savingLevelUi.SetActive(false);
        savingLevelInfo.text = "";
    }

    /// <summary>
    /// Will save level
    /// </summary>
    /// <param name="levelName"></param>
    public void saveLevel()
    {
        this.savingState = SavingState.StartSaving;
    }
    public void saveLevel(string levelName)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Cancels the level saving process
    /// </summary>
    public void cancelSavingName()
    {
        this.savingState = SavingState.Idle;
        levelNameSaved = false;
        levelNameText.text = "";

        normalUi.SetActive(true);
        savingLevelUi.SetActive(false);

        notifyObservers();
    }

    /// <summary>
    /// Saves the level name
    /// </summary>
    public void saveLevelName()
    {
        if (levelNameText.text.Length == 0) return;
        this.levelName = levelNameText.text;
        levelNameSaved = true;
    }

    /// <summary>
    /// Add to observers
    /// </summary>
    public void subscribe(SaveLevelObserver observer)
    {
        this.observers.Add(observer);
    }

    private void notifyObservers()
    {
        foreach(SaveLevelObserver observer in observers)
        {
            observer.notify();
        }
    }
}
