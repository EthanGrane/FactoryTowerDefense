using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }
    public const int WorldSize = 100;
    private Tile[,] tiles;
    
    public TerrainSO[] terrains;
    private Dictionary<TileBase, TerrainSO> terrainByTileBase;

    public Tilemap terrainTilemap;
    
    private void Awake()
    {
        #region Singleton Awake
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        #endregion

        // Terrain Dictionary
        terrainByTileBase = new Dictionary<TileBase, TerrainSO>();
        foreach (var t in terrains)
        {
            terrainByTileBase[t.sprite] = t;
        }
        
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        tiles = new Tile[WorldSize, WorldSize];

        for (int x = 0; x < WorldSize; x++)
        {
            for (int y = 0; y < WorldSize; y++)
            {
                TerrainSO terrainSo = terrainByTileBase[terrainTilemap.GetTile(new Vector3Int(x,y,0))];
                
                tiles[x, y] = new Tile(new Vector2Int(x, y), terrainSo, null);

                // Set Terrain Colliders
                /*
                if (terrainSo != null)
                {
                    if (terrainSo.solid)
                        terrainCollider.SetTile(new Vector3Int(x, y, 0), colliderTile);
                    else
                        terrainCollider.SetTile(new Vector3Int(x, y, 0), null);   
                }
                else
                {
                    Debug.Log("NULL on x:{x} y:{y} postiion");
                    Debug.Break();
                }
                */
            }
        }
    }

    public Tile[,] GetTiles() => tiles;

    public Tile GetTile(int x, int y) => tiles[x, y];
    public Tile GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);
    
    public Building GetBuilding(int x, int y) => GetTile(x, y).building;
    public Building GetBuilding(Vector2Int pos) => GetTile(pos).building;
    
    public List<Tile> GetNeighbors(Vector2Int pos)
    {
        List<Tile> neighbors = new List<Tile>();

        // Direcciones cardinales (arriba, derecha, abajo, izquierda)
        Vector2Int[] dirs = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (var dir in dirs)
        {
            int nx = pos.x + dir.x;
            int ny = pos.y + dir.y;

            // Verificar límites del mundo
            if (nx < 0 || nx >= WorldSize || ny < 0 || ny >= WorldSize)
                continue;

            Tile tile = GetTile(nx, ny);
            if (tile != null && tile.building != null)
            {
                neighbors.Add(tile);
            }
        }

        return neighbors;
    }

    public static Vector2Int GetDirectionFromRotation(int rotation)
    {
        switch (rotation)
        {
            case 0:
                return Vector2Int.up;
            case 1:
                return Vector2Int.right;
            case 2:
                return Vector2Int.down;
            case 3:
                return Vector2Int.left;
            default:
                return Vector2Int.up;
        }
    }
    
    public static bool IsBuildFacingPosition(Building building, Vector2Int pos)
    {
        Vector2Int dir = GetDirectionFromRotation(building.rotation);
        if(building.position + dir == pos) return true;
        return false;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(WorldSize * Vector3.one * 0.5f, WorldSize * Vector3.one);
    }
}