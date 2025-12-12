using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(World))]
public class WorldRenderer : MonoBehaviour
{
    public static WorldRenderer Instance { get; private set; }

    private World world;

    public Tilemap terrainTilemap;
    public Tilemap buildingTilemap;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        world = World.Instance;
        RenderAll();
    }
    
    private void RenderAll()
    {
        var tiles = world.GetTiles();

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                SetTileVisual(x, y, tiles[x, y]);
            }
        }
    }

    public void SetTileVisual(int x, int y, Tile tile)
    {
        Vector3Int pos = new Vector3Int(x, y, 0);

        // Terrain Visual
        terrainTilemap.SetTile(pos, tile.terrainSO.sprite);

        // Building Visual
        if (tile.building == null)
        {
            buildingTilemap.SetTile(pos, null);
            // Also clear flags so no stale transform remains:
            buildingTilemap.SetTransformMatrix(pos, Matrix4x4.identity);
        }        
        else
        {
            buildingTilemap.SetTile(pos, tile.building.block.blockTile);

            int size = tile.building.block.size;

            Vector3 centerOffset = new Vector3((size - 1) * 0.5f, (size - 1) * 0.5f, 0f);
            Quaternion rot = Quaternion.Euler(0f, 0f, tile.building.rotation * -90f);
            Matrix4x4 trs = Matrix4x4.TRS(centerOffset, rot, Vector3.one);

            buildingTilemap.SetTransformMatrix(pos, trs);
        }
    }

    
}