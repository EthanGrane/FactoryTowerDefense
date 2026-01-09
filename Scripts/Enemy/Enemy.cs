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
    private Vector3 lastDir;

    // ================= PATHFINDING =================
    [HideInInspector] public List<Vector2> currentPath;
    [HideInInspector] public int pathIndex = 0;

    private void Start()
    {
        EnemyManager.Instance.ApplyTier(this, currentTierSo);
        RecalculatePath();
    }

    private void Update()
    {
        if (!isAlive) return;

        if (currentPath == null || currentPath.Count == 0)
            return;

        if (pathIndex >= currentPath.Count)
        {
            OnPathFinished();
            return;
        }

        Vector3 targetPos = currentPath[pathIndex];
        targetPos = new Vector3(targetPos.x, 0, targetPos.y);
        Vector3 dir = (targetPos - transform.position).normalized;
        dir = Vector3.Lerp(lastDir, dir, 0.2f);
        lastDir = dir;
        
        transform.Translate(dir * (moveSpeed * Time.deltaTime), Space.World);
        
        transform.forward = dir;

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
            pathIndex++;
    }

    public void DieExtern()
    {
        if (!isAlive) return;

        isAlive = false;
        Destroy(gameObject);
    }
    
    void OnPathFinished()
    {
        if (!isAlive) return;

        EnemyManager.Instance.ProcessDamageToBase(this);

        EnemyManager.Instance.UnregisterEnemy(this);
        DieExtern();
    }

    
    public Vector3 GetPosition() => transform.position;
    public Vector3 GetVelocity() => lastDir * moveSpeed;
    
    // ================= PATHFINDING =================
    public void RecalculatePath()
    {
        if (!isAlive) return;

        currentPath = EnemyManager.Instance.GetPathCentered().ToList();
        pathIndex = 0;
    }
}
