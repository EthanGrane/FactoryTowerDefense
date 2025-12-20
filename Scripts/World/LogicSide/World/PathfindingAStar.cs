using System.Collections.Generic;
using UnityEngine;

public class PathfindingAStar : MonoBehaviour
{
    public static PathfindingAStar Instance { get; private set; }

    [Header("Pathfinding Settings")]
    public bool canBreakObstacles = true;
    public float obstacleBreakCostMultiplier = 1f;

    [Header("Wall Proximity Settings")]
    public float wallProximityCost = 5f; // Coste adicional por cercanía a muros
    public int wallProximityDistance = 5; // Distancia de penalización

    [Header("Movement Costs")]
    public float straightCost = 1f;

    private Vector2Int? start = null;
    private Vector2Int? end = null;

    public byte[,] wallDistance;
    
    // Vecinos con diagonales (los primeros 4 son cardinales, más eficiente)
    private static readonly Vector2Int[] Neighbors =
    {
        new Vector2Int(0, 1),   // Norte
        new Vector2Int(1, 0),   // Este
        new Vector2Int(0, -1),  // Sur
        new Vector2Int(-1, 0),  // Oeste
        new Vector2Int(1, 1),   // NE
        new Vector2Int(1, -1),  // SE
        new Vector2Int(-1, -1), // SO
        new Vector2Int(-1, 1)   // NO
    };

    private const float SQRT2 = 1.41421356f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        EnsureEndpoints();
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ========================= PUBLIC API =========================
    
    /// <summary>
    /// Encuentra un camino entre start y goal usando A*
    /// </summary>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        Tile[,] tiles = World.Instance.GetTiles();
        if (tiles == null) return null;

        // Asegurar que wallDistance está calculado
        if (wallDistance == null)
            RebuildWallDistance();

        Dictionary<Vector2Int, Node> allNodes = new(256); // Pre-allocate con capacidad razonable
        PriorityQueue<Node> open = new();
        HashSet<Vector2Int> closed = new(256);

        Node startNode = new(start, 0, Heuristic(start, goal), null);
        open.Enqueue(startNode, startNode.f);
        allNodes[start] = startNode;

        while (open.Count > 0)
        {
            Node current = open.Dequeue();

            if (current.pos == goal)
                return ReconstructPath(current);

            closed.Add(current.pos);

            foreach (Vector2Int dir in Neighbors)
            {
                Vector2Int n = current.pos + dir;
                
                // Validaciones tempranas
                if (!IsValid(n) || closed.Contains(n))
                    continue;

                Tile tile = tiles[n.x, n.y];
                if (tile?.terrainSO == null || tile.terrainSO.solid)
                    continue;

                // Verificar movimiento diagonal
                bool isDiagonal = dir.x != 0 && dir.y != 0;
                if (isDiagonal && !CanMoveDiagonal(current.pos, n, tiles))
                    continue;

                // Calcular coste del paso
                float stepCost = CalculateStepCost(tile, isDiagonal, n);
                if (stepCost < 0) continue; // Obstáculo infranqueable

                float newG = current.g + stepCost;

                // Verificar si ya existe un camino mejor
                if (allNodes.TryGetValue(n, out Node existing))
                {
                    if (newG >= existing.g) continue;
                    // Remover el nodo antiguo del open (será reemplazado)
                }

                float h = Heuristic(n, goal);
                Node node = new(n, newG, newG + h, current);
                allNodes[n] = node;
                open.Enqueue(node, node.f);
            }
        }

        return null; // No se encontró camino
    }

    /// <summary>
    /// Calcula el coste de moverse a una casilla considerando terreno, obstáculos y proximidad a muros
    /// </summary>
    private float CalculateStepCost(Tile tile, bool isDiagonal, Vector2Int position)
    {
        // Coste base de movimiento
        float stepCost = isDiagonal ? SQRT2 : 1f;
        
        // Coste del terreno
        stepCost *= tile.terrainSO.movementCost;

        // Coste de obstáculos rompibles
        if (tile.building?.block != null && tile.building.block.solid)
        {
            if (!canBreakObstacles) 
                return -1f; // No se puede pasar
            
            stepCost += tile.building.block.blockHealth * obstacleBreakCostMultiplier;
        }

        // Coste por proximidad a muros
        if (wallDistance != null && wallProximityCost > 0)
        {
            byte dist = wallDistance[position.x, position.y];
            if (dist < wallProximityDistance)
            {
                // Coste inversamente proporcional a la distancia
                // Distancia 0 = máximo coste, distancia 3+ = sin coste adicional
                float proximityFactor = 1f - (dist / (float)wallProximityDistance);
                stepCost += wallProximityCost * proximityFactor;
            }
        }

        return stepCost;
    }

    public List<Vector2Int> GetPathToGoal()
    {
        EnsureEndpoints();
        return FindPath(start.Value, end.Value);
    }
    
    public Vector2Int GetStart()
    {
        if (!start.HasValue) EnsureEndpoints();
        return start.Value;
    }

    public Vector2Int GetEnd()
    {
        if (!end.HasValue) EnsureEndpoints();
        return end.Value;
    }

    public void SetStart(Vector2Int p) => start = p;
    public void SetEnd(Vector2Int p) => end = p;

    // ========================= WALL DISTANCE =========================
    
    /// <summary>
    /// Recalcula la distancia a muros. Llamar cuando el terreno se modifique.
    /// </summary>
    public void RebuildWallDistance()
    {
        int size = World.WorldSize;
        Tile[,] tiles = World.Instance.GetTiles();
        
        if (wallDistance == null || wallDistance.GetLength(0) != size)
            wallDistance = new byte[size, size];

        Queue<Vector2Int> q = new(size * 4); // Pre-allocate

        // Inicializar: paredes = 0, resto = 255
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                if (tiles[x, y]?.terrainSO?.solid == true)
                {
                    wallDistance[x, y] = 0;
                    q.Enqueue(new Vector2Int(x, y));
                }
                else
                {
                    wallDistance[x, y] = 255;
                }
            }
        }

        // BFS para calcular distancia a muro (solo hasta la distancia necesaria)
        while (q.Count > 0)
        {
            Vector2Int p = q.Dequeue();
            byte d = wallDistance[p.x, p.y];
            
            // Optimización: solo calcular hasta la distancia que nos interesa
            if (d >= wallProximityDistance) continue;

            // Solo vecinos cardinales para distancia (más rápido y preciso)
            for (int i = 0; i < 4; i++)
            {
                Vector2Int n = p + Neighbors[i];
                if (!IsValid(n)) continue;
                
                byte newDist = (byte)(d + 1);
                if (wallDistance[n.x, n.y] > newDist)
                {
                    wallDistance[n.x, n.y] = newDist;
                    q.Enqueue(n);
                }
            }
        }
    }

    // ========================= AUXILIARES =========================
    
    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        // Octile distance (admisible para movimiento con diagonales)
        return Mathf.Min(dx, dy) * SQRT2 + Mathf.Abs(dx - dy);
    }

    private List<Vector2Int> ReconstructPath(Node node)
    {
        List<Vector2Int> path = new();
        while (node != null)
        {
            path.Add(node.pos);
            node = node.parent;
        }
        path.Reverse();
        return path;
    }

    private bool CanMoveDiagonal(Vector2Int from, Vector2Int to, Tile[,] tiles)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        Vector2Int sideA = new(from.x + dx, from.y);  // tile horizontal
        Vector2Int sideB = new(from.x, from.y + dy);  // tile vertical

        if (!IsValid(sideA) || !IsValid(sideB)) 
            return false;

        Tile tileA = tiles[sideA.x, sideA.y];
        Tile tileB = tiles[sideB.x, sideB.y];

        // Bloquear si cualquiera es terreno sólido
        if (tileA?.terrainSO?.solid == true || tileB?.terrainSO?.solid == true) 
            return false;

        // Bloquear si cualquiera tiene building sólido
        if (tileA?.building?.block?.solid == true || tileB?.building?.block?.solid == true)
            return false;

        return true;
    }


    private void EnsureEndpoints()
    {
        if (!start.HasValue)
        {
            var spawn = FindFirstObjectByType<EnemyBasePoint>();
            if (spawn) 
                start = new Vector2Int((int)spawn.transform.position.x, (int)spawn.transform.position.y);
        }
        if (!end.HasValue)
        {
            var basePoint = FindFirstObjectByType<PlayerBasePoint>();
            if (basePoint) 
                end = new Vector2Int((int)basePoint.transform.position.x + 1, (int)basePoint.transform.position.y + 1);
        }
    }

    private bool IsValid(Vector2Int p) => p.x >= 0 && p.x < World.WorldSize && p.y >= 0 && p.y < World.WorldSize;

    // ========================= NODO =========================
    class Node
    {
        public Vector2Int pos;
        public float g;
        public float f;
        public Node parent;

        public Node(Vector2Int p, float g, float f, Node parent)
        {
            this.pos = p;
            this.g = g;
            this.f = f;
            this.parent = parent;
        }
    }
}

// ========================= PRIORITY QUEUE OPTIMIZADA =========================
public class PriorityQueue<T>
{
    private readonly List<(T item, float priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority) => elements.Add((item, priority));

    public T Dequeue()
    {
        int bestIndex = 0;
        float bestPriority = elements[0].priority;

        for (int i = 1; i < elements.Count; i++)
        {
            if (elements[i].priority < bestPriority)
            {
                bestPriority = elements[i].priority;
                bestIndex = i;
            }
        }

        T item = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return item;
    }
}

// ========================= FLOWFIELD OPCIONAL =========================
public class FlowField
{
    public readonly int size;
    public readonly float[,] cost;
    public readonly Vector2[,] direction;

    public FlowField(int size)
    {
        this.size = size;
        cost = new float[size, size];
        direction = new Vector2[size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                cost[x, y] = float.MaxValue;
    }
}