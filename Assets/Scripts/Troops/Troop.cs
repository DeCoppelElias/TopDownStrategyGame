using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Troop : MonoBehaviour
{
    public List<Vector2> path = new List<Vector2>();
    protected int speed;
    public Castle castle;

    public void updateTroop()
    {
        if(path.Count > 0)
        {
            Vector2 currentPosition = transform.position;
            if (Vector2.Distance(currentPosition, path[0]) < 0.3)
            {
                path.RemoveAt(0);
            }
            else
            {
                var step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, path[0], step);
            }
        }
    }

    public void setPath(List<Vector2> path)
    {
        this.path = new List<Vector2>(path);
    }
}
