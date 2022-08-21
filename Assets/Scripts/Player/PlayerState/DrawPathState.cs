using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawPathState : ClientState
{
    private List<Vector2> path = new List<Vector2>();
    private bool mouse = false;
    private LineRenderer lineRenderer;

    public DrawPathState(ClientStateManager p) : base(p)
    {
        this.lineRenderer = GameObject.Find("LineRenderer").GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    /// <summary>
    /// This method will keep track of the mouse position and create a list of vectors representing a path. When the mouse is no longer pressed down it will send the path to the ClientStateManager
    /// </summary>
    public override void action()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!mouse)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                startPath(mousePosition);
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                addToPath(mousePosition);
            }
            else
            {
                endPath(mousePosition);
            }
        }
    }

    private void startPath(Vector2 position)
    {
        this.path = new List<Vector2>();
        lineRenderer.gameObject.SetActive(true);

        //Debug.Log("Starting to draw at position" + position);
        Vector2 castlePosition = clientStateManager.getCastlePosition();
        if (Vector2.Distance(position, castlePosition) < 1)
        {
            mouse = true;
            this.path.Add(position);
            lineRenderer.positionCount += 1;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
        }
        else
        {
            DebugPanel.displayDebugMessage("Path must start at your castle");
            //Debug.Log("Path must start at your castle, please try again");
        }
    }

    private void addToPath(Vector3 position)
    {
        if (Vector2.Distance(path[path.Count - 1], position) > 0.2f)
        {
            RaycastHit2D[] hits = Physics2D.LinecastAll(path[path.Count - 1], position);
            bool valid = true;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.tag == "wall")
                {
                    valid = false;
                }
            }
            if (valid)
            {
                path.Add(position);
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            }
            else
            {
                //Debug.Log("Path crossed obstacle, please try again");
                DebugPanel.displayDebugMessage("Path crossed obstacle, please try again");
                resetPath();
            }
        }
    }

    private void endPath(Vector2 position)
    {
        bool found = false;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(position, 1))
        {
            Castle castle = collider.GetComponent<Castle>();
            if (castle && castle.Owner != this.clientStateManager.Client)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            //Debug.Log("Finished drawing");
            clientStateManager.sendPathToPlayer(path);
        }
        else
        {
            //Debug.Log("Path will auto extent to Enemy closest enemy castle");
            Castle closestCastle = null;
            float distanceToClosest = float.MaxValue;
            foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
            {
                Castle castle = castleGameObject.GetComponent<Castle>();
                float newDistance = Vector2.Distance(path[path.Count-1], castle.transform.position);
                if (distanceToClosest > newDistance && castle.Owner != this.clientStateManager.Client)
                {
                    distanceToClosest = newDistance;
                    closestCastle = castle;
                }
            }
            PathFinding pathFinding = GameObject.Find("PathFinding").GetComponent<PathFinding>();
            List<Vector2> pathExtend = pathFinding.findShortestPath(Vector3Int.FloorToInt(path[path.Count-1]), Vector3Int.FloorToInt(closestCastle.transform.position));
            path.AddRange(pathExtend);

            clientStateManager.sendPathToPlayer(path);
        }
        resetPath();
    }

    private void resetPath()
    {
        mouse = false;
        this.path = new List<Vector2>();
        this.lineRenderer.positionCount = 0;
    }

    public override void onExitState()
    {
        resetPath();
    }
}
