using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    public Tilemap buildingCollider;
    public TileBase colliderTile;

    private World world;
    private WorldRenderer worldRenderer;

    // Bloque de prueba para testear
    public Block testBlock;
    public int rotation = 0;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if(worldRenderer == null)
            worldRenderer = WorldRenderer.Instance;
    }

    private void Update()
    {
        // Build
        if (Input.GetMouseButton(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePos = worldRenderer.terrainTilemap.WorldToCell(mouseWorld);
            Build(tilePos.x, tilePos.y, testBlock, rotation);
        }

        // Remove
        if (Input.GetMouseButton(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePos = worldRenderer.terrainTilemap.WorldToCell(mouseWorld);
            RemoveBuilding(tilePos.x, tilePos.y);
        }

        // Rotate
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotation += 1;
            rotation %= 4;
        }
    }

    public bool CanBuild(int startX, int startY, Block block)
    {
        if(world == null)
            world = World.Instance;
        
        Tile[,] tiles = world.GetTiles();

        for (int x = 0; x < block.size; x++)
        {
            for (int y = 0; y < block.size; y++)
            {
                int tx = startX + x;
                int ty = startY + y;

                if (tx < 0 || ty < 0 || tx >= tiles.GetLength(0) || ty >= tiles.GetLength(1))
                    return false;

                Tile tile = tiles[tx, ty];
                if (tile == null || tile.TerrainSo.solid || tile.building != null)
                    return false;
            }
        }

        return true;
    }

    public bool Build(int startX, int startY, Block block, int rotation = 0)
    {
        if (!CanBuild(startX, startY, block))
            return false;

        Tile[,] tiles = world.GetTiles();

        // Crear building una sola vez
        Building building = new Building { block = block, rotation = rotation, position = new Vector2Int(startX, startY) };

        // Crear lógica una sola vez si tiene
        if (block.logicType != null)
        {
            building.logic = (BuildingLogic)System.Activator.CreateInstance(block.logicType);
            building.logic.building = building;

            // Activar update
            building.logic.update = true;

            LogicManager.Instance.Register(building.logic);
            
            // Se llama al colocarlo
            building.logic.OnPlaced();
        }

        // Asignar building a cada tile del área
        for (int x = 0; x < block.size; x++)
        {
            for (int y = 0; y < block.size; y++)
            {
                int tx = startX + x;
                int ty = startY + y;
                Tile tile = tiles[tx, ty];

                tile.building = building;
                worldRenderer.SetTileVisual(tx, ty, tile);

                if (block.solid)
                    buildingCollider.SetTile(new Vector3Int(tx, ty, 0), colliderTile);
            }
        }

        return true;
    }


    public void RemoveBuilding(int startX, int startY)
    {
        if(world == null) world = World.Instance;
        Tile tile = world.GetTile(startX, startY);

        if (tile == null || tile.building == null) return;

        Building building = tile.building;
        tile.building = null; // Borra el building del tile
        worldRenderer.SetTileVisual(startX, startY, tile);
        buildingCollider.SetTile(new Vector3Int(startX, startY, 0), null);
        
        LogicManager.Instance.Unregister(building.logic);
    }

}
