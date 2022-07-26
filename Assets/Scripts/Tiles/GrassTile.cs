using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrassTile : Tile
{
    public Sprite[] normalSprites;
    public Sprite[] rareSprites;

    private class SpriteGenOptions
    {
        Sprite sprite;
        int spawnChance; //actual spawnchance is decided by (this spawn chance / sum of all spawn chances)

        public SpriteGenOptions(Sprite sprite, int spawnChance)
        {
            this.sprite = sprite;
            this.spawnChance = spawnChance;
        }
    }

    // This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        tilemap.RefreshTile(location);
    }
    // This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
    // As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        int index = GetIndex(location, tilemap);
        //Debug.Log(index);
        if (index < 0 || index >= this.normalSprites.Length + this.rareSprites.Length) return;
        
        else if (index < normalSprites.Length)
        {
            tileData.sprite = normalSprites[index];
        }
        else
        {
            index -= normalSprites.Length;
            tileData.sprite = rareSprites[index];
        }
        tileData.color = Color.white;
        var m = tileData.transform;
        m.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
        tileData.transform = m;
        tileData.flags = TileFlags.LockAll;
        tileData.colliderType = ColliderType.None;
    }

    // This determines if the Tile at the position is the same SelectingTile.
    private bool HasGrassTile(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) is GrassTile;
    }

    // The following determines which sprite to use based on the number of adjacent RoadTiles
    private int GetIndex(Vector3Int location, ITilemap tilemap)
    {
        int rarity = Random.Range(0, 10);
        if (rarity < 9)
        {
            return Random.Range(0, this.normalSprites.Length);
        }
        else
        {
            return this.normalSprites.Length + Random.Range(0, this.rareSprites.Length);
        }
    }

#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/GrassTile")]
    public static void CreateGrassTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Grass Tile", "New Grass Tile", "Asset", "Save Grass Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GrassTile>(), path);
    }
#endif
}
