using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// In the DrawPathState this method will store a path made with the mouse
    /// </summary>
    public override void action()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!mouse)
        {
            if (Input.GetMouseButtonDown(0))
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

        Debug.Log("Starting to draw at position" + position);
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
            Debug.Log("Path must start at your castle");
        }
    }

    private void addToPath(Vector3 position)
    {
        if (Vector2.Distance(path[path.Count - 1], position) > 1)
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
                Debug.Log("Path crossed obstacle, please try again");
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
            if (castle && castle.Owner != this.clientStateManager.client)
            {
                found = true;
                break;
            }
        }
        if (found)
        {
            Debug.Log("Finished drawing");
            clientStateManager.sendPathToPlayer(path);
        }
        else
        {
            Debug.Log("Path will auto extent to Enemy closest enemy castle");
            Castle closestCastle = null;
            float distanceToClosest = 100000;
            foreach (GameObject castleGameObject in GameObject.FindGameObjectsWithTag("castle"))
            {
                Castle castle = castleGameObject.GetComponent<Castle>();
                float newDistance = Vector2.Distance(path[0], castle.transform.position);
                if (distanceToClosest > newDistance && castle != this.clientStateManager.client.castle)
                {
                    distanceToClosest = newDistance;
                    closestCastle = castle;
                }
            }
            PathFinding pathFinding = GameObject.Find("PathFinding").GetComponent<PathFinding>();
            List<Vector3> pathExtend = pathFinding.findPath(Vector3Int.FloorToInt(path[path.Count-1]), Vector3Int.FloorToInt(closestCastle.transform.position));
            foreach(Vector3 extendPosition in pathExtend)
            {
                path.Add(extendPosition);
            }
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
}
