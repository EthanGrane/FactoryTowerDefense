using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Tiers")]
    public EnemyTierSO[] orderedTiers;

    public List<Enemy> enemies = new();

    public Action<Enemy> onEnemyDie;
    public Action onAllEnemiesDead;

    private List<Vector2Int> path;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RecalculateFlow();
    }

    private void RecalculateFlow()
    {
        path = new List<Vector2Int>();
        path = PathfindingAStar.Instance.GetPathToGoal();
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            enemy.currentPath = path;
            enemy.pathIndex = 0;
        }
    }

    public Vector2Int[] GetPath() => path.ToArray();
    
    /* =====================
     * ENEMY LOGIC 
     * ===================== */

    public EnemyTierSO GetLowerTier(EnemyTierSO current)
    {
        int index = Array.IndexOf(orderedTiers, current);
        return index > 0 ? orderedTiers[index - 1] : null;
    }

    public void ApplyTier(Enemy enemy, EnemyTierSO tierSo)
    {
        RegisterEnemy(enemy);

        enemy.currentTierSo = tierSo;
        enemy.moveSpeed = tierSo.moveSpeed;

        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        sr.sprite = tierSo.sprite;
        Color c = tierSo.color;
        c.a = 1;
        sr.color = c;
    }
    
    public Enemy[] GetEnemiesOnRadius(Vector2 center, float radius)
    {
        List<Enemy> enemiesDetected = new();

        for (int i = 0; i < enemies.Count; i++)
        {
            if (Vector2.Distance(center, enemies[i].transform.position) <= radius)
                enemiesDetected.Add(enemies[i]);
        }

        return enemiesDetected.ToArray();
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (!enemies.Contains(enemy)) return;

        enemies.Remove(enemy);
        onEnemyDie?.Invoke(enemy);

        if (enemies.Count == 0)
            onAllEnemiesDead?.Invoke();
    }

    public void ProcessDamage(Enemy enemy, Projectile projectile)
    {
        for (int i = 0; i < projectile.damage; i++)
        {
            EnemyTierSO lower = GetLowerTier(enemy.currentTierSo);
            if (lower != null)
                ApplyTier(enemy, lower);
            else
            {
                UnregisterEnemy(enemy);
                enemy.DieExtern();
                return;
            }
        }
    }
    
    public int GetEnemiesAliveCount() => enemies.Count;
}
