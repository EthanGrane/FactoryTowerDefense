using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(World))]
public class WorldRenderer : MonoBehaviour
{
    public static WorldRenderer Instance { get; private set; }

    private World world;

    public GameObject TestTile;
    public GameObject ornamentTile;
    
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

        terrainTilemap.enabled = false;
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

        // Terrain
        // terrainTilemap.SetTile(pos, tile.terrainSO.sprite);
        float height = -0.5f;
        if (tile.terrainSO.solid) 
            height = .5f;
        GameObject threeDimTile = Instantiate(TestTile, new Vector3(pos.x,height,pos.y), Quaternion.identity);
        if (tile.terrainSO.solid) 
            threeDimTile.AddComponent<BoxCollider>();
        
        if(tile.terrainSO.name == "Stone")
            Instantiate(ornamentTile, new Vector3(pos.x,height,pos.y), Quaternion.identity);
        
        // Building
        if (tile.building == null)
        {
            buildingTilemap.SetTile(pos, null);
            buildingTilemap.SetTransformMatrix(pos, Matrix4x4.identity);
            return;
        }

        Building b = tile.building;

        if (b.position.x != x || b.position.y != y)
        {
            buildingTilemap.SetTile(pos, null);
            buildingTilemap.SetTransformMatrix(pos, Matrix4x4.identity);
            return;
        }

        buildingTilemap.SetTile(pos, b.block.blockTile);

        Vector3 centerOffset = new Vector3(
            (b.block.size - 1) * 0.5f,
            (b.block.size - 1) * 0.5f,
            0f
        );

        Quaternion rot = Quaternion.Euler(0f, 0f, b.rotation * -90f);
        Matrix4x4 trs = Matrix4x4.TRS(centerOffset, rot, Vector3.one);

        buildingTilemap.SetTransformMatrix(pos, trs);
    }


    
}