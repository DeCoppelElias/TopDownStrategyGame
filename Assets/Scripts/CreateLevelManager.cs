using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class CreateLevelManager : MonoBehaviour
{
    private Tile selectedTile;
    private Tilemap selectedTileMap;
    private GameObject selectedGameObject;

    private Tilemap floorTilemap;
    private Tilemap wallsTilemap;
    private Tilemap decorationTilemap;
    private Tilemap previewTilemap;

    private Tilemap selectingTilemap;
    private GameObject selectingGameObject;
    private GameObject cursorSize;
    private SelectingTile customSelectingTile;

    private GameObject tileScrollView;
    private GameObject gameObjectScrollView;

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
    private enum BuildingState { BuildingTiles, BuildingGameObjects, Viewing, Selecting, Paste}
    private BuildingState buildingState = BuildingState.Viewing;

    private TMP_Text buildingStateInfo;
    private TMP_Text selectedTileText;
    private TMP_Text selectedTilemapText;
    private TMP_Text selectedGameObjectText;
    private GameObject buildingTileInfo;
    private GameObject buildingGameObjectInfo;
    private GameObject InfoPanel;

    private GameObject selectTileMapButtons;

    private GameObject copyToCursorButton;

    private int buildingSize = 1;
    private void Start()
    {
        this.selectTileMapButtons = GameObject.Find("SelectTileMapButtons");
        this.selectTileMapButtons.SetActive(false);

        this.floorTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
        this.wallsTilemap = GameObject.Find("Walls").GetComponent<Tilemap>();
        this.decorationTilemap = GameObject.Find("Decoration").GetComponent<Tilemap>();
        this.previewTilemap = GameObject.Find("Preview").GetComponent<Tilemap>();

        this.selectingTilemap = GameObject.Find("Selecting").GetComponent<Tilemap>();
        this.selectingGameObject = Instantiate((GameObject)Resources.Load("Prefabs/SelectingGameObject"), new Vector3(0, 0, 0), Quaternion.identity);
        this.selectingGameObject.SetActive(false);
        this.customSelectingTile = (SelectingTile)Resources.Load("Tiles/SelectingTile");
        this.cursorSize = GameObject.Find("CursorSize");
        this.cursorSize.SetActive(false);

        this.InfoPanel = GameObject.Find("InfoPanel");
        this.InfoPanel.SetActive(false);
        this.buildingTileInfo = InfoPanel.transform.Find("BuildingTileInfo").gameObject;
        this.buildingTileInfo.SetActive(false);
        this.buildingGameObjectInfo = InfoPanel.transform.Find("BuildingGameObjectInfo").gameObject;
        this.buildingGameObjectInfo.SetActive(false);

        this.buildingStateInfo = InfoPanel.transform.Find("BuildingState").GetComponent<TMP_Text>();
        this.selectedTileText = buildingTileInfo.transform.Find("Current Tile").GetComponent<TMP_Text>();
        this.selectedTilemapText = buildingTileInfo.transform.Find("Current Tilemap").GetComponent<TMP_Text>();
        this.selectedGameObjectText = buildingGameObjectInfo.transform.Find("Current GameObject").GetComponent<TMP_Text>();

        this.tileScrollView = GameObject.Find("TileScrollView");
        this.tileScrollView.SetActive(false);
        this.gameObjectScrollView = GameObject.Find("GameObjectScrollView");
        this.gameObjectScrollView.SetActive(false);

        this.selectFloor();
        this.selectErasor();

        this.copyToCursorButton = GameObject.Find("CopyToCursor");
        this.copyToCursorButton.SetActive(false);

        GameObject buttonPrefab = Resources.Load("Prefabs/Button") as GameObject;
        foreach (UnityEngine.Object obj in Resources.LoadAll("Tiles/BuildableTiles"))
        {
            if(obj is Tile tile)
            {
                createTileButton(tile, buttonPrefab);
            }
        }

        GameObject castleGameObject = (GameObject)Resources.Load("Prefabs/Entities/Castle/PlayerCastle");
        createGameObjectButton(castleGameObject, buttonPrefab);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            this.previewTilemap.ClearAllTiles();
            this.selectingTilemap.ClearAllTiles();
            return;
        }

        if (buildingState == BuildingState.Paste)
        {
            pasteState();
        }
        if (buildingState == BuildingState.Selecting)
        {
            selectingState();
        }
        if (buildingState == BuildingState.BuildingTiles)
        {
            buildingTilesState();
        }
        if (buildingState == BuildingState.BuildingGameObjects)
        {
            buildingGameObjectsState();
        }
    }

    private void pasteState()
    {
        this.createSelectedTilesPreview();

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

    private void buildingTilesState()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePositionInt = Vector3Int.FloorToInt(mousePosition);

        createSelectingGrid(mousePositionInt);
        setTileFull(mousePositionInt, this.selectedTile, this.previewTilemap, true);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            toViewState();
        }
        if (Input.GetMouseButton(0))
        {
            setTileFull(mousePositionInt, this.selectedTile, this.selectedTileMap, false);
        }
    }

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

    private void selectingState()
    {
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

    private void createSelectingGrid(Vector3Int pos)
    {
        this.selectingTilemap.ClearAllTiles();

        int minX = pos.x - (this.buildingSize - 1);
        int maxX = pos.x + (this.buildingSize - 1);
        int minY = pos.y - (this.buildingSize - 1);
        int maxY = pos.y + (this.buildingSize - 1);

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

    private void createTileButton(Tile tile, GameObject ButtonPrefab)
    {
        GameObject buttonGameobjectTile = Instantiate(ButtonPrefab, tileScrollView.transform.Find("Viewport").transform.Find("Content"));
        buttonGameobjectTile.GetComponent<Image>().sprite = tile.sprite;
        buttonGameobjectTile.GetComponent<Image>().color = tile.color;
        buttonGameobjectTile.GetComponent<Button>().onClick.AddListener(delegate { setTile(tile); });
        GameObject tileNameGameObject = buttonGameobjectTile.transform.Find("Name").gameObject;
        tileNameGameObject.GetComponent<TMP_Text>().text = tile.name;
    }

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

    private void updateBuildingSize(int size)
    {
        this.buildingSize = size;
        if (size <= 1)
        {
            this.buildingSize = 1;
        }
    }




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

    public void increaseBuildingSize()
    {
        updateBuildingSize(this.buildingSize + 1);
    }

    public void decreaseBuildingSize()
    {
        if (this.buildingSize == 1) return;
        updateBuildingSize(this.buildingSize - 1);
    }
    public void setTile(Tile tile)
    {
        this.selectedTile = tile;
        this.selectedTileText.text = "Tile: " + tile.name;
    }

    public void setGameObject(GameObject gameObject)
    {
        this.selectedGameObject = gameObject;
        this.selectedGameObjectText.text = "GameObject: " + gameObject.name;
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
        if(buildingState == BuildingState.BuildingTiles)
        {
            this.selectedTile = null;
            this.selectedTileText.text = "Erasing Tiles";
        }
        if (buildingState == BuildingState.BuildingGameObjects)
        {
            this.selectedGameObject = null;
            this.selectedGameObjectText.text = "Erasing GameObjects";
        }
    }

    public void clearLevel()
    {
        this.wallsTilemap.ClearAllTiles();
        this.floorTilemap.ClearAllTiles();
        this.decorationTilemap.ClearAllTiles();
        deleteCastles();
    }

    private void deleteCastles()
    {
        GameObject castles = GameObject.Find("Castles");
        foreach (Castle castle in castles.GetComponentsInChildren<Castle>())
        {
            NetworkServer.Destroy(castle.gameObject);
        }
    }

    public void toBuildTileState()
    {
        this.InfoPanel.SetActive(true);
        this.tileScrollView.SetActive(true);
        this.buildingTileInfo.SetActive(true);
        this.selectTileMapButtons.SetActive(true);
        this.cursorSize.SetActive(true);
        this.updateBuildingSize(1);
        this.gameObjectScrollView.SetActive(false);
        this.buildingGameObjectInfo.SetActive(false);
        this.copyToCursorButton.SetActive(false);
        this.buildingState = BuildingState.BuildingTiles;
        this.buildingStateInfo.text = "Building Tiles";
    }

    public void toBuildGameObjectState()
    {
        this.InfoPanel.SetActive(true);
        this.tileScrollView.SetActive(false);
        this.buildingTileInfo.SetActive(false);
        this.selectTileMapButtons.SetActive(false);
        this.cursorSize.SetActive(false);
        this.copyToCursorButton.SetActive(false);
        this.gameObjectScrollView.SetActive(true);
        this.buildingGameObjectInfo.SetActive(true);
        this.buildingState = BuildingState.BuildingGameObjects;
        this.buildingStateInfo.text = "PlacingGameObjects";
    }

    public void toViewState()
    {
        this.InfoPanel.SetActive(false);
        this.tileScrollView.SetActive(false);
        this.buildingTileInfo.SetActive(false);
        this.selectTileMapButtons.SetActive(false);
        this.cursorSize.SetActive(false);
        this.gameObjectScrollView.SetActive(false);
        this.buildingGameObjectInfo.SetActive(false);
        this.copyToCursorButton.SetActive(false);

        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();

        this.buildingState = BuildingState.Viewing;
    }

    public void toSelectState()
    {
        this.InfoPanel.SetActive(false);
        this.tileScrollView.SetActive(false);
        this.buildingTileInfo.SetActive(false);
        this.selectTileMapButtons.SetActive(false);
        this.cursorSize.SetActive(false);
        this.gameObjectScrollView.SetActive(false);
        this.buildingGameObjectInfo.SetActive(false);
        this.copyToCursorButton.SetActive(true);

        this.previewTilemap.ClearAllTiles();
        this.selectingTilemap.ClearAllTiles();
        this.selectedTiles = null;

        this.buildingState = BuildingState.Selecting;
    }

    public void toPasteState()
    {
        this.InfoPanel.SetActive(false);
        this.tileScrollView.SetActive(false);
        this.buildingTileInfo.SetActive(false);
        this.selectTileMapButtons.SetActive(false);
        this.cursorSize.SetActive(false);
        this.gameObjectScrollView.SetActive(false);
        this.buildingGameObjectInfo.SetActive(false);
        this.copyToCursorButton.SetActive(true);
        this.buildingState = BuildingState.Paste;
    }

    public void leaveLevelCreation()
    {
        NetworkManager.singleton.StopHost();
        NetworkManager.singleton.StopClient();
    }

    public void saveLevel()
    {
        string levelName = GameObject.Find("SaveButtonInputField").GetComponent<TMP_InputField>().text;
        GameObject.Find("LoadSaveLevelManager").GetComponent<SaveLoadLevel>().saveLevel(levelName);
    }

    public void loadLevel()
    {
        string levelName = GameObject.Find("LoadButtonInputField").GetComponent<TMP_InputField>().text;
        GameObject.Find("LoadSaveLevelManager").GetComponent<SaveLoadLevel>().loadLevel(levelName);
    }
}
