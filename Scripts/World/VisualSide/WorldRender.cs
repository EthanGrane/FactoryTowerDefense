using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[RequireComponent(typeof(World))]
public class WorldRenderer : MonoBehaviour
{
    public static WorldRenderer Instance { get; private set; }

    private World world;

    public GameObject TestTile;
    public GameObject ornamentTile;
    
    public Tilemap terrainTilemap;
    public Tilemap buildingTilemap;
    
    public Dictionary<Vector3Int, GameObject> buildingObjects = new Dictionary<Vector3Int, GameObject>();

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
        float height = 0f;
        if (tile.terrainSO.solid) 
            height = 1f;
        GameObject threeDimTile = Instantiate(TestTile, new Vector3(pos.x + .5f,height,pos.y+ .5f), Quaternion.identity);
        
        if(tile.terrainSO.name == "Stone")
        {
            GameObject ornament = Instantiate(ornamentTile, new Vector3(pos.x+ .5f, height + Random.Range(-0.025f,0f), pos.y+ .5f), Quaternion.Euler(0,90 * Random.Range((int)1,4),0));
        }        
        
        // Building
        if (tile.building == null)
        {
            if (buildingObjects.TryGetValue(pos, out var obj))
            {
                Destroy(obj);
                buildingObjects.Remove(pos);
            }


            return;
            // Remove building visual
            buildingTilemap.SetTile(pos, null);
            buildingTilemap.SetTransformMatrix(pos, Matrix4x4.identity);
            return;
        }

        Building b = tile.building;

        if (b.position.x != x || b.position.y != y)
        {
            if (buildingObjects.TryGetValue(pos, out var obj))
            {
                Destroy(obj);
                buildingObjects.Remove(pos);
            }

            return;
            // Remove building visual
            buildingTilemap.SetTile(pos, null);
            buildingTilemap.SetTransformMatrix(pos, Matrix4x4.identity);
            return;
        }

        
        // buildingTilemap.SetTile(pos, b.block.blockTile);
        if (buildingObjects.TryGetValue(pos, out var oldObj))
        {
            Destroy(oldObj);
            buildingObjects.Remove(pos);
        }
        GameObject buildingObject = Instantiate(TestTile, new Vector3(pos.x + 0.5f, height, pos.y + 0.5f), Quaternion.identity);
        buildingObject.GetComponent<MeshFilter>().mesh = tile.building.block.mesh;
        buildingObjects[pos] = buildingObject;

        
        // Rotation on tilemap
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