using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int maxPlayers;
    public List<Vector2> castleLocations;

    private void Start()
    {
        if (castleLocations.Count != maxPlayers)
        {
            throw new System.Exception("The amount of castles and players is not the same");
        }
    }
}
