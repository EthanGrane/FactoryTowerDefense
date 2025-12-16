using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public EnemyTierSO currentTierSo;
    public float collisionRadius = 0.5f;
    [HideInInspector] public float moveSpeed = 2f;

    private bool isAlive = true;
    private Vector2 lastDir;

    // ================= PATHFINDING =================
    [HideInInspector] public List<Vector2Int> currentPath;
    [HideInInspector] public int pathIndex = 0;

    private void Start()
    {
        EnemyManager.Instance.ApplyTier(this, currentTierSo);
        RecalculatePath();
    }

    private void Update()
    {
        if (!isAlive) return;
        
        // Movimiento hacia el siguiente nodo del path
        Vector2 targetPos = currentPath[pathIndex];
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        dir = Vector2.Lerp(lastDir, dir, 0.2f); // suavizado
        lastDir = dir;

        transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);

        // Si llegamos al nodo, avanzar al siguiente
        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
            pathIndex++;
    }
    public void DieExtern()
    {
        if (!isAlive) return;

        isAlive = false;
        Destroy(gameObject);
    }

    public Vector2 GetPosition() => transform.position;
    public Vector2 GetVelocity() => lastDir * moveSpeed;

    // ================= PATHFINDING =================
    public void RecalculatePath()
    {
        if (!isAlive) return;

        currentPath = EnemyManager.Instance.GetPath().ToList();
        pathIndex = 0;
    }
}
