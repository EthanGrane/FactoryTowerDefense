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
    public float diagonalCost = 0.7f; // ⬅ DIAGONAL MÁS BARATA

    [Header("Testing")]
    public bool runTests = false;
    public KeyCode testKey = KeyCode.T;

    private Vector2Int? pointA = null;
    private Vector2Int? pointB = null;
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

    private void Update()
    {
        if (!showDebug) return;
        if (!Input.GetKey(KeyCode.LeftShift)) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int? clickPos = GetMouseWorldPosition();
            if (clickPos.HasValue)
            {
                pointA = clickPos.Value;
                if (pointB.HasValue) CalculatePath();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector2Int? clickPos = GetMouseWorldPosition();
            if (clickPos.HasValue)
            {
                pointB = clickPos.Value;
                if (pointA.HasValue) CalculatePath();
            }
        }
    }

    public void SetStartPoint(Vector2Int point) => pointA = point;
    public void SetEndPoint(Vector2Int point) => pointB = point;
    
    private Vector2Int? GetMouseWorldPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.FloorToInt(mousePos.x);
        int y = Mathf.FloorToInt(mousePos.y);

        Vector2Int pos = new Vector2Int(x, y);
        return IsValid(pos) ? pos : null;
    }

    public void CalculatePath()
    {
        currentPath = FindPath(pointA.Value, pointB.Value);
        obstaclesToBreak = currentPath != null ? new List<Vector2Int>() : null;
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
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

    public Vector3 GetNearestPos(Vector3 worldPos)
    {
        Vector2Int start = new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );

        Tile[,] tiles = World.Instance.GetTiles();

        // Clamp por si viene fuera del mundo
        start.x = Mathf.Clamp(start.x, 0, World.WorldSize - 1);
        start.y = Mathf.Clamp(start.y, 0, World.WorldSize - 1);

        // 1️⃣ Si el tile directo es válido, usarlo
        if (IsNavigable(start, tiles))
            return GridToWorld(start);

        // 2️⃣ Buscar alrededor por anillos
        int maxRadius = World.WorldSize; // seguro

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Solo borde del anillo (optimización)
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                        continue;

                    Vector2Int p = new Vector2Int(start.x + dx, start.y + dy);
                    if (!IsValid(p)) continue;

                    if (IsNavigable(p, tiles))
                        return GridToWorld(p);
                }
            }
        }

        // 3️⃣ Fallback (no debería pasar)
        return GridToWorld(start);
    }

    public Vector2Int? GetTargetNode() => pointB;
    public Vector2Int? GetStartNode() => pointA;
    

    private bool IsNavigable(Vector2Int pos, Tile[,] tiles)
    {
        Tile t = tiles[pos.x, pos.y];
        if (t == null || t.terrainSO == null)
            return false;

        if (t.terrainSO.solid)
            return false;

        if (!canBreakObstacles &&
            t.building?.block != null &&
            t.building.block.solid)
            return false;

        return true;
    }

    private Vector3 GridToWorld(Vector2Int pos)
    {
        return new Vector3(
            pos.x + 0.5f,
            pos.y + 0.5f,
            0f
        );
    }

    
    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        // Dibujar punto A (Start)
        if (pointA.HasValue)
        {
            Gizmos.color = startColor;
            Vector3 posA = new Vector3(
                pointA.Value.x + 0.5f,
                pointA.Value.y + 0.5f,
                0
            );

            Gizmos.DrawSphere(posA, gizmoSize);
            Gizmos.DrawWireCube(posA, Vector3.one * 0.8f);
        }

        // Dibujar punto B (End)
        if (pointB.HasValue)
        {
            Gizmos.color = endColor;
            Vector3 posB = new Vector3(
                pointB.Value.x + 0.5f,
                pointB.Value.y + 0.5f,
                0
            );

            Gizmos.DrawSphere(posB, gizmoSize);
            Gizmos.DrawWireCube(posB, Vector3.one * 0.8f);
        }

        // Dibujar obstáculos que se van a romper
        if (obstaclesToBreak != null && obstaclesToBreak.Count > 0)
        {
            Gizmos.color = obstacleColor;

            foreach (Vector2Int pos in obstaclesToBreak)
            {
                Vector3 worldPos = new Vector3(
                    pos.x + 0.5f,
                    pos.y + 0.5f,
                    0
                );

                Gizmos.DrawWireCube(worldPos, Vector3.one * 0.9f);
                Gizmos.DrawSphere(worldPos, gizmoSize * 0.5f);
            }
        }

        // Dibujar camino
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = pathColor;

            // Esferas en cada punto del camino
            foreach (Vector2Int pos in currentPath)
            {
                Vector3 worldPos = new Vector3(
                    pos.x + 0.5f,
                    pos.y + 0.5f,
                    0
                );

                Gizmos.DrawSphere(worldPos, gizmoSize * 0.7f);
            }

            // Líneas del camino
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector3 from = new Vector3(
                    currentPath[i].x + 0.5f,
                    currentPath[i].y + 0.5f,
                    0
                );

                Vector3 to = new Vector3(
                    currentPath[i + 1].x + 0.5f,
                    currentPath[i + 1].y + 0.5f,
                    0
                );

                Gizmos.DrawLine(from, to);
            }
        }
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
