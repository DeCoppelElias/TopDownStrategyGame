using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AVL;

public class PathFinding : MonoBehaviour
{
    [SerializeField]
    private bool display = false;
    [SerializeField]
    private float displayActiveTime = 1;
    private enum DebugState {Offline, SelectingStart, SelectingFinish, Searching, Done}
    [SerializeField]
    private DebugState debugState = DebugState.Offline;
    private bool debugFinished;

    [SerializeField]
    private Vector3Int debugStart = new Vector3Int(0, 0, 0);
    [SerializeField]
    private Vector3Int debugFinish = new Vector3Int(0, 0, 0);

    [SerializeField]
    private Tilemap obstacleTilemap;
    [SerializeField]
    private Tilemap displayTilemap;
    [SerializeField]
    private Tile displayPathTile;
    [SerializeField]
    private Tile displaySearchedTile;
    [SerializeField]
    private Tile displayErrorTile;
    [SerializeField]
    private float debugUpdateSpeed = 1;
    private float lastUpdate = 0;

    private AVL avlTree = new AVL();
    private Dictionary<int, Node> storedNodes = new Dictionary<int, Node>();
    private Dictionary<int,int> expandedNodes = new Dictionary<int,int>();
    private Node currentNode;
    private int counter = 0;

    private void Start()
    {
        
    }

    private void test()
    {
        GameObject.Find("SaveLoadLevel").GetComponent<SaveLoadLevel>().loadLevel("Level-1");
        this.debugStart = new Vector3Int(-52, 4, 0);
        this.debugFinish = new Vector3Int(29, 7, 0);
        this.debugState = DebugState.SelectingStart;
    }

    private void Update()
    {
        if (debugState == DebugState.SelectingStart)
        {
            if (debugStart != new Vector3Int(0, 0, 0))
            {
                this.debugState = DebugState.SelectingFinish;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                this.debugStart = Vector3Int.FloorToInt(getMousePosition());
                this.debugState = DebugState.SelectingFinish;
            }
        }
        else if (debugState == DebugState.SelectingFinish)
        {
            if (debugFinish != new Vector3Int(0, 0, 0))
            {
                findPathDebug();
                this.debugState = DebugState.Searching;
                this.displayTilemap.ClearAllTiles();
            }
            if (Input.GetMouseButtonDown(0))
            {
                this.debugFinish = Vector3Int.FloorToInt(getMousePosition());

                findPathDebug();
                this.debugState = DebugState.Searching;
                this.displayTilemap.ClearAllTiles();
            }
        }
        else if (debugState == DebugState.Searching)
        {
            float startTime = Time.time;
            debugUpdate();
            float endTime = Time.time;
            //Debug.Log(endTime - startTime);

            if (debugFinished)
            {
                resetDebug();
            }
        }
    }

    private Vector3 getMousePosition()
    {
        Vector3 result = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        result.z = 0;
        return result;
    }

    /// <summary>
    /// Will update the visual debug if debug is enabled
    /// </summary>
    private void debugUpdate()
    {
        if (Time.time - lastUpdate > debugUpdateSpeed)
        {
            findPathStep();
            lastUpdate = Time.time;
        }
    }

    /// <summary>
    /// This method will find a path from start to finish using A* algorithm
    /// </summary>
    /// <param name="start"></param>
    /// <param name="finish"></param>
    /// <returns></returns>
    public List<Vector2> findPath(Vector3Int start, Vector3Int finish)
    {
        if (display)
        {
            displayTilemap.ClearAllTiles();
        }
        int counter = 0;
        Dictionary<int, Node> storedNodes = new Dictionary<int, Node>();
        Dictionary<int, int> expandedNodes = new Dictionary<int, int>();
        AVL avlTree = new AVL();

        Node firstNode = new Node(0, 0, start, null);
        avlTree.Add(firstNode);
        storedNodes.Add(firstNode.GetHashCode(),firstNode);

        Node currentNode = avlTree.PopMinValue();
        while(currentNode.tilePosition != finish && counter < 10000)
        {
            if (display)
            {
                displayTilemap.SetTile(currentNode.tilePosition, displaySearchedTile);
            }

            counter++;
            List<Vector3Int> neighbors = getNeighbors(currentNode.tilePosition);
            foreach (Vector3Int neighborPosition in neighbors)
            {
                float distanceToFinish = Vector3Int.Distance(neighborPosition, finish);
                float distancePath = currentNode.distancePath + Vector3Int.Distance(currentNode.tilePosition, neighborPosition);
                Node neighborNode = new Node(distancePath, distanceToFinish,  neighborPosition, currentNode);
                int hashCode = neighborNode.GetHashCode();

                if (!expandedNodes.ContainsKey(hashCode))
                {
                    if (storedNodes.ContainsKey(hashCode))
                    {
                        Node node = storedNodes[hashCode];
                        if (distancePath < node.distancePath)
                        {
                            avlTree.Delete(node);
                            storedNodes.Remove(hashCode);

                            avlTree.Add(neighborNode);
                            storedNodes.Add(hashCode, neighborNode);
                        }
                    }
                    else
                    {
                        avlTree.Add(neighborNode);
                        storedNodes.Add(neighborNode.GetHashCode(), neighborNode);
                    }
                }
            }
            currentNode = avlTree.PopMinValue();
            expandedNodes.Add(currentNode.GetHashCode(), 0);
            storedNodes.Remove(currentNode.GetHashCode());
        }

        List<Vector2> path = new List<Vector2>();
        while (currentNode.previousNode != null)
        {
            Vector2 currentPosition = (Vector3)currentNode.tilePosition;
            path.Add(currentPosition + new Vector2(0.5f,0.5f));
            if (display)
            {
                displayTilemap.SetTile(currentNode.tilePosition, displayPathTile);
            }
            currentNode = currentNode.previousNode;
        }
        Vector2 lastPosition = (Vector3)currentNode.tilePosition;
        path.Add(lastPosition);
        path.Reverse();

        Invoke("clearDisplay", this.displayActiveTime);
        return path;
    }
    public List<Vector2> findPath(Vector3 s, Vector3 f)
    {
        Vector3Int start = Vector3Int.FloorToInt(s);
        Vector3Int finish = Vector3Int.FloorToInt(f);

        return findPath(start, finish);
    }

    /// <summary>
    /// This will initialize the pathfinding
    /// </summary>
    private void findPathDebug()
    {
        if (obstacleTilemap.GetTile(debugStart))
        {
            Debug.Log("Start is an obstacle");
            resetDebug();
            return;
        }
        else if (obstacleTilemap.GetTile(debugFinish))
        {
            Debug.Log("Finish is an obstacle");
            resetDebug();
            return;
        }
        else
        {
            this.counter = 0;
            this.storedNodes = new Dictionary<int, Node>();
            this.expandedNodes = new Dictionary<int, int>();
            this.avlTree = new AVL();

            Node firstNode = new Node(0, 0, debugStart, null);
            avlTree.Add(firstNode);
            storedNodes.Add(firstNode.GetHashCode(), firstNode);

            this.currentNode = avlTree.PopMinValue();
            displayTilemap.SetTile(currentNode.tilePosition, displaySearchedTile);
        }
    }

    private void resetDebug()
    {
        this.debugState = DebugState.Done;
        this.debugFinished = false;
        this.debugStart = new Vector3Int(0, 0, 0);
        this.debugFinish = new Vector3Int(0, 0, 0);
    }

    private void clearDisplay()
    {
        this.displayTilemap.ClearAllTiles();
    }

    /// <summary>
    /// This will update the pathfinding with one tile
    /// </summary>
    private void findPathStep()
    {
        try
        {
            displayTilemap.SetTile(currentNode.tilePosition, displaySearchedTile);
            if (currentNode.tilePosition != debugFinish && counter < 10000)
            {
                counter++;
                List<Vector3Int> neighbors = getNeighbors(currentNode.tilePosition);
                foreach (Vector3Int neighborPosition in neighbors)
                {
                    float distanceToFinish = Vector3Int.Distance(neighborPosition, debugFinish);
                    float distancePath = currentNode.distancePath + Vector3Int.Distance(currentNode.tilePosition, neighborPosition);
                    Node neighborNode = new Node(distancePath, distanceToFinish, neighborPosition, currentNode);
                    int hashCode = neighborNode.GetHashCode();

                    if (!expandedNodes.ContainsKey(hashCode))
                    {
                        if (storedNodes.ContainsKey(hashCode))
                        {
                            Node node = storedNodes[hashCode];
                            if (distancePath < node.distancePath)
                            {
                                avlTree.Delete(node);
                                storedNodes.Remove(hashCode);

                                avlTree.Add(neighborNode);
                                storedNodes.Add(hashCode, neighborNode);
                            }
                        }
                        else
                        {
                            avlTree.Add(neighborNode);
                            storedNodes.Add(neighborNode.GetHashCode(), neighborNode);
                        }
                    }
                }
                currentNode = avlTree.PopMinValue();
                expandedNodes.Add(currentNode.GetHashCode(),0);
                storedNodes.Remove(currentNode.GetHashCode());
            }
            else if (counter < 10000)
            {
                List<Vector3> path = new List<Vector3>();
                while (currentNode.previousNode != null)
                {
                    path.Add(currentNode.tilePosition + new Vector3(0.5f,0.5f,0));
                    displayTilemap.SetTile(currentNode.tilePosition, displayPathTile);
                    currentNode = currentNode.previousNode;
                }
                path.Reverse();

                string resultString = "Found path:";
                foreach (Vector3 position in path)
                {
                    resultString += " " + position + " ";
                }
                Debug.Log(resultString);
                this.debugFinished = true;
                
            }
            else
            {
                Debug.Log("No path was found");
            }
        }
        catch(Exception exception)
        {
            displayTilemap.SetTile(currentNode.tilePosition, displayErrorTile);

            Debug.Log("Counter: " + counter);
            Debug.Log(exception.Message);
            Debug.Log(exception.StackTrace);

            throw exception;
        }
    }

    /// <summary>
    /// This will get all valid neighbours of a certain position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private List<Vector3Int> getNeighbors(Vector3Int position)
    {
        List<Vector3Int> result = new List<Vector3Int>();
        List<Vector3Int> neighborPositionsNormal = new List<Vector3Int>() { position + new Vector3Int(1, 0, 0), position + new Vector3Int(0, 1, 0), position + new Vector3Int(-1, 0, 0), position + new Vector3Int(0, -1, 0)};

        List<Vector3Int> neighborPositionsDiagonal = new List<Vector3Int>() {position + new Vector3Int(1, 1, 0), position + new Vector3Int(-1, 1, 0), position + new Vector3Int(1, -1, 0), position + new Vector3Int(-1, -1, 0)};

        foreach (Vector3Int neighborPosition in neighborPositionsNormal)
        {
            if (obstacleTilemap.GetTile(neighborPosition) == null)
            {
                result.Add(neighborPosition);
            }
        }
        foreach (Vector3Int neighborPosition in neighborPositionsDiagonal)
        {
            Vector3Int side1 = new Vector3Int(neighborPosition.x, position.y, 0);
            Vector3Int side2 = new Vector3Int(position.x, neighborPosition.y, 0);
            if (obstacleTilemap.GetTile(neighborPosition) == null && (obstacleTilemap.GetTile(side1) == null || obstacleTilemap.GetTile(side2) == null))
            {
                result.Add(neighborPosition);
            }
        }
        return result;
    }
}
