using Mirror;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static AVL;

public class CreateLevelManager : MonoBehaviour
{
    // Selected objects
    private Tile selectedTile;
    private Tilemap selectedTileMap;
    private GameObject selectedGameObject;

    // Tilemaps
    private Tilemap floorTilemap;
    private Tilemap wallsTilemap;
    private Tilemap decorationTilemap;
    private Tilemap previewTilemap;
    private Tilemap selectingTilemap;

    // Scrollviews
    private GameObject tileScrollView;
    private GameObject gameObjectScrollView;

    // others
    private GameObject selectingGameObject;
    private SelectingTile customSelectingTile;

    // Current building state
    private enum BuildingState { BuildingTiles, BuildingGameObjects, Viewing, Selecting, Paste, DrawingLine, DrawingRectangle, Fill}
    private BuildingState buildingState = BuildingState.Viewing;

    // Main Ui Elements
    private GameObject buildingTilesUi;
    private GameObject buildingGameObjectsUi;
    private GameObject basicUi;
    private GameObject infoPanel;
    private GameObject normalUi;

    // Basic Ui Elements
    private GameObject mainButtons;
    private GameObject levelOptions;

    // Infopanel Elements
    private TMP_Text buildingStateInfo;
    private TMP_Text firstLine;
    private TMP_Text secondLine;
    private GameObject buildingInfo;

    // Storing building size
    private int buildingSize = 1;

    // Preview for empty tile
    [SerializeField]
    private Tile emptyTilePreview;

    // Storing the selected tiles information
    private SelectedTiles selectedTiles;
    private class SelectedTiles
    {
        public List<SelectedTileGroup> tileGroups;
        public Vector3Int bottomLeft;
        public Vector3Int topRight;

        public SelectedTiles(List<SelectedTileGroup> tileGroups, Vector3Int bottomLeft, Vector3Int topRight)
        {
            this.tileGroups = tileGroups;
            this.bottomLeft = bottomLeft;
            this.topRight = topRight;
        }
    }
    private class SelectedTileGroup
    {
        public SelectedTileGroup(List<SelectedTileInfo> tilesInfo, Vector3Int position)
        {
            this.tilesInfo = tilesInfo;
            this.position = position;

            if (tilesInfo.Count == 0) return;
            frontTile = tilesInfo[0];
            foreach (SelectedTileInfo tileInfo in tilesInfo)
            {
                int oldval = SortingLayer.GetLayerValueFromID(frontTile.tilemap.GetComponent<TilemapRenderer>().sortingLayerID);
                int newval = SortingLayer.GetLayerValueFromID(tileInfo.tilemap.GetComponent<TilemapRenderer>().sortingLayerID);
                if (oldval < newval)
                {
                    frontTile = tileInfo;
                }
            }
        }

        public List<SelectedTileInfo> tilesInfo;
        public SelectedTileInfo frontTile;
        public Vector3Int position;
    }
    private class SelectedTileInfo
    {
        public SelectedTileInfo(Tile tile, Tilemap tilemap)
        {
            this.tile = tile;
            this.tilemap = tilemap;
        }

        public Tile tile;
        public Tilemap tilemap;
    }

    private Vector2Int startSelectingPoint;
    private Vector2Int endSelectingPoint;
    private void Start()
    {
        // Creating the selecting Gameobject for the build gameobject state
        this.selectingGameObject = Instantiate((GameObject)Resources.Load("Prefabs/SelectingGameObject"), new Vector3(0, 0, 0), Quaternion.identity);
        this.selectingGameObject.SetActive(false);

        // Finding and storing the custom selecting tile
        this.customSelectingTile = (SelectingTile)Resources.Load("Tiles/SelectingTile");

        // Finding tile scrollwheels
        this.tileScrollView = GameObject.Find("TileScrollView");
        this.gameObjectScrollView = GameObject.Find("GameObjectScrollView");

        // Finding and clearing all tilemaps
        this.selectingTilemap = GameObject.Find("Selecting").GetComponent<Tilemap>();
        this.selectingTilemap.ClearAllTiles();
        this.floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        this.floorTilemap.ClearAllTiles();
        this.wallsTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        this.wallsTilemap.ClearAllTiles();
        this.decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        this.decorationTilemap.ClearAllTiles();
        this.previewTilemap = GameObject.Find("Preview").GetComponent<Tilemap>();
        this.previewTilemap.ClearAllTiles();

        // Finding Main Ui Elements and deactivating when neccessary
        this.basicUi = GameObject.Find("BasicUi");
        this.buildingTilesUi = GameObject.Find("BuildingTilesUi");
        this.buildingTilesUi.SetActive(false);
        this.buildingGameObjectsUi = GameObject.Find("BuildingGameObjectsUi");
        this.buildingGameObjectsUi.SetActive(false);
        this.infoPanel = GameObject.Find("InfoPanel");
        this.infoPanel.SetActive(false);
        this.normalUi = GameObject.Find("NormalUi");

        // Finding Basic Ui Elements
        this.mainButtons = basicUi.transform.Find("Main Buttons").gameObject;
        this.levelOptions = basicUi.transform.Find("Level Options").gameObject;
        resetBasicUi();

        // Finding InfoPanel Elements
        this.buildingInfo = infoPanel.transform.Find("BuildingInfo").gameObject;

        this.buildingStateInfo = infoPanel.transform.Find("BuildingState").GetComponent<TMP_Text>();
        this.firstLine = buildingInfo.transform.Find("First Line").GetComponent<TMP_Text>();
        this.secondLine = buildingInfo.transform.Find("Second Line").GetComponent<TMP_Text>();

        // Setting standard tilemap and tile
        this.selectFloor();
        this.setTile(null);

        // Creating buttons for selecting tiles to build
        GameObject buttonPrefab = Resources.Load("Prefabs/Button") as GameObject;
        foreach (UnityEngine.Object obj in Resources.LoadAll("Tiles/BuildableTiles"))
        {
            if(obj is Tile tile)
            {
                createTileButton(tile, buttonPrefab);
            }
        }

        // Creating buttons for selecting gameobjects to build
        GameObject castleGameObject = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
        createGameObjectButton(castleGameObject, buttonPrefab);
    }

    private void Update()
    {
        if (buildingState == BuildingState.Paste)
        {
            pasteState();
        }
        else if (buildingState == BuildingState.Selecting)
        {
            selectingState();
        }
        else if (buildingState == BuildingState.BuildingTiles)
        {
            buildingTilesState();
        }
        else if (buildingState == BuildingState.BuildingGameObjects)
        {
            buildingGameObjectsState();
        }
        else if (buildingState == BuildingState.DrawingLine)
        {
            drawingLineState();
        }
        else if (buildingState == BuildingState.DrawingRectangle)
        {
            drawingRectangleState();
        }
        else if (buildingState == BuildingState.Fill)
        {
            fillState();
        }
    }

    private void fillState()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePositionInt = Vector3Int.FloorToInt(mousePosition);

        // Check if exit state
        if (Input.GetMouseButtonDown(1))
        {
            toBuildTileState();
        }

        // Store start point
        if (Input.GetMouseButtonDown(0))
        {
            fill(this.selectedTile, this.selectedTileMap, mousePositionInt);
        }
    }

    private class FillNode : Node
    {
        public Vector3Int startPosition;
        public FillNode(Vector3Int startPosition, Vector3Int tilePosition) : base(tilePosition)
        {
            this.startPosition = startPosition;
        }
        public override object Clone()
        {
            return new FillNode(startPosition, this.tilePosition);
        }
        public override float getCost()
        {
            return Vector3Int.Distance(this.startPosition, this.tilePosition);
        }

        public override int GetHashCode()
        {
            return this.tilePosition.x * 1000000 + tilePosition.y;
        }
    }

    private void fill(Tile selectedTile, Tilemap tilemap, Vector3Int position)
    {
        AVL avl = new AVL();
        Dictionary<Vector3Int, string> checkedPositions = new Dictionary<Vector3Int, string>();
        Dictionary<Vector3Int, string> checkPositions = new Dictionary<Vector3Int, string>();

        FillNode startFillNode = new FillNode(position, position);
        checkPositions.Add(position,"");
        avl.Add(startFillNode);

        Tile clickedTile = (Tile)tilemap.GetTile(position);

        int counter = 0;
        int maxCounter = 100000;
        while(!avl.isEmpty() && checkPositions.Count > 0 && counter < maxCounter)
        {
            FillNode currentfillNode = (FillNode)avl.PopMinValue();
            checkPositions.Remove(currentfillNode.tilePosition);

            checkedPositions.Add(currentfillNode.tilePosition, "");
            tilemap.SetTile(currentfillNode.tilePosition, selectedTile);

            List<Vector3Int> neighbors = getNeighbours(clickedTile, tilemap, currentfillNode.tilePosition);
            foreach(Vector3Int neighbor in neighbors)
            {
                if (!checkedPositions.ContainsKey(neighbor) && !checkPositions.ContainsKey(neighbor))
                {
                    FillNode fillNode = new FillNode(position, neighbor);
                    checkPositions.Add(neighbor, "");
                    avl.Add(fillNode);
                }
            }

            counter++;
        }
    }

    private List<Vector3Int> getNeighbours(Tile tile, Tilemap tilemap, Vector3Int position)
    {
        List<Vector3Int> neighbours = new List<Vector3Int>() { 
            new Vector3Int(position.x+1,position.y,position.z),
            new Vector3Int(position.x-1, position.y, position.z),
            new Vector3Int(position.x, position.y+1, position.z),
            new Vector3Int(position.x,position.y-1,position.z)};

        List<Vector3Int> result = new List<Vector3Int>();
        foreach (Vector3Int neighbor in neighbours)
        {
            Tile neighborTile = (Tile)tilemap.GetTile(neighbor);
            if (neighborTile == tile)
            {
                result.Add(neighbor);
            }
        }
        return result;
    }

    private void drawingRectangleState()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePositionInt = Vector3Int.FloorToInt(mousePosition);

        createSelectingGrid(mousePositionInt);
        createPreviewPoint(mousePositionInt);

        // Check if exit state
        if (Input.GetMouseButtonDown(1))
        {
            toBuildTileState();
        }

        // Store start point
        if (Input.GetMouseButtonDown(0))
        {
            this.startSelectingPoint = Vector2Int.FloorToInt(mousePosition);
        }

        // While holding create preview
        if (Input.GetMouseButton(0))
        {
            createPreviewRectangle(this.startSelectingPoint, mousePosition);
        }

        // When released store end point and draw result
        if (Input.GetMouseButtonUp(0))
        {
            this.endSelectingPoint = Vector2Int.FloorToInt(mousePosition);
            drawRectangle(this.startSelectingPoint, this.endSelectingPoint, this.selectedTileMap, this.selectedTile, false);
        }
    }
    private void drawingLineState()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePositionInt = Vector3Int.FloorToInt(mousePosition);

        createSelectingGrid(mousePositionInt);
        createPreviewPoint(mousePositionInt);

        // Check if exit state
        if (Input.GetMouseButtonDown(1))
        {
            toBuildTileState();
        }

        // Store start point
        if (Input.GetMouseButtonDown(0))
        {
            this.startSelectingPoint = Vector2Int.FloorToInt(mousePosition);
        }
        
        // While holding create preview
        if (Input.GetMouseButton(0))
        {
            createPreviewLine(this.startSelectingPoint, mousePosition);
        }

        // When released store end point and draw result
        if (Input.GetMouseButtonUp(0))
        {
            this.endSelectingPoint = Vector2Int.FloorToInt(mousePosition);
            drawLine(this.startSelectingPoint, this.endSelectingPoint, this.selectedTileMap, this.selectedTile, false);
        }
    }

    private void createPreviewPoint(Vector3Int mousePositionInt)
    {
        if(this.selectedTile == null)
        {
            setTileFull(mousePositionInt, this.emptyTilePreview, this.previewTilemap, true);
        }
        else
        {
            setTileFull(mousePositionInt, this.selectedTile, this.previewTilemap, true);
        }
    }
    private void createPreviewLine(Vector2 start, Vector2 finish)
    {
        if (this.selectedTile == null)
        {
            drawLine(start, finish, this.previewTilemap, this.emptyTilePreview, true);
        }
        else
        {
            drawLine(start, finish, this.previewTilemap, this.selectedTile, true);
        }
    }
    private void createPreviewRectangle(Vector2 start, Vector2 finish)
    {
        if (this.selectedTile == null)
        {
            drawRectangle(start, finish, this.previewTilemap, this.emptyTilePreview, true);
        }
        else
        {
            drawRectangle(start, finish, this.previewTilemap, this.selectedTile, true);
        }
    }

    private void drawRectangle(Vector2 s, Vector2 f, Tilemap tilemap, Tile tile, bool clear)
    {
        if (clear) tilemap.ClearAllTiles();

        Vector3 s3 = new Vector3(s.x, s.y, 0);
        Vector3 f3 = new Vector3(f.x, f.y, 0);

        Vector3Int start = Vector3Int.FloorToInt(s3);
        Vector3Int finish = Vector3Int.FloorToInt(f3);

        int minY = Mathf.Min(start.y, finish.y);
        int maxY = Mathf.Max(start.y, finish.y);
        int minX = Mathf.Min(start.x, finish.x);
        int maxX = Mathf.Max(start.x, finish.x);

        for (int x = minX; x <= maxX; x++)
        {
            setTileFull(new Vector3Int(x, minY, 0), tile, tilemap, false);
            setTileFull(new Vector3Int(x, maxY, 0), tile, tilemap, false);
        }
        for (int y = minY; y <= maxY; y++)
        {
            setTileFull(new Vector3Int(minX, y, 0), tile, tilemap, false);
            setTileFull(new Vector3Int(maxX, y, 0), tile, tilemap, false);
        }
    }
    private void drawLine(Vector2 s, Vector2 f, Tilemap tilemap, Tile tile, bool clear)
    {
        if (clear) tilemap.ClearAllTiles();

        Vector3 s3 = new Vector3(s.x, s.y, 0);
        Vector3 f3 = new Vector3(f.x, f.y, 0);

        Vector3Int start = Vector3Int.FloorToInt(s3);
        Vector3Int finish = Vector3Int.FloorToInt(f3);

        // Vertical line
        if (finish.x == start.x)
        {
            int minY = Mathf.Min(start.y, finish.y);
            int maxY = Mathf.Max(start.y, finish.y);
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int position = new Vector3Int(finish.x, y, 0);
                setTileFull(position, tile, tilemap, false);
            }
        }

        // Horizontal line
        else if (finish.y == start.y)
        {
            int minX = Mathf.Min(start.x, finish.x);
            int maxX = Mathf.Max(start.x, finish.x);
            for (int x = minX; x <= maxX; x++)
            {
                Vector3Int position = new Vector3Int(x, finish.y, 0);
                setTileFull(position, tile, tilemap, false);
            }
        }

        else
        {
            // Calculating rico
            float rico = ((float)(finish.y - start.y)) / (finish.x - start.x);

            // y = (rico * (x - start.x)) + start.y

            int minX = Mathf.Min(start.x, finish.x);
            int maxX = Mathf.Max(start.x, finish.x);

            for (int x = minX; x <= maxX; x++)
            {
                int y = (int)((rico * (x - start.x)) + start.y);
                Vector3Int position = new Vector3Int(x, y, 0);
                setTileFull(position, tile, tilemap, false);
            }

            int minY = Mathf.Min(start.y, finish.y);
            int maxY = Mathf.Max(start.y, finish.y);

            for (int y = minY; y <= maxY; y++)
            {
                int x = (int)(((y - start.y) / rico) + start.x);

                Vector3Int position = new Vector3Int(x, y, 0);
                setTileFull(position, tile, tilemap, false);
            }
        }
    }

    /// <summary>
    /// Update actions in the paste state
    /// </summary>
    private void pasteState()
    {
        this.createSelectedTilesPreview();
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            placeSelectedTiles();
        }
        if (Input.GetMouseButtonDown(1))
        {
            toSelectState();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toViewState();
        }
    }

    /// <summary>
    /// Update actions in the building tiles state
    /// </summary>
    private void buildingTilesState()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePositionInt = Vector3Int.FloorToInt(mousePosition);

        createSelectingGrid(mousePositionInt);
        createPreviewPoint(mousePositionInt);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toViewState();
        }
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            setTileFull(mousePositionInt, this.selectedTile, this.selectedTileMap, false);
        }
    }

    /// <summary>
    /// Update actions in the building gameobjects state
    /// </summary>
    private void buildingGameObjectsState()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePositionInt = Vector3Int.FloorToInt(mousePosition);

        this.selectingGameObject.transform.position = mousePosition;
        this.selectingGameObject.SetActive(true);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toViewState();
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (this.selectedGameObject == null)
            {
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(mousePosition, 1))
                {
                    Castle castle = collider.GetComponent<Castle>();
                    if (castle != null)
                    {
                        NetworkServer.Destroy(castle.gameObject);
                    }
                }
            }
            else
            {
                GameObject gameObject = Instantiate(this.selectedGameObject, mousePosition, Quaternion.identity);

                Castle castle = gameObject.GetComponent<Castle>();
                if (castle != null)
                {
                    GameObject castles = GameObject.Find("Castles").gameObject;
                    gameObject.transform.SetParent(castles.transform);
                }
            }
        }
    }

    /// <summary>
    /// Update acitons in the selecting state
    /// </summary>
    private void selectingState()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector2Int mousePositionInt = Vector2Int.FloorToInt(mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            this.startSelectingPoint = mousePositionInt;
        }
        if (Input.GetMouseButton(0))
        {
            createSelectingGrid(this.startSelectingPoint, mousePositionInt);
        }
        if (Input.GetMouseButtonUp(0))
        {
            this.endSelectingPoint = mousePositionInt;
            createSelectingGrid(this.startSelectingPoint, this.endSelectingPoint);
        }
    }

    /// <summary>
    /// Creates a selecting rectangle with the given positions, uses the custom selecting tile
    /// </summary>
    /// <param name="pos1"></param>
    /// <param name="pos2"></param>
    private void createSelectingGrid(Vector2Int pos1, Vector2Int pos2)
    {
        this.selectingTilemap.ClearAllTiles();

        Vector2Int bottomLeft = new Vector2Int(Mathf.Min(pos1.x, pos2.x), Mathf.Min(pos1.y, pos2.y));
        Vector2Int topRight = new Vector2Int(Mathf.Max(pos1.x, pos2.x), Mathf.Max(pos1.y, pos2.y));

        this.customSelectingTile.topRight = topRight;
        this.customSelectingTile.bottomLeft = bottomLeft;

        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            this.selectingTilemap.SetTile(new Vector3Int(x, bottomLeft.y, 0), this.customSelectingTile);
            this.selectingTilemap.SetTile(new Vector3Int(x, topRight.y, 0), this.customSelectingTile);
        }
        for (int y = bottomLeft.y; y <= topRight.y; y++)
        {
            this.selectingTilemap.SetTile(new Vector3Int(bottomLeft.x, y, 0), this.customSelectingTile);
            this.selectingTilemap.SetTile(new Vector3Int(topRight.x, y, 0), this.customSelectingTile);
        }
    }
    private void createSelectingGrid(Vector3Int pos1, Vector3Int pos2)
    {
        this.selectingTilemap.ClearAllTiles();

        Vector2Int bottomLeft = new Vector2Int(Mathf.Min(pos1.x, pos2.x), Mathf.Min(pos1.y, pos2.y));
        Vector2Int topRight = new Vector2Int(Mathf.Max(pos1.x, pos2.x), Mathf.Max(pos1.y, pos2.y));

        this.customSelectingTile.topRight = topRight;
        this.customSelectingTile.bottomLeft = bottomLeft;

        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            this.selectingTilemap.SetTile(new Vector3Int(x, bottomLeft.y, 0), this.customSelectingTile);
            this.selectingTilemap.SetTile(new Vector3Int(x, topRight.y, 0), this.customSelectingTile);
        }
        for (int y = bottomLeft.y; y <= topRight.y; y++)
        {
            this.selectingTilemap.SetTile(new Vector3Int(bottomLeft.x, y, 0), this.customSelectingTile);
            this.selectingTilemap.SetTile(new Vector3Int(topRight.x, y, 0), this.customSelectingTile);
        }
    }

    /// <summary>
    /// Creates a selecting rectangle with the given position, uses the custom selecting tile
    /// </summary>
    /// <param name="mousePosition"></param> the position of the mouse as middle of the selecting grid
    private void createSelectingGrid(Vector3Int mousePosition)
    {
        this.selectingTilemap.ClearAllTiles();

        int minX = mousePosition.x - (this.buildingSize - 1);
        int maxX = mousePosition.x + (this.buildingSize - 1);
        int minY = mousePosition.y - (this.buildingSize - 1);
        int maxY = mousePosition.y + (this.buildingSize - 1);

        Vector2Int bottomLeft = new Vector2Int(minX, minY);
        Vector2Int topRight = new Vector2Int(maxX, maxY);

        this.customSelectingTile.topRight = topRight;
        this.customSelectingTile.bottomLeft = bottomLeft;

        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            this.selectingTilemap.SetTile(new Vector3Int(x, bottomLeft.y, 0), this.customSelectingTile);
            this.selectingTilemap.SetTile(new Vector3Int(x, topRight.y, 0), this.customSelectingTile);
        }
        for (int y = bottomLeft.y; y <= topRight.y; y++)
        {
            this.selectingTilemap.SetTile(new Vector3Int(bottomLeft.x, y, 0), this.customSelectingTile);
            this.selectingTilemap.SetTile(new Vector3Int(topRight.x, y, 0), this.customSelectingTile);
        }
    }

    /// <summary>
    /// Creates a preview on the previewTilemap of the selected tiles
    /// </summary>
    private void createSelectedTilesPreview()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePosition3Int = Vector3Int.FloorToInt(mousePosition);

        createSelectingGrid(mousePosition3Int + this.selectedTiles.bottomLeft, mousePosition3Int + this.selectedTiles.topRight);

        this.previewTilemap.ClearAllTiles();
        foreach (SelectedTileGroup selectedTileGroup in this.selectedTiles.tileGroups)
        {
            Vector3Int position = selectedTileGroup.position + mousePosition3Int;
            this.previewTilemap.SetTile(position, selectedTileGroup.frontTile.tile);
        }
    }

    /// <summary>
    /// Will place the selected tiles on the current mouse position
    /// </summary>
    private void placeSelectedTiles()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePosition3Int = Vector3Int.FloorToInt(mousePosition);

        foreach (SelectedTileGroup selectedTileGroup in this.selectedTiles.tileGroups)
        {
            Vector3Int position = selectedTileGroup.position + mousePosition3Int;
            foreach (SelectedTileInfo selectedTileInfo in selectedTileGroup.tilesInfo)
            {
                selectedTileInfo.tilemap.SetTile(position, selectedTileInfo.tile);
            }
        }
    }

    /// <summary>
    /// Will place a square of tiles on the specified tilemap, this size is determined by the current buildingsize
    /// </summary>
    /// <param name="pos"></param> middle of the square
    /// <param name="tile"></param>
    /// <param name="tilemap"></param>
    /// <param name="clear"></param> true => clears tilemap, false => does not clear tilemap
    private void setTileFull(Vector3Int pos, Tile tile, Tilemap tilemap, bool clear)
    {
        if (clear) tilemap.ClearAllTiles();
        int minX = pos.x - (this.buildingSize - 1);
        int maxX = pos.x + (this.buildingSize - 1);
        int minY = pos.y - (this.buildingSize - 1);
        int maxY = pos.y + (this.buildingSize - 1);
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                tilemap.SetTile(new Vector3Int(x,y,0), tile);
            }
        }
    }

    /// <summary>
    /// Will place the sides of a square of tiles on the specified tilemap, this size is determined by the current buildingsize
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="tile"></param>
    /// <param name="tilemap"></param>
    /// <param name="clear"></param>
    private void setTileSides(Vector3Int pos, Tile tile, Tilemap tilemap, bool clear)
    {
        if (clear) tilemap.ClearAllTiles();
        int minX = pos.x - (this.buildingSize - 1);
        int maxX = pos.x + (this.buildingSize - 1);
        int minY = pos.y - (this.buildingSize - 1);
        int maxY = pos.y + (this.buildingSize - 1);
        for (int x = minX; x <= maxX; x++)
        {
            tilemap.SetTile(new Vector3Int(x, minY, 0), tile);
            tilemap.SetTile(new Vector3Int(x, maxY, 0), tile);
        }
        for (int y = minY; y <= maxY; y++)
        {
            tilemap.SetTile(new Vector3Int(minX, y, 0), tile);
            tilemap.SetTile(new Vector3Int(maxX, y, 0), tile);
        }
    }

    /// <summary>
    /// Creates a button for selecting the given tile
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="ButtonPrefab"></param>
    private void createTileButton(Tile tile, GameObject ButtonPrefab)
    {
        GameObject buttonGameobjectTile = Instantiate(ButtonPrefab, this.tileScrollView.transform.Find("Viewport").transform.Find("Content"));
        buttonGameobjectTile.GetComponent<Image>().sprite = tile.sprite;
        buttonGameobjectTile.GetComponent<Image>().color = tile.color;
        buttonGameobjectTile.GetComponent<Button>().onClick.AddListener(delegate { setTile(tile); });
        GameObject tileNameGameObject = buttonGameobjectTile.transform.Find("Name").gameObject;
        tileNameGameObject.GetComponent<TMP_Text>().text = tile.name;
    }

    /// <summary>
    /// Creates a button for selecting the given gameobject
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="ButtonPrefab"></param>
    private void createGameObjectButton(GameObject gameObject, GameObject ButtonPrefab)
    {
        SpriteRenderer spriteRenderer2D = gameObject.GetComponent<SpriteRenderer>();
        GameObject buttonGameobject = Instantiate(ButtonPrefab, gameObjectScrollView.transform.Find("Viewport").transform.Find("Content"));
        buttonGameobject.GetComponent<Image>().sprite = spriteRenderer2D.sprite;
        buttonGameobject.GetComponent<Image>().color = spriteRenderer2D.color;
        buttonGameobject.GetComponent<Button>().onClick.AddListener(delegate { setGameObject(gameObject); });
        GameObject gameObjectName = buttonGameobject.transform.Find("Name").gameObject;
        gameObjectName.GetComponent<TMP_Text>().text = gameObject.name;
    }

    /// <summary>
    /// Updates the building size, cannot be lower than 1
    /// </summary>
    /// <param name="size"></param>
    private void updateBuildingSize(int size)
    {
        this.buildingSize = size;
        if (size <= 1)
        {
            this.buildingSize = 1;
        }
    }

    /// <summary>
    /// Will reset the basic Ui to its original state
    /// </summary>
    private void resetBasicUi()
    {
        this.levelOptions.SetActive(false);
    }

    /// <summary>
    /// Will reset the full Ui to its original state. It will deactivate all Ui except the basic Ui
    /// </summary>
    private void resetUi()
    {
        foreach (Transform uiElement in this.normalUi.GetComponentInChildren<Transform>())
        {
            if (uiElement.name != "BasicUi" && uiElement.name != "QuickSelectButtons")
            {
                uiElement.gameObject.SetActive(false);
            }
        }

        this.basicUi.SetActive(true);
    }

    /// <summary>
    /// Will reset the info panel to its original state
    /// </summary>
    private void resetInfoPanel()
    {
        this.buildingStateInfo.text = "";
        this.firstLine.text = "";
        this.secondLine.text = "";
    }

    /// <summary>
    /// Will reset the selected Tile, Tilemap and Gameobject
    /// </summary>
    private void resetSelected()
    {
        this.selectedTileMap = floorTilemap;
        this.selectedTile = null;
        this.selectedGameObject = null;
    }

    /// <summary>
    /// Stores the selected tiles
    /// </summary>
    public void copyToCursor()
    {
        List<SelectedTileGroup> selectedTileGroups = new List<SelectedTileGroup>();

        Vector3Int bottomLeft = new Vector3Int(Mathf.Min(startSelectingPoint.x, endSelectingPoint.x), Mathf.Min(startSelectingPoint.y, endSelectingPoint.y), 0);
        Vector3Int topRight = new Vector3Int(Mathf.Max(startSelectingPoint.x, endSelectingPoint.x), Mathf.Max(startSelectingPoint.y, endSelectingPoint.y), 0);

        int middleX = (int)((topRight.x + bottomLeft.x) / 2);
        int middleY = (int)((topRight.y + bottomLeft.y) / 2);
        Vector3Int middlePos = new Vector3Int(middleX, middleY, 0);

        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            for (int y = bottomLeft.y; y <= topRight.y; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                List<SelectedTileInfo> tilesInfo = new List<SelectedTileInfo>();

                // Check floor tilemap
                Tile floorTile = (Tile)this.floorTilemap.GetTile(position);
                if (floorTile != null)
                {
                    SelectedTileInfo selectedTileInfo = new SelectedTileInfo(floorTile, floorTilemap);
                    tilesInfo.Add(selectedTileInfo);
                }

                // Check wall tilemap
                Tile wallTile = (Tile)this.wallsTilemap.GetTile(position);
                if(wallTile != null)
                {
                    SelectedTileInfo selectedTileInfo = new SelectedTileInfo(wallTile, wallsTilemap);
                    tilesInfo.Add(selectedTileInfo);
                }

                // Check decoration tilemap
                Tile decoTile = (Tile)this.decorationTilemap.GetTile(position);
                if (decoTile != null)
                {
                    SelectedTileInfo selectedTileInfo = new SelectedTileInfo(decoTile, decorationTilemap);
                    tilesInfo.Add(selectedTileInfo);
                }

                if(tilesInfo.Count > 0)
                {
                    SelectedTileGroup selectedTileGroup = new SelectedTileGroup(tilesInfo, position - middlePos);
                    selectedTileGroups.Add(selectedTileGroup);
                }
            }
        }
        this.selectedTiles = new SelectedTiles(selectedTileGroups, bottomLeft - middlePos, topRight - middlePos);
    }

    /// <summary>
    /// Increases building size by one
    /// </summary>
    public void increaseBuildingSize()
    {
        updateBuildingSize(this.buildingSize + 1);
    }

    /// <summary>
    /// Decreases building size by one
    /// </summary>
    public void decreaseBuildingSize()
    {
        if (this.buildingSize == 1) return;
        updateBuildingSize(this.buildingSize - 1);
    }

    /// <summary>
    /// Sets the selected tile and updates the infopanel
    /// </summary>
    /// <param name="tile"></param>
    public void setTile(Tile tile)
    {
        this.selectedTile = tile;

        if(tile == null)
        {
            this.firstLine.text = "Erasing Tiles";
        }
        else
        {
            this.firstLine.text = "Tile: " + tile.name;
        }
    }

    /// <summary>
    /// Sets the selected gameobject and updates the infopanel
    /// </summary>
    /// <param name="gameObject"></param>
    public void setGameObject(GameObject gameObject)
    {
        this.selectedGameObject = gameObject;

        if(gameObject == null)
        {
            this.firstLine.text = "Erasing GameObjects";
        }
        else
        {
            this.firstLine.text = "GameObject: " + gameObject.name;
        }
    }

    /// <summary>
    /// Select the floor tilemap and update the infopanel
    /// </summary>
    public void selectFloor()
    {
        this.selectedTileMap = floorTilemap;
        this.secondLine.text = "Tilemap: " + floorTilemap.name;
    }

    /// <summary>
    /// Select the wall tilemap and update the infopanel
    /// </summary>
    public void selectWalls()
    {
        this.selectedTileMap = wallsTilemap;
        this.secondLine.text = "Tilemap: " + wallsTilemap.name;
    }

    /// <summary>
    /// Select the decoration tilemap and update the infopanel
    /// </summary>
    public void selectDecoration()
    {
        this.selectedTileMap = decorationTilemap;
        this.secondLine.text = "Tilemap: " + decorationTilemap.name;
    }

    /// <summary>
    /// Selects the erasor
    /// </summary>
    public void selectErasor()
    {
        if(buildingState == BuildingState.BuildingTiles || buildingState == BuildingState.DrawingRectangle || buildingState == BuildingState.DrawingLine)
        {
            setTile(null);
        }
        if (buildingState == BuildingState.BuildingGameObjects)
        {
            setGameObject(null);
        }
    }

    /// <summary>
    /// Clears the level by clearing the tilemap and deleting gameobjects
    /// </summary>
    public void clearLevel()
    {
        this.wallsTilemap.ClearAllTiles();
        this.floorTilemap.ClearAllTiles();
        this.decorationTilemap.ClearAllTiles();
        deleteCastles();
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

    private void selectTilemap(Tilemap tilemap)
    {
        this.selectedTileMap = tilemap;

        if (tilemap == null)
        {
            this.secondLine.text = "No tilemap selected";
        }
        else
        {
            this.secondLine.text = "Tilemap: " + tilemap.name;
        }
    }

    public void toFillState()
    {
        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        this.setTile(this.selectedTile);
        this.selectTilemap(this.selectedTileMap);

        resetUi();

        this.buildingTilesUi.SetActive(true);
        this.infoPanel.SetActive(true);

        this.buildingState = BuildingState.Fill;
        this.buildingStateInfo.text = "Filling Space";
    }

    public void toDrawLineState()
    {
        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        this.setTile(this.selectedTile);
        this.selectTilemap(this.selectedTileMap);

        resetUi();

        this.buildingTilesUi.SetActive(true);
        this.infoPanel.SetActive(true);

        this.buildingState = BuildingState.DrawingLine;
        this.buildingStateInfo.text = "Drawing Line";
    }

    public void toDrawRectangleState()
    {
        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        this.setTile(this.selectedTile);
        this.selectTilemap(this.selectedTileMap);

        resetUi();

        this.buildingTilesUi.SetActive(true);
        this.infoPanel.SetActive(true);

        this.buildingState = BuildingState.DrawingRectangle;
        this.buildingStateInfo.text = "Drawing Rectangle";
    }

    public void toBuildTileState()
    {
        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        this.setTile(this.selectedTile);
        this.selectTilemap(this.selectedTileMap);

        resetUi();

        this.buildingTilesUi.SetActive(true);
        this.infoPanel.SetActive(true);

        this.buildingState = BuildingState.BuildingTiles;
        this.buildingStateInfo.text = "Building Tiles";
    }

    public void toBuildGameObjectState()
    {
        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        resetInfoPanel();
        resetUi();

        this.buildingGameObjectsUi.SetActive(true);
        this.infoPanel.SetActive(true);

        setGameObject(null);

        this.buildingState = BuildingState.BuildingGameObjects;
        this.buildingStateInfo.text = "PlacingGameObjects";
    }

    public void toViewState()
    {
        resetUi();

        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        this.buildingState = BuildingState.Viewing;
    }

    public void toSelectState()
    {
        resetUi();

        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();
        this.selectedTiles = null;

        this.buildingState = BuildingState.Selecting;
    }

    public void toPasteState()
    {
        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        this.resetUi();

        this.buildingState = BuildingState.Paste;
    }

    public void leaveLevelCreation()
    {
        NetworkManager.singleton.ServerChangeScene("MainMenu");
    }

    public void saveLevel()
    {
        string levelName = GameObject.Find("SaveButtonInputField").GetComponent<TMP_InputField>().text;
        SaveLoadLevel saveLoadLevel = GameObject.Find("LoadSaveLevelManager").GetComponent<SaveLoadLevel>();
        saveLoadLevel.saveLevel(levelName);
    }

    public void loadLevel()
    {
        string levelName = GameObject.Find("LoadButtonInputField").GetComponent<TMP_InputField>().text;
        GameObject.Find("LoadSaveLevelManager").GetComponent<SaveLoadLevel>().loadLevel(levelName);
    }

    public void OnClickLevelOptions()
    {
        if (this.levelOptions.activeSelf == true)
        {
            resetBasicUi();
        }
        else
        {
            resetBasicUi();
            this.levelOptions.SetActive(true);
        }
    }
}
