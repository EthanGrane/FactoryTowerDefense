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

        // Click izquierdo (botón 0) - Punto A (Start)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2Int? clickPos = GetMouseWorldPosition();
            if (clickPos.HasValue)
            {
                pointA = clickPos.Value;
                Debug.Log($"Punto A establecido en: {pointA}");
                
                if (pointB.HasValue)
                {
                    CalculatePath();
                }
            }
        }

        // Click derecho (botón 1) - Punto B (End)
        if (Input.GetMouseButtonDown(1))
        {
            Vector2Int? clickPos = GetMouseWorldPosition();
            if (clickPos.HasValue)
            {
                pointB = clickPos.Value;
                Debug.Log($"Punto B establecido en: {pointB}");
                
                if (pointA.HasValue)
                {
                    CalculatePath();
                }
            }
        }
    }

    private Vector2Int? GetMouseWorldPosition()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int x = Mathf.FloorToInt(mousePos.x);
        int y = Mathf.FloorToInt(mousePos.y);

        if (IsValid(new Vector2Int(x, y)))
        {
            return new Vector2Int(x, y);
        }

        return null;
    }

    private void CalculatePath()
    {
        if (!pointA.HasValue || !pointB.HasValue)
            return;

        currentPath = FindPath(pointA.Value, pointB.Value);

        if (currentPath != null)
        {
            PrintPathDetails(currentPath, "CAMINO ENCONTRADO");
        }
        else
        {
            Debug.Log("No se encontró camino");
            obstaclesToBreak = null;
        }
    }

    private void PrintPathDetails(List<Vector2Int> path, string title)
    {
        float totalCost = 0f;
        float terrainCost = 0f;
        float obstacleCost = 0f;
        obstaclesToBreak = new List<Vector2Int>();
        
        Tile[,] tiles = World.Instance.GetTiles();
        
        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int pos = path[i];
            Tile tile = tiles[pos.x, pos.y];
            
            // Coste de terreno
            terrainCost += tile.terrainSO.movementCost;
            
            // Coste de obstáculos
            if (tile.building != null && tile.building.block != null && tile.building.block.solid)
            {
                float breakCost = tile.building.block.blockHealth * obstacleBreakCostMultiplier;
                obstacleCost += breakCost;
                obstaclesToBreak.Add(pos);
            }
        }
        
        totalCost = terrainCost + obstacleCost;
        
        Debug.Log($"=== {title} ===");
        Debug.Log($"Pasos totales: {path.Count}");
        Debug.Log($"Coste terreno: {terrainCost:F2}");
        Debug.Log($"Coste obstáculos: {obstacleCost:F2}");
        Debug.Log($"Obstáculos a romper: {obstaclesToBreak.Count}");
        Debug.Log($"COSTE TOTAL: {totalCost:F2}");
    }

    private void RunPathfindingTests()
    {
        Debug.Log("========================================");
        Debug.Log("INICIANDO TESTS DE PATHFINDING");
        Debug.Log("========================================");

        // TEST 1: 100 tiles vs 1 obstáculo con vida 50
        Test_LongPathVsObstacle();

        // TEST 2: Comparar diferentes multiplicadores
        Test_MultiplierComparison();

        Debug.Log("========================================");
        Debug.Log("TESTS COMPLETADOS");
        Debug.Log("========================================");
    }

    private void Test_LongPathVsObstacle()
    {
        Debug.Log("\n--- TEST 1: 100 Tiles (coste 1) vs 1 Obstáculo (vida 50) ---");
        
        // Escenario teórico
        float longPathCost = 100 * 1f; // 100 tiles con coste 1
        float obstaclePathCost = 50 * obstacleBreakCostMultiplier; // 1 obstáculo con 50 vida
        
        Debug.Log($"Camino largo (100 tiles): {longPathCost}");
        Debug.Log($"Romper obstáculo (50 vida × {obstacleBreakCostMultiplier}): {obstaclePathCost}");
        
        if (longPathCost < obstaclePathCost)
        {
            Debug.Log($"✓ RESULTADO: Es más barato rodear ({longPathCost} < {obstaclePathCost})");
        }
        else if (longPathCost > obstaclePathCost)
        {
            Debug.Log($"✓ RESULTADO: Es más barato romper ({obstaclePathCost} < {longPathCost})");
        }
        else
        {
            Debug.Log($"✓ RESULTADO: Ambos caminos cuestan igual ({longPathCost})");
        }

        // Si hay puntos A y B establecidos, probar con el mapa real
        if (pointA.HasValue && pointB.HasValue)
        {
            Debug.Log("\n--- Probando en el mapa actual ---");
            List<Vector2Int> path = FindPath(pointA.Value, pointB.Value);
            if (path != null)
            {
                PrintPathDetails(path, "Camino calculado en mapa real");
            }
        }
    }

    private void Test_MultiplierComparison()
    {
        Debug.Log("\n--- TEST 2: Comparación de Multiplicadores ---");
        
        float obstacleHealth = 50f;
        float[] multipliers = { 0.5f, 1f, 2f, 3f, 5f };
        
        Debug.Log($"Para un obstáculo con {obstacleHealth} de vida:");
        foreach (float mult in multipliers)
        {
            float cost = obstacleHealth * mult;
            Debug.Log($"  Multiplicador {mult}: Coste = {cost}");
            
            // Comparar con camino largo
            if (cost < 100)
                Debug.Log($"    → Más barato que 100 tiles");
            else if (cost > 100)
                Debug.Log($"    → Más caro que 100 tiles");
            else
                Debug.Log($"    → Igual que 100 tiles");
        }

        Debug.Log($"\nMultiplicador actual: {obstacleBreakCostMultiplier}");
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Tile[,] tiles = World.Instance.GetTiles();
        
        if (!IsValid(start) || !IsValid(end))
            return null;

        // Verificar que el tile de destino existe
        Tile endTile = tiles[end.x, end.y];
        if (endTile == null || endTile.terrainSO == null)
            return null;

        // Si el terreno es sólido, no se puede llegar
        if (endTile.terrainSO.solid)
            return null;

        List<Node> openList = new List<Node>();
        HashSet<Vector2Int> closedList = new HashSet<Vector2Int>();

        Node startNode = new Node(start, null, 0, GetHeuristic(start, end));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node current = GetLowestFNode(openList);
            
            if (current.position == end)
            {
                return ReconstructPath(current);
            }

            openList.Remove(current);
            closedList.Add(current.position);

            foreach (Vector2Int neighbor in GetNeighbors(current.position))
            {
                if (closedList.Contains(neighbor))
                    continue;

                Tile neighborTile = tiles[neighbor.x, neighbor.y];
                
                // Verificar null
                if (neighborTile == null || neighborTile.terrainSO == null)
                    continue;

                // Si el terreno es sólido (agua, montañas), no se puede pasar
                if (neighborTile.terrainSO.solid)
                    continue;

                // Calcular coste de movimiento
                float movementCost = neighborTile.terrainSO.movementCost;
                
                // Añadir coste de romper obstáculo si existe (estilo Clash of Clans)
                if (canBreakObstacles && 
                    neighborTile.building != null && 
                    neighborTile.building.block != null && 
                    neighborTile.building.block.solid)
                {
                    // El coste depende de la vida del obstáculo
                    // Cuanta más vida, más costoso es romperlo
                    movementCost += neighborTile.building.block.blockHealth * obstacleBreakCostMultiplier;
                }
                else if (!canBreakObstacles && 
                         neighborTile.building != null && 
                         neighborTile.building.block != null && 
                         neighborTile.building.block.solid)
                {
                    // Si no puede romper obstáculos, tratarlo como bloqueado
                    continue;
                }
                
                float newG = current.g + movementCost;

                Node existingNode = openList.Find(n => n.position == neighbor);

                if (existingNode == null)
                {
                    Node newNode = new Node(neighbor, current, newG, GetHeuristic(neighbor, end));
                    openList.Add(newNode);
                }
                else if (newG < existingNode.g)
                {
                    existingNode.g = newG;
                    existingNode.parent = current;
                }
            }
        }

        return null;
    }

    private bool IsValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < World.WorldSize && 
               pos.y >= 0 && pos.y < World.WorldSize;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Arriba
            new Vector2Int(0, -1),  // Abajo
            new Vector2Int(-1, 0),  // Izquierda
            new Vector2Int(1, 0)    // Derecha
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbor = pos + dir;
            if (IsValid(neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private float GetHeuristic(Vector2Int a, Vector2Int b)
    {
        // Distancia Manhattan
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Node GetLowestFNode(List<Node> list)
    {
        Node lowest = list[0];
        foreach (Node node in list)
        {
            if (node.f < lowest.f)
                lowest = node;
        }
        return lowest;
    }

    private List<Vector2Int> ReconstructPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node current = endNode;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;

        // Dibujar punto A (Start)
        if (pointA.HasValue)
        {
            Gizmos.color = startColor;
            Vector3 posA = new Vector3(pointA.Value.x + 0.5f, pointA.Value.y + 0.5f, 0);
            Gizmos.DrawSphere(posA, gizmoSize);
            Gizmos.DrawWireCube(posA, Vector3.one * 0.8f);
        }

        // Dibujar punto B (End)
        if (pointB.HasValue)
        {
            Gizmos.color = endColor;
            Vector3 posB = new Vector3(pointB.Value.x + 0.5f, pointB.Value.y + 0.5f, 0);
            Gizmos.DrawSphere(posB, gizmoSize);
            Gizmos.DrawWireCube(posB, Vector3.one * 0.8f);
        }

        // Dibujar obstáculos que se van a romper
        if (obstaclesToBreak != null && obstaclesToBreak.Count > 0)
        {
            Gizmos.color = obstacleColor;
            foreach (Vector2Int pos in obstaclesToBreak)
            {
                Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
                Gizmos.DrawWireCube(worldPos, Vector3.one * 0.9f);
                Gizmos.DrawSphere(worldPos, gizmoSize * 0.5f);
            }
        }

        // Dibujar camino
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = pathColor;
            
            // Dibujar esferas en cada punto del camino
            foreach (Vector2Int pos in currentPath)
            {
                Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);
                Gizmos.DrawSphere(worldPos, gizmoSize * 0.7f);
            }

            // Dibujar líneas conectando el camino
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector3 from = new Vector3(currentPath[i].x + 0.5f, currentPath[i].y + 0.5f, 0);
                Vector3 to = new Vector3(currentPath[i + 1].x + 0.5f, currentPath[i + 1].y + 0.5f, 0);
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

        public Node(Vector2Int position, Node parent, float g, float h)
        {
            this.position = position;
            this.parent = parent;
            this.g = g;
            this.h = h;
        }
    }
}