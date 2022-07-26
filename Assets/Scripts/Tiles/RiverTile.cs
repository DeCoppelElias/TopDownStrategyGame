using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RiverTile : Tile
{
    public Sprite[] m_Sprites;

    // This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        for (int yd = -1; yd <= 1; yd++)
        {
            for (int xd = -1; xd <= 1; xd++)
            {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if (HasRiverTile(tilemap, position))
                    tilemap.RefreshTile(position);
            }
        }
    }
    // This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
    // As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        Vector2Int index = GetIndex(location, tilemap);
        int spriteIndex = index[0];
        int rotationIndex = index[1];
        if (spriteIndex >= 0 && spriteIndex < m_Sprites.Length)
        {
            tileData.sprite = m_Sprites[spriteIndex];
            tileData.color = Color.white;
            var m = tileData.transform;
            m.SetTRS(Vector3.zero, Quaternion.Euler(0f, 0f, -90f * rotationIndex), Vector3.one);
            tileData.transform = m;
            tileData.flags = TileFlags.LockAll;
            tileData.colliderType = ColliderType.None;
        }
        else
        {
            Debug.LogWarning("Not enough sprites in RiverTile instance");
        }
    }

    // This determines if the Tile at the position is the same SelectingTile.
    private bool HasRiverTile(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) is RiverTile;
    }

    private bool checkMatrix(int[,] original, int[,] check)
    {
        for (int rowIndex = 0; rowIndex < original.GetLength(0); rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < original.GetLength(1); columnIndex++)
            {
                if(check[rowIndex, columnIndex] != -1 && check[rowIndex, columnIndex] != original[rowIndex, columnIndex])
                {
                    return false;
                }
            }
        }

        return true;
    }

    // The following determines which sprite to use based on the number of adjacent RoadTiles
    private Vector2Int GetIndex(Vector3Int location, ITilemap tilemap)
    {
        
        int[,] mask = new int[,] { 
            { 0, 0, 0 }, 
            { 0, 1, 0 }, 
            { 0, 0, 0 } };

        mask[0, 1] = HasRiverTile(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
        mask[1, 2] = HasRiverTile(tilemap, location + new Vector3Int(1, 0, 0)) ? 1 : 0;
        mask[2, 1] = HasRiverTile(tilemap, location + new Vector3Int(0, -1, 0)) ? 1 : 0;
        mask[1, 0] = HasRiverTile(tilemap, location + new Vector3Int(-1, 0, 0)) ? 1 : 0;

        mask[0, 2] = HasRiverTile(tilemap, location + new Vector3Int(1, 1, 0)) ? 1 : 0;
        mask[2, 2] = HasRiverTile(tilemap, location + new Vector3Int(1, -1, 0)) ? 1 : 0;
        mask[0, 0] = HasRiverTile(tilemap, location + new Vector3Int(-1, 1, 0)) ? 1 : 0;
        mask[2, 0] = HasRiverTile(tilemap, location + new Vector3Int(-1, -1, 0)) ? 1 : 0;

        // One side not river tiles (tile 0)
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 1 },
            { 1, 1, 1 } }))
        {
            return new Vector2Int(0, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, -1 },
            { 1, 1, 0 },
            { 1, 1, -1 } }))
        {
            return new Vector2Int(0, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(0, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 1, 1 },
            { 0, 1, 1 },
            { -1, 1, 1 } }))
        {
            return new Vector2Int(0, 3);
        }

        // 2 opposing corner not river tiles (tile 1)
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 0 } }))
        {
            return new Vector2Int(1, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 1 } }))
        {
            return new Vector2Int(1, 1);
        }

        //Only one direct side is river tile (tile 2)
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 0, 1, 0 },
            { -1, 1, -1 } }))
        {
            return new Vector2Int(2, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 0 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(2, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 1, -1 },
            { 0, 1, 0 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(2, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 0, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(2, 3);
        }

        // Only one full corner is river tiles (tile 3)
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 0, 1, 1 },
            { -1, 1, 1 } }))
        {
            return new Vector2Int(3, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 0 },
            { 1, 1, -1 } }))
        {
            return new Vector2Int(3, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, -1 },
            { 1, 1, 0 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(3, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 1, 1 },
            { 0, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(3, 3);
        }

        // The whole river tile (tile 4)
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 } }))
        {
            return new Vector2Int(4, 0);
        }

        // 2 direct tiles not river tiles (tile 5)
        if (checkMatrix(mask, new int[,] {
            { -1, 1, 0 },
            { 0, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(5, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 0, 1, 1 },
            { -1, 1, 0 } }))
        {
            return new Vector2Int(5, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 0 },
            { 0, 1, -1 } }))
        {
            return new Vector2Int(5, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { 0, 1, -1 },
            { 1, 1, 0 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(5, 3);
        }

        // One corner not river tile (tile 6)
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 } }))
        {
            return new Vector2Int(6, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 0 },
            { 1, 1, 1 },
            { 1, 1, 1 } }))
        {
            return new Vector2Int(6, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 0 } }))
        {
            return new Vector2Int(6, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 1 } }))
        {
            return new Vector2Int(6, 3);
        }

        // 3 corner not river tiles (tile 7)
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 } }))
        {
            return new Vector2Int(7, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 } }))
        {
            return new Vector2Int(7, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 1 } }))
        {
            return new Vector2Int(7, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 1, 1, 0 } }))
        {
            return new Vector2Int(7, 3);
        }

        // 2 sides not river tiles (tile 8)
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(8, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 1, -1 },
            { 0, 1, 0 },
            { -1, 1, -1 } }))
        {
            return new Vector2Int(8, 1);
        }

        // one side 2 corners not river tiles (tile 9)
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(9, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 1, 0 },
            { 0, 1, 1 },
            { -1, 1, 0 } }))
        {
            return new Vector2Int(9, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 1 },
            { 0, 1, 0 } }))
        {
            return new Vector2Int(9, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { 0, 1, -1 },
            { 1, 1, 0 },
            { 0, 1, -1 } }))
        {
            return new Vector2Int(9, 3);
        }

        // 4 corner not river tiles (tile 10)
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 0, 1, 0 } }))
        {
            return new Vector2Int(10, 0);
        }

        // 2 corners not river tiles (tile 11)
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 0 },
            { 1, 1, 1 },
            { 1, 1, 1 } }))
        {
            return new Vector2Int(11, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 0 },
            { 1, 1, 1 },
            { 1, 1, 0 } }))
        {
            return new Vector2Int(11, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 0 } }))
        {
            return new Vector2Int(11, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 1 },
            { 1, 1, 1 },
            { 0, 1, 1 } }))
        {
            return new Vector2Int(11, 3);
        }

        // no river tiles
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 0, 1, 0 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(12, 0);
        }

        // One side one corner right (tile 13)
        if (checkMatrix(mask, new int[,] {
            { 1, 1, 0 },
            { 1, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(13, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 1, 1 },
            { 0, 1, 1 },
            { -1, 1, 0 } }))
        {
            return new Vector2Int(13, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 1 },
            { 0, 1, 1 } }))
        {
            return new Vector2Int(13, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { 0, 1, -1 },
            { 1, 1, 0 },
            { 1, 1, -1 } }))
        {
            return new Vector2Int(13, 3);
        }

        // One side one corner left (tile 14)
        if (checkMatrix(mask, new int[,] {
            { 0, 1, 1 },
            { 1, 1, 1 },
            { -1, 0, -1 } }))
        {
            return new Vector2Int(14, 0);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 1, 0 },
            { 0, 1, 1 },
            { -1, 1, 1 } }))
        {
            return new Vector2Int(14, 1);
        }
        if (checkMatrix(mask, new int[,] {
            { -1, 0, -1 },
            { 1, 1, 1 },
            { 1, 1, 0 } }))
        {
            return new Vector2Int(14, 2);
        }
        if (checkMatrix(mask, new int[,] {
            { 1, 1, -1 },
            { 1, 1, 0 },
            { 0, 1, -1 } }))
        {
            return new Vector2Int(14, 3);
        }

        return new Vector2Int(this.m_Sprites.Length-1, 0);
    }

#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/RiverTile")]
    public static void CreateRiverTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save River Tile", "New River Tile", "Asset", "Save River Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RiverTile>(), path);
    }
#endif
}
