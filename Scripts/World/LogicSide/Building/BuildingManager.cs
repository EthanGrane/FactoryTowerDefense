using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance { get; private set; }

    [Header("Tilemaps")]
    public Tilemap buildingCollider;
    public TileBase colliderTile;
    
    public Block[] blocks;
    
    [Header("Building Ghost")]
    public GameObject ghostPrefab;
    GameObject ghostObject;
    
    private World world;
    private WorldRenderer worldRenderer;
    [CanBeNull] Block selectedBlock = null;
    int rotation = 0;
    
    // Events
    public Action<Block> onBlockSelected;
    
    // TOOL FOR TESTIG (BORRAR)
    bool noMaterialsNeeded = false;
    
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
        
        if (ghostObject != null)
            Destroy(ghostObject);
        
        ghostObject = Instantiate(ghostPrefab, transform);
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        Vector3 mouseWorldXZ = GetMouseWorldPositionXZ();

        // Tilemap
        int gridX = Mathf.FloorToInt(mouseWorldXZ.x);
        int gridZ = Mathf.FloorToInt(mouseWorldXZ.z);

        Vector3Int tilePos = new Vector3Int(gridX, 0, gridZ);

        
        HandleGhostBuilding(tilePos);

        if (Input.GetMouseButton(0))
            Build(tilePos.x, tilePos.z, selectedBlock, rotation);

        if (Input.GetMouseButton(1))
        {
            if(selectedBlock == null)
                RemoveBuilding(tilePos.x, tilePos.z);
            else
                SetSelectedBlock(null);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            rotation = (rotation + 1) % 4;
        }

        if(Input.GetKeyDown(KeyCode.F1)) noMaterialsNeeded = !noMaterialsNeeded;
    }

    void HandleGhostBuilding(Vector3Int tilePos)
    {
        if (selectedBlock == null)
        {
            ghostObject.SetActive(false);
            return;
        }

        ghostObject.SetActive(true);

        float size = selectedBlock.size;
        Vector3 centerOffset = new Vector3(
            (size - 1) * 0.5f,
            0f,
            (size - 1) * 0.5f
        );

        ghostObject.transform.position = new Vector3(
            tilePos.x + 0.5f + centerOffset.x,
            0f,
            tilePos.z + 0.5f + centerOffset.z
        );

        // Rotación correcta para XZ
        ghostObject.transform.rotation = Quaternion.Euler(
            90f,
            rotation * 90f,
            0f
        );

        SpriteRenderer sr = ghostObject.GetComponent<SpriteRenderer>();
        sr.sprite = selectedBlock.sprite;

        sr.color = CanBuild(tilePos.x, tilePos.z, selectedBlock)
            ? new Color(1f, 1f, 1f, 0.9f)
            : new Color(1f, 0.1f, 0.1f, 0.9f);
    }
    
    public void SelectBlock(Block block)
    {
        if (blocks.Contains(block))
        {
            if(selectedBlock == block)
                SetSelectedBlock(null);
            else
                SetSelectedBlock(block);
            
        }
    }

    void SetSelectedBlock(Block block)
    {
        selectedBlock = block;
        onBlockSelected?.Invoke(block);
    }

    public bool CanBuild(int startX, int startY, Block block)
    {
        if (world == null)
            world = World.Instance;

        Tile[,] tiles = world.GetTiles();

        // Comprobar inventario
        if (!noMaterialsNeeded)
        {
            if (block.buildingCost != null && block.buildingCost.Length != 0)
            {
                Inventory playerInv = GameManager.Instance.GetPlayerInventory();
                foreach (var cost in block.buildingCost)
                {
                    if (!playerInv.Contains(cost.requieredItem, cost.amount))
                        return false;
                }
            }
        }

        Vector2Int[] path = null;
        bool checkPath = EnemyWavesManager.Instance.GetWavePhase() != WavePhase.Planning;
        if (checkPath)
            path = EnemyManager.Instance.GetPath();

        // Recorrer todos los tiles del multibloque
        for (int x = 0; x < block.size; x++)
        {
            for (int y = 0; y < block.size; y++)
            {
                int tx = startX + x;
                int ty = startY + y;

                // Limites del mundo
                if (tx < 0 || ty < 0 || tx >= tiles.GetLength(0) || ty >= tiles.GetLength(1))
                    return false;

                Tile tile = tiles[tx, ty];

                if (tile == null || tile.terrainSO.solid || tile.building != null)
                    return false;

                // Revisar path de enemigos
                if (checkPath && path.Contains(tile.position))
                    return false;
            }
        }

        return true;
    }

    public bool Build(int startX, int startY, Block block, int rotation = 0)
    {
        if (!block) return false;
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

                /*
                if (block.solid)
                    buildingCollider.SetTile(new Vector3Int(tx, ty, 0), colliderTile);
                    */
            }
        }

        for (int i = 0; i < block.buildingCost.Length; i++)
        {
            GameManager.Instance.RemoveItem(block.buildingCost[i].requieredItem, block.buildingCost[i].amount);
        }
        
        worldRenderer.SetTileVisual(startX, startY, tiles[startX,startY]);
        EnemyManager.Instance.SetPathDirty();
        
        return true;
    }
    public bool RemoveBuilding(int startX, int startY)
    {
        if (world == null) world = World.Instance;

        Tile origin = world.GetTile(startX, startY);
        if (origin == null || origin.building == null) return false;

        Building building = origin.building;
        
        if(!building.block.CanBeRemovedByPlayer) return false;
        
        Vector2Int buildingPos = building.position;
        int size = building.block.size;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int tx = buildingPos.x + x;
                int ty = buildingPos.y + y;

                Tile t = world.GetTile(tx, ty);
                if (t != null)
                {
                    t.building = null;
                    worldRenderer.SetTileVisual(tx, ty, t);
                }
                
                /*
                buildingCollider.SetTile(new Vector3Int(tx, ty, 0), null);
                */
            }
        }
        
        
        for (int i = 0; i < building.block.buildingCost.Length; i++)
        {
            GameManager.Instance.AddItemToInventory(building.block.buildingCost[i].requieredItem, building.block.buildingCost[i].amount);
        }

        LogicManager.Instance.Unregister(building.logic);
        EnemyManager.Instance.SetPathDirty();

        return true;
    }

    Vector3 GetMouseWorldPositionXZ()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Y=0
        if (groundPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter); // Esto devuelve un Vector3 en XZ
        }
        return Vector3.zero;
    }

}
