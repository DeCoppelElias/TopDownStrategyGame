using Mirror;
using System;
using System.Collections;
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

    // List of actions while mouse button down
    List<Action> mouseButtonDownActions = new List<Action>();

    // Action Manager will store and handle actions
    ActionManager actionManager;

    // Undo and Redo count
    private TMP_Text redoCountText;
    private TMP_Text undoCountText;

    private class NullKey
    {
        public override bool Equals(object obj)
        {
            if (obj is NullKey) return true;
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
    private class CustomDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : class
    {
        private class CustomIEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            public KeyValuePair<TKey, TValue>[] pairs;
            int position = -1;

            public CustomIEnumerator(KeyValuePair<TKey, TValue>[] pairs)
            {
                this.pairs = pairs;
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    try
                    {
                        return pairs[position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
            public bool MoveNext()
            {
                position++;
                return (position < pairs.Length);
            }

            public void Reset()
            {
                position = -1;
            }

            public void Dispose()
            {
                
            }
        }

        private NullKey nullKey = new NullKey();

        public Dictionary<object, TValue> dict;

        public CustomDictionary()
        {
            this.dict = new Dictionary<object, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            if(key == null)
            {
                dict.Add(nullKey, value);
            }
            else
            {
                dict.Add(key, value);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<object, TValue>> enumerator = this.dict.GetEnumerator();
            List<KeyValuePair<TKey, TValue>> pairs = new List<KeyValuePair<TKey, TValue>>();
            while (enumerator.MoveNext())
            {
                KeyValuePair<object, TValue> pair = enumerator.Current;
                if(pair.Key is NullKey)
                {
                    pairs.Add(new KeyValuePair<TKey, TValue>(null, pair.Value));
                }
                else
                {
                    pairs.Add(new KeyValuePair<TKey, TValue>((TKey)pair.Key, pair.Value));
                }
            }
            return new CustomIEnumerator(pairs.ToArray());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public TValue this[TKey key]
        {
            get
            {
                if (key == null) return this.dict[nullKey];
                return this.dict[key];
            }
            set
            {
                if (key == null) this.dict[nullKey] = value;
                else this.dict[key] = value;
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (key == null) return this.dict.ContainsKey(nullKey);
            else return this.dict.ContainsKey(key);
        }
        public int Count()
        {
            return this.dict.Count;
        }
    }

    private class ActionManager
    {
        public int size = 20;
        // List of last actions
        public ActionQueue previousActions;
        public ActionQueue undoneActions;

        // Undo and Redo count
        private TMP_Text redoCountText;
        private TMP_Text undoCountText;

        public ActionManager(int size)
        {
            this.size = size;
            this.previousActions = new ActionQueue(size);
            this.undoneActions = new ActionQueue(size);
        }

        public ActionManager(int size, TMP_Text undoCountText, TMP_Text redoCountText)
        {
            this.size = size;
            this.previousActions = new ActionQueue(size);
            this.undoneActions = new ActionQueue(size);

            this.undoCountText = undoCountText;
            this.redoCountText = redoCountText;
        }

        public void addAction(Action action)
        {
            if (action == null) return;
            addToPreviousActions(action);
            this.undoneActions = new ActionQueue(size);

            // Display new count
            if (this.redoCountText == null) return;
            if (this.undoneActions.Count() == 0) this.redoCountText.text = "";
            else this.redoCountText.text = this.undoneActions.Count().ToString();
        }

        public void undo()
        {
            // Pop latest action from list and undo it
            Action action = this.previousActions.undoLastAction();
            if (action == null) return;
            addToUndoneActions(action);

            // Display new count
            if (this.undoCountText == null) return;
            if (this.previousActions.Count() == 0) this.undoCountText.text = "";
            else this.undoCountText.text = this.previousActions.Count().ToString();
        }
        
        public void redo()
        {
            Action action = this.undoneActions.redoLastAction();
            if (action == null) return;
            addToPreviousActions(action);

            // Display new count
            if (this.redoCountText == null) return;
            if (this.undoneActions.Count() == 0) this.redoCountText.text = "";
            else this.redoCountText.text = this.undoneActions.Count().ToString();
        }

        private void addToPreviousActions(Action action)
        {
            this.previousActions.Add(action);

            // Display new count
            if (this.undoCountText == null) return;
            if (this.previousActions.Count() == 0) this.undoCountText.text = "";
            else this.undoCountText.text = this.previousActions.Count().ToString();
        }

        private void addToUndoneActions(Action action)
        {
            this.undoneActions.Add(action);

            // Display new count
            if (this.redoCountText == null) return;
            if (this.undoneActions.Count() == 0) this.redoCountText.text = "";
            else this.redoCountText.text = this.undoneActions.Count().ToString();
        }
    }
    private class ActionQueue
    {
        public List<Action> actions;
        public int maxSize;
        public ActionQueue(int size)
        {
            this.actions = new List<Action>();
            this.maxSize = size;
        }

        public ActionQueue(List<Action> actions, int size)
        {
            this.maxSize = size;
            if (actions.Count <= size) this.actions = actions;
            else
            {
                List <Action> result = new List<Action>();
                for (int i = 0; i < size; i++)
                {
                    Action action = actions[i];
                    result.Add(action);
                }
                this.actions = result;
            }
        }

        public void Add(Action action)
        {
            if (action == null) return;
            actions.Insert(0, action);
            if(actions.Count > maxSize)
            {
                actions.RemoveAt(actions.Count-1);
            }
        }

        private Action PopFirst()
        {
            if (actions.Count == 0) return null;
            Action result = actions[0];
            this.actions.RemoveAt(0);

            return result;
        }

        public Action undoLastAction()
        {
            Action action = PopFirst();
            if (action == null) return null;
            action.undo();
            return action;
        }

        public Action redoLastAction()
        {
            Action action = PopFirst();
            if (action == null) return null;
            action.execute();
            return action;
        }

        public int Count()
        {
            return this.actions.Count;
        }
    }
    private interface Action
    {
        public void execute();
        public void undo();
    }
    private class MultipleActions : Action
    {
        public List<Action> actions;

        public MultipleActions(List<Action> actions)
        {
            this.actions = actions;
        }

        /// <summary>
        /// Will create a MultipleActions object with the data in info
        /// </summary>
        /// <param name="info"></param> the biggest dictionary will store changes for each tilemap, the dictionary inside stores for each new tile which tiles got replaced by it
        public MultipleActions(Dictionary<Tilemap, CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>>> info)
        {
            List<Action> actions = new List<Action>();
            foreach (KeyValuePair<Tilemap, CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>>> keyVal in info)
            {
                Tilemap tilemap = keyVal.Key;
                CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>> newTile_positions = keyVal.Value;

                List<Action> tilemapActions = new List<Action>();
                foreach (KeyValuePair<Tile, CustomDictionary<Tile, List<Vector3Int>>> keyVal2 in newTile_positions)
                {
                    Tile newTile = keyVal2.Key;
                    CustomDictionary<Tile, List<Vector3Int>> positions = keyVal2.Value;

                    TileAction action = new TileAction(positions, tilemap, newTile);
                    if (action != null) tilemapActions.Add(action);
                }

                MultipleActions multipleActions = new MultipleActions(tilemapActions);
                actions.Add(multipleActions);
            }
            this.actions = actions;
        }

        public void execute()
        {
            if (actions == null) return;
            foreach (Action action in actions)
            {
                action.execute();
            }
        }

        public void undo()
        {
            if (actions == null) return;
            foreach (Action action in actions)
            {
                action.undo();
            }
        }
    }
    private class TileAction : Action
    {
        public Tilemap tilemap;
        public CustomDictionary<Tile, List<Vector3Int>> positions;
        public Tile newTile;

        public TileAction(CustomDictionary<Tile, List<Vector3Int>> positions, Tilemap tilemap, Tile newTile)
        {
            this.positions = positions;
            this.tilemap = tilemap;
            this.newTile = newTile;
        }

        public void execute()
        {
            foreach(KeyValuePair<Tile,List<Vector3Int>> keyVal in positions)
            {
                List<Vector3Int> pos = keyVal.Value;
                foreach(Vector3Int position in pos)
                {
                    tilemap.SetTile(position, newTile);
                }
            }
        }

        public void undo()
        {
            foreach (KeyValuePair<Tile, List<Vector3Int>> keyVal in positions)
            {
                List<Vector3Int> pos = keyVal.Value;
                Tile previousTile = keyVal.Key;
                foreach (Vector3Int position in pos)
                {
                    tilemap.SetTile(position, previousTile);
                }
            }
        }
    }

    private class PlaceGameObjectAction : Action
    {
        public string gameObjectName;
        public Vector3 position;
        public Transform parent;
        public GameObject gameObject;

        public PlaceGameObjectAction(GameObject gameObject, string gameObjectName)
        {
            this.gameObjectName = gameObjectName;
            this.position = gameObject.transform.position;
            this.gameObject = gameObject;
            this.parent = gameObject.transform.parent;
        }

        public void execute()
        {
            GameObject gameObjectPrefab = (GameObject)Resources.Load("Prefabs/BuildablePrefabs/" + gameObjectName);
            GameObject gameObject = Instantiate(gameObjectPrefab, position, Quaternion.identity, parent);
            NetworkServer.Spawn(gameObject);
            this.gameObject = gameObject;
        }

        public void undo()
        {
            if (gameObject == null) return;
            NetworkServer.Destroy(gameObject);
        }
    }
    private class RemoveGameObjectAction : Action
    {
        public string gameObjectName;
        public Vector3 position;
        public GameObject gameObject;
        public Transform parent;

        public RemoveGameObjectAction(GameObject gameObject)
        {
            this.gameObjectName = gameObject.name;
            this.position = gameObject.transform.position;
            this.gameObject = gameObject;
            this.parent = gameObject.transform.parent;
        }

        public void execute()
        {
            if (gameObject == null) return;
            NetworkServer.Destroy(gameObject);
        }

        public void undo()
        {
            GameObject gameObjectPrefab = (GameObject)Resources.Load("Prefabs/BuildablePrefabs/" + gameObjectName);
            GameObject gameObject = Instantiate(gameObjectPrefab, position, Quaternion.identity);
            NetworkServer.Spawn(gameObject);
        }
    }

    // Building size
    private int buildingSize = 1;

    // Last Tile placement
    private Vector3Int lastTilePlacement = new Vector3Int(0, 0, 1);

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
        public List<SelectedTileInfo> tilesInfo;
        public SelectedTileInfo frontTile;
        public Vector3Int position;
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
        GameObject castleGameObject = (GameObject)Resources.Load("Prefabs/BuildablePrefabs/PlayerCastle");
        createGameObjectButton(castleGameObject, buttonPrefab);

        // Finding redo and undo count text
        this.undoCountText = GameObject.Find("UndoButton").transform.Find("Count").GetComponent<TMP_Text>();
        this.redoCountText = GameObject.Find("RedoButton").transform.Find("Count").GetComponent<TMP_Text>();

        // Instantiating action manager
        this.actionManager = new ActionManager(20, this.undoCountText, this.redoCountText);
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

    /// <summary>
    /// Update actions in the fill state
    /// </summary>
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
            Action action = fill(this.selectedTile, this.selectedTileMap, mousePositionInt);
            this.actionManager.addAction(action);
        }
    }

    /// <summary>
    /// Update actions in the drawing rectangle state
    /// </summary>
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
            Action action = drawRectangle(this.startSelectingPoint, this.endSelectingPoint, this.selectedTileMap, this.selectedTile, false);
            this.actionManager.addAction(action);
        }
    }

    /// <summary>
    /// Update actions in the drawing line state
    /// </summary>
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
            Action action = drawLine(this.startSelectingPoint, this.endSelectingPoint, this.selectedTileMap, this.selectedTile, false);
            this.actionManager.addAction(action);
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
            Action action = placeSelectedTiles();
            this.actionManager.addAction(action);
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

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            this.mouseButtonDownActions = new List<Action>();
        }

        if (Input.GetMouseButton(0))
        {
            if (this.lastTilePlacement == mousePositionInt) return;
            Action action = setTileFull(mousePositionInt, this.selectedTile, this.selectedTileMap, false);
            if (action != null) this.mouseButtonDownActions.Add(action);
            this.lastTilePlacement = mousePositionInt;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (mouseButtonDownActions.Count == 0) return;
            MultipleActions action = new MultipleActions(this.mouseButtonDownActions);
            this.actionManager.addAction(action);
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
                        RemoveGameObjectAction action = new RemoveGameObjectAction(castle.gameObject);
                        this.actionManager.addAction(action);

                        NetworkServer.Destroy(castle.gameObject);
                    }
                }
            }
            else
            {
                GameObject gameObject = Instantiate(this.selectedGameObject, mousePosition, Quaternion.identity);
                NetworkServer.Spawn(gameObject);

                PlaceGameObjectAction action = new PlaceGameObjectAction(gameObject, this.selectedGameObject.name);
                this.actionManager.addAction(action);

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

    public void undoLastAction()
    {
        this.actionManager.undo();
    }

    public void redoLastUndoneAction()
    {
        this.actionManager.redo();
    }

    /// <summary>
    /// Node class created for the AVL tree for the fill method
    /// </summary>
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

    /// <summary>
    /// Fills a space with a certain tile
    /// </summary>
    /// <param name="selectedTile"></param>
    /// <param name="tilemap"></param>
    /// <param name="position"></param>
    private Action fill(Tile selectedTile, Tilemap tilemap, Vector3Int position)
    {
        AVL avl = new AVL();
        Dictionary<Vector3Int, string> checkedPositions = new Dictionary<Vector3Int, string>();
        CustomDictionary<Tile, List<Vector3Int>> positions = new CustomDictionary<Tile, List<Vector3Int>>();
        List<Vector3Int> pos = new List<Vector3Int>();
        Dictionary<Vector3Int, string> checkPositions = new Dictionary<Vector3Int, string>();

        FillNode startFillNode = new FillNode(position, position);
        checkPositions.Add(position, "");
        avl.Add(startFillNode);

        Tile clickedTile = (Tile)tilemap.GetTile(position);

        // If clicked tile and selected tile are the same then nothing will change
        if (clickedTile == selectedTile) return null;

        int counter = 0;
        int maxCounter = 1000;
        while (!avl.isEmpty() && checkPositions.Count > 0 && counter < maxCounter)
        {
            FillNode currentfillNode = (FillNode)avl.PopMinValue();
            checkPositions.Remove(currentfillNode.tilePosition);

            checkedPositions.Add(currentfillNode.tilePosition, "");

            pos.Add(currentfillNode.tilePosition);
            tilemap.SetTile(currentfillNode.tilePosition, selectedTile);

            List<Vector3Int> neighbors = getNeighbors(clickedTile, tilemap, currentfillNode.tilePosition);
            foreach (Vector3Int neighbor in neighbors)
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

        positions.Add(clickedTile, pos);
        TileAction tileAction = new TileAction(positions, tilemap, selectedTile);
        return tileAction;
    }

    /// <summary>
    /// Gets neighbors of a certain tile, only tiles that are the same type will be considered neighbors
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="tilemap"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private List<Vector3Int> getNeighbors(Tile tile, Tilemap tilemap, Vector3Int position)
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

    /// <summary>
    /// Creates a preview of a point
    /// </summary>
    /// <param name="mousePositionInt"></param>
    private void createPreviewPoint(Vector3Int mousePositionInt)
    {
        if (this.selectedTile == null)
        {
            setTileFull(mousePositionInt, this.emptyTilePreview, this.previewTilemap, true);
        }
        else
        {
            setTileFull(mousePositionInt, this.selectedTile, this.previewTilemap, true);
        }
    }

    /// <summary>
    /// Creates a preview of a line
    /// </summary>
    /// <param name="start"></param>
    /// <param name="finish"></param>
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

    /// <summary>
    /// Creates a preview of a rectangle
    /// </summary>
    /// <param name="start"></param>
    /// <param name="finish"></param>
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

    /// <summary>
    /// Draws a rectangle
    /// </summary>
    /// <param name="s"></param>
    /// <param name="f"></param>
    /// <param name="tilemap"></param>
    /// <param name="tile"></param>
    /// <param name="clear"></param>
    private Action drawRectangle(Vector2 s, Vector2 f, Tilemap tilemap, Tile tile, bool clear)
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

        List<Action> actions = new List<Action>();
        for (int x = minX; x <= maxX; x++)
        {
            Action action1 = setTileFull(new Vector3Int(x, minY, 0), tile, tilemap, false);
            Action action2 = setTileFull(new Vector3Int(x, maxY, 0), tile, tilemap, false);

            if (action1 != null) actions.Add(action1);
            if (action2 != null) actions.Add(action2);
        }
        for (int y = minY; y <= maxY; y++)
        {
            Action action1 = setTileFull(new Vector3Int(minX, y, 0), tile, tilemap, false);
            Action action2 = setTileFull(new Vector3Int(maxX, y, 0), tile, tilemap, false);

            if (action1 != null) actions.Add(action1);
            if (action2 != null) actions.Add(action2);
        }

        if (actions.Count == 0) return null;
        return new MultipleActions(actions);
    }

    /// <summary>
    /// Draws a line
    /// </summary>
    /// <param name="s"></param>
    /// <param name="f"></param>
    /// <param name="tilemap"></param>
    /// <param name="tile"></param>
    /// <param name="clear"></param>
    private Action drawLine(Vector2 s, Vector2 f, Tilemap tilemap, Tile tile, bool clear)
    {
        if (clear) tilemap.ClearAllTiles();

        Vector3 s3 = new Vector3(s.x, s.y, 0);
        Vector3 f3 = new Vector3(f.x, f.y, 0);

        Vector3Int start = Vector3Int.FloorToInt(s3);
        Vector3Int finish = Vector3Int.FloorToInt(f3);

        // Vertical line
        if (finish.x == start.x)
        {
            List<Action> actions = new List<Action>();

            int minY = Mathf.Min(start.y, finish.y);
            int maxY = Mathf.Max(start.y, finish.y);
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int position = new Vector3Int(finish.x, y, 0);
                Action action = setTileFull(position, tile, tilemap, false);
                if (action != null) actions.Add(action);
            }

            if (actions.Count == 0) return null;
            return new MultipleActions(actions);
        }

        // Horizontal line
        else if (finish.y == start.y)
        {
            List<Action> actions = new List<Action>();

            int minX = Mathf.Min(start.x, finish.x);
            int maxX = Mathf.Max(start.x, finish.x);
            for (int x = minX; x <= maxX; x++)
            {
                Vector3Int position = new Vector3Int(x, finish.y, 0);
                Action action = setTileFull(position, tile, tilemap, false);
                if (action != null) actions.Add(action);
            }

            if (actions.Count == 0) return null;
            return new MultipleActions(actions);
        }

        else
        {
            List<Action> actions = new List<Action>();

            // Calculating rico
            float rico = ((float)(finish.y - start.y)) / (finish.x - start.x);

            // y = (rico * (x - start.x)) + start.y

            int minX = Mathf.Min(start.x, finish.x);
            int maxX = Mathf.Max(start.x, finish.x);

            for (int x = minX; x <= maxX; x++)
            {
                int y = (int)((rico * (x - start.x)) + start.y);
                Vector3Int position = new Vector3Int(x, y, 0);
                Action action = setTileFull(position, tile, tilemap, false);
                if (action != null) actions.Add(action);
            }

            int minY = Mathf.Min(start.y, finish.y);
            int maxY = Mathf.Max(start.y, finish.y);

            for (int y = minY; y <= maxY; y++)
            {
                int x = (int)(((y - start.y) / rico) + start.x);

                Vector3Int position = new Vector3Int(x, y, 0);
                Action action = setTileFull(position, tile, tilemap, false);
                if (action != null) actions.Add(action);
            }

            if (actions.Count == 0) return null;
            return new MultipleActions(actions);
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
    private Action placeSelectedTiles()
    {
        // This variable will store for each tilemap the chages that have been made
        Dictionary<Tilemap, CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>>> info = new Dictionary<Tilemap, CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>>>();

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int mousePosition3Int = Vector3Int.FloorToInt(mousePosition);

        foreach (SelectedTileGroup selectedTileGroup in this.selectedTiles.tileGroups)
        {
            Vector3Int position = selectedTileGroup.position + mousePosition3Int;
            foreach (SelectedTileInfo selectedTileInfo in selectedTileGroup.tilesInfo)
            {
                Tilemap tilemap = selectedTileInfo.tilemap;
                Tile newTile = selectedTileInfo.tile;
                Tile previousTile = (Tile)selectedTileInfo.tilemap.GetTile(position);

                tilemap.SetTile(position, newTile);


                // Check if tilemap in info
                if (info.ContainsKey(tilemap))
                {
                    CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>> newTile_positions = info[tilemap];

                    // Check if newTile is in existing dictionary
                    if (newTile_positions.ContainsKey(newTile))
                    {
                        CustomDictionary<Tile, List<Vector3Int>> positions = newTile_positions[newTile];

                        // Check if previousTile is in existing dictionary
                        if (positions.ContainsKey(previousTile))
                        {
                            positions[previousTile].Add(position);
                        }
                        else
                        {
                            positions.Add(previousTile, new List<Vector3Int>() { position });
                        }
                        newTile_positions[newTile] = positions;
                    }
                    else
                    {
                        CustomDictionary<Tile, List<Vector3Int>> positions = new CustomDictionary<Tile, List<Vector3Int>>();
                        positions.Add(previousTile, new List<Vector3Int>() { position });
                        newTile_positions.Add(newTile, positions);
                    }
                    info[tilemap] = newTile_positions;
                }
                else
                {
                    CustomDictionary<Tile, List<Vector3Int>> positions = new CustomDictionary<Tile, List<Vector3Int>>();
                    positions.Add(previousTile, new List<Vector3Int>() { position });

                    CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>> newTile_positions = new CustomDictionary<Tile, CustomDictionary<Tile, List<Vector3Int>>>();
                    newTile_positions.Add(newTile, positions);

                    info.Add(tilemap, newTile_positions);
                }
            }
        }

        return new MultipleActions(info);
    }

    /// <summary>
    /// Will place a square of tiles on the specified tilemap, this size is determined by the current buildingsize
    /// </summary>
    /// <param name="pos"></param> middle of the square
    /// <param name="tile"></param>
    /// <param name="tilemap"></param>
    /// <param name="clear"></param> true => clears tilemap, false => does not clear tilemap
    private Action setTileFull(Vector3Int pos, Tile tile, Tilemap tilemap, bool clear)
    {
        if (clear) tilemap.ClearAllTiles();
        int minX = pos.x - (this.buildingSize - 1);
        int maxX = pos.x + (this.buildingSize - 1);
        int minY = pos.y - (this.buildingSize - 1);
        int maxY = pos.y + (this.buildingSize - 1);
        CustomDictionary<Tile, List<Vector3Int>> positions = new CustomDictionary<Tile, List<Vector3Int>>();
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int currentPosition = new Vector3Int(x, y, 0);
                Tile previousTile = (Tile)tilemap.GetTile(currentPosition);
                tilemap.SetTile(currentPosition, tile);

                if (previousTile != tile)
                {
                    if (positions.ContainsKey(previousTile))
                    {
                        positions[previousTile].Add(currentPosition);
                    }
                    else
                    {
                        positions.Add(previousTile, new List<Vector3Int>() { currentPosition });
                    }
                }
            }
        }
        if(positions.Count() != 0) return new TileAction(positions, tilemap, tile);
        return null;
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
        if(buildingState == BuildingState.BuildingTiles || buildingState == BuildingState.DrawingRectangle || buildingState == BuildingState.DrawingLine || buildingState == BuildingState.Fill)
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

    /// <summary>
    /// Selects a tilemap and changes the infopanel
    /// </summary>
    /// <param name="tilemap"></param>
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
