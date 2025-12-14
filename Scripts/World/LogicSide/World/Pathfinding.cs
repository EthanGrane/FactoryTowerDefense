using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public static Pathfinding Instance { get; private set; }

    [Header("Debug")]
    public bool showDebug = true;
    public Color pathColor = Color.green;
    public Color startColor = Color.blue;
    public Color endColor = Color.red;
    public Color obstacleColor = Color.yellow;
    public float gizmoSize = 0.3f;

    [Header("Pathfinding Settings")]
    public bool canBreakObstacles = true;
    public float obstacleBreakCostMultiplier = 1f;

    [Header("Movement Costs")]
    public float straightCost = 1f;
    public float diagonalCost = 2f;

    private Vector2Int? start = null;
    private Vector2Int? end = null;
    private List<Vector2Int> currentPath = null;
    private List<Vector2Int> obstaclesToBreak = null;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Public funcitons
    public List<Vector2Int> Calculate()
    {
        if (!start.HasValue)
        {
            Vector2 pos = FindFirstObjectByType<EnemySpawnPoint>().transform.position;
            start = new Vector2Int((int)pos.x, (int)pos.y);
        }
        
        if (!end.HasValue)
        {
            Vector2 pos = FindFirstObjectByType<PlayerBasePoint>().transform.position;
            end = new Vector2Int((int)pos.x, (int)pos.y);
        }


        return FindPath(start.Value, end.Value);
    }
    
    // SET
    public void SetStart(Vector2Int p) => start = p;
    public void SetEnd(Vector2Int p)   => end = p;
    
    // GET
    public Vector2Int? GetStart() => start;
    public Vector2Int? GetEnd() => end;
    
    // Private funcitons
    
    private Vector2Int? GetMouseWorldPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.FloorToInt(mousePos.x);
        int y = Mathf.FloorToInt(mousePos.y);

        Vector2Int pos = new Vector2Int(x, y);
        return IsValid(pos) ? pos : null;
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Tile[,] tiles = World.Instance.GetTiles();

        if (!IsValid(start) || !IsValid(end))
            return null;

        Tile endTile = tiles[end.x, end.y];
        if (endTile == null || endTile.terrainSO == null || endTile.terrainSO.solid)
            return null;

        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        openList.Add(new Node(start, null, 0, GetHeuristic(start, end)));

        while (openList.Count > 0)
        {
            Node current = GetLowestFNode(openList);

            if (current.position == end)
                return ReconstructPath(current);
            
            openList.Remove(current);
            closedList.Add(current.position);

            foreach (Vector2Int neighbor in GetNeighbors(current.position))
            {
                if (closedList.Contains(neighbor))
                    continue;

                Tile tile = tiles[neighbor.x, neighbor.y];
                if (tile == null || tile.terrainSO == null || tile.terrainSO.solid)
                    continue;

                bool isDiagonal =
                    Mathf.Abs(neighbor.x - current.position.x) == 1 &&
                    Mathf.Abs(neighbor.y - current.position.y) == 1;

                float movementCost = tile.terrainSO.movementCost;
                movementCost *= isDiagonal ? diagonalCost : straightCost;

                // 🔹 COSTE ADICIONAL POR PROXIMIDAD A MUROS
                movementCost += GetProximityToWallCost(neighbor, tiles);

                // Si hay obstáculo rompible
                if (tile.building?.block != null && tile.building.block.solid)
                {
                    if (!canBreakObstacles)
                        continue;

                    movementCost += tile.building.block.blockHealth * obstacleBreakCostMultiplier;
                    obstaclesToBreak?.Add(neighbor);
                }

                float newG = current.g + movementCost;

                Node existing = openList.Find(n => n.position == neighbor);
                if (existing == null)
                {
                    openList.Add(new Node(neighbor, current, newG, GetHeuristic(neighbor, end)));
                }
                else if (newG < existing.g)
                {
                    existing.g = newG;
                    existing.parent = current;
                }
            }

        }

        return null;
    }
    
    private float GetProximityToWallCost(Vector2Int pos, Tile[,] tiles)
    {
        // Valores de coste por cercanía (distancia Manhattan)
        int[] costs = { 5, 3, 2, 1 };
        float extraCost = 0f;

        for (int radius = 1; radius <= costs.Length; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        continue; // Solo borde del anillo

                    Vector2Int check = new Vector2Int(pos.x + dx, pos.y + dy);
                    if (!IsValid(check))
                        continue;

                    Tile t = tiles[check.x, check.y];
                    if (t != null && t.terrainSO != null && t.terrainSO.solid)
                    {
                        extraCost += costs[radius - 1];
                        // Una vez detectado un muro en este anillo, no seguir buscando en el mismo radio
                        break;
                    }
                }
            }
        }

        return extraCost;
    }
    
    private bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < World.WorldSize &&
               pos.y >= 0 && pos.y < World.WorldSize;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        Vector2Int[] dirs =
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),

            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        foreach (Vector2Int d in dirs)
        {
            Vector2Int n = pos + d;
            if (IsValid(n))
                neighbors.Add(n);
        }

        return neighbors;
    }

    private float GetHeuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return straightCost * (dx + dy)
             + (diagonalCost - 2f * straightCost) * Mathf.Min(dx, dy);
    }

    private Node GetLowestFNode(List<Node> list)
    {
        Node best = list[0];
        foreach (Node n in list)
            if (n.f < best.f)
                best = n;
        return best;
    }

    private List<Vector2Int> ReconstructPath(Node end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node current = end;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }
    
    private class Node
    {
        public Vector2Int position;
        public Node parent;
        public float g;
        public float h;
        public float f => g + h;

        public Node(Vector2Int pos, Node parent, float g, float h)
        {
            position = pos;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }
}
