using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPathState : PlayerState
{
    public List<Vector2> path = new List<Vector2>();
    public bool mouse = false;

    public DrawPathState(PlayerStateManager p) : base(p) { }
    public override void action()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!mouse)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Starting to draw at position" + mousePosition);
                Vector2 castlePosition = playerStateManager.getCastlePosition();
                if(Vector2.Distance(mousePosition,castlePosition) < 1)
                {
                    mouse = true;
                    path.Add(mousePosition);
                }
                else
                {
                    Debug.Log("Path must start at your castle");
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (Vector2.Distance(path[path.Count-1], mousePosition) > 1)
                {
                    RaycastHit2D[] hits = Physics2D.LinecastAll(path[path.Count - 1], mousePosition);
                    bool valid = true;
                    foreach(RaycastHit2D hit in hits)
                    {
                        if(hit.transform.tag == "wall")
                        {
                            valid = false;
                        }
                    }
                    if (valid)
                    {
                        Debug.Log("Added position" + mousePosition);
                        path.Add(mousePosition);
                    }
                    else
                    {
                        Debug.Log("Path crossed obstacle, please try again");
                        mouse = false;
                        path = new List<Vector2>();
                    }
                }
            }
            else
            {
                Debug.Log("Finished drawing");
                mouse = false;
                playerStateManager.sendPathToPlayer(path);
                path = new List<Vector2>();
            }
        }
    }
}
