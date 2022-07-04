using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SelectingTile : Tile
{
    public Sprite[] m_Sprites;
    public Vector2Int topRight;
    public Vector2Int bottomLeft;

    // This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        for (int yd = -1; yd <= 1; yd++)
            for (int xd = -1; xd <= 1; xd++)
            {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if (HasSelectingTile(tilemap, position))
                    tilemap.RefreshTile(position);
            }
    }
    // This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
    // As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        int index = GetIndex(location, tilemap);
        //Debug.Log(index);
        if (index >= 0 && index < m_Sprites.Length)
        {
            tileData.sprite = m_Sprites[index];
            tileData.color = Color.white;
            var m = tileData.transform;
            m.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
            tileData.transform = m;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = ColliderType.None;
        }
        else
        {
            Debug.LogWarning("Not enough sprites in RoadTile instance");
        }
    }

    // This determines if the Tile at the position is the same SelectingTile.
    private bool HasSelectingTile(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) is SelectingTile;
    }

    // The following determines which sprite to use based on the number of adjacent RoadTiles
    private int GetIndex(Vector3Int location, ITilemap tilemap)
    {
        if(topRight == bottomLeft)
        {
            return 0;
        }
        if(location.y == topRight.y)
        {
            if(HasSelectingTile(tilemap, location + new Vector3Int(-1, 0, 0)) && HasSelectingTile(tilemap, location + new Vector3Int(1, 0, 0)))
            {
                if (topRight.y == bottomLeft.y)
                {
                    // Top/Bottom 2 sides
                    return 14;
                }
                // Top
                return 1;
            }
            if (HasSelectingTile(tilemap, location + new Vector3Int(1, 0, 0)))
            {
                if (topRight.y == bottomLeft.y)
                {
                    // Left 3 sides
                    return 11;
                }
                
                // Top Left
                return 7;
            }
            if (HasSelectingTile(tilemap, location + new Vector3Int(-1, 0, 0)))
            {
                if (topRight.y == bottomLeft.y)
                {
                    // Right 3 sides
                    return 12;
                }
                // Top Right
                return 5;
            }
            if (topRight.x == bottomLeft.x)
            {
                // Top 3 sides
                return 9;
            }
        }
        if (location.y == bottomLeft.y)
        {
            if (HasSelectingTile(tilemap, location + new Vector3Int(-1, 0, 0)) && HasSelectingTile(tilemap, location + new Vector3Int(1, 0, 0)))
            {
                if(topRight.y == bottomLeft.y)
                {
                    // Top/Bottom 2 sides
                    return 14;
                }
                // Bottom
                return 2;
            }
            if (HasSelectingTile(tilemap, location + new Vector3Int(1, 0, 0)))
            {
                if (topRight.y == bottomLeft.y)
                {
                    // Left 3 sides
                    return 11;
                }
                // Bottom Left
                return 8;
            }
            if (HasSelectingTile(tilemap, location + new Vector3Int(-1, 0, 0)))
            {
                if (topRight.y == bottomLeft.y)
                {
                    // Right 3 sides
                    return 12;
                }
                // Bottom Right
                return 6;
            }
            if (topRight.x == bottomLeft.x)
            {
                // Bottom 3 sides
                return 10;
            }
        }

        if (location.x == bottomLeft.x)
        {
            if (HasSelectingTile(tilemap, location + new Vector3Int(0, -1, 0)) && HasSelectingTile(tilemap, location + new Vector3Int(0, 1, 0)))
            {
                if(bottomLeft.x == topRight.x)
                {
                    // Left/Right 2 sides
                    return 13;
                }
                // Left
                return 4;
            }
        }
        if (location.x == topRight.x)
        {
            if (HasSelectingTile(tilemap, location + new Vector3Int(0, -1, 0)) && HasSelectingTile(tilemap, location + new Vector3Int(0, 1, 0)))
            {
                if (bottomLeft.x == topRight.x)
                {
                    // Left/Right 2 sides
                    return 13;
                }
                // Right
                return 3;
            }
        }

        return 0;
    }

#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/SelectingTile")]
    public static void CreateSelectingTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Selecting Tile", "New Selecting Tile", "Asset", "Save Selecting Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SelectingTile>(), path);
    }
#endif
}
