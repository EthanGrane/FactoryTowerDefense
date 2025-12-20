using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Tiers")]
    public EnemyTierSO[] orderedTiers;

    public List<Enemy> enemies = new();

    public Action<Enemy> onEnemyDie;
    public Action onAllEnemiesDead;
    public Action onPathChanged;

    private List<Vector2Int> path;
    private bool pathDirty = true;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetPathDirty();
        RecalculateFlow();
        
        LogicManager.Instance.OnTick += RecalculateFlow;
    }

    public void SetPathDirty()
    {
        pathDirty = true;
    }
    
    private void RecalculateFlow()
    {
        if(!pathDirty) return;
        if(EnemyWavesManager.Instance.GetWavePhase() != WavePhase.Planning) return;
        
        pathDirty = false;
        
        path = new List<Vector2Int>();
        path = PathfindingAStar.Instance.GetPathToGoal();
        
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            enemy.currentPath = GetPathCentered().ToList();
            enemy.pathIndex = 0;
        }
        
        onPathChanged?.Invoke();
    }

    public Vector2Int[] GetPath() => path.ToArray();

    public Vector2[] GetPathCentered()
    {
        Vector2[] offsetPath = new Vector2[path.Count];
        for (int i = 0; i < path.Count; i++)
            offsetPath[i] = path[i] + Vector2.one * 0.5f;
        
        return offsetPath;
    }

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

    public void ProcessDamage(Enemy enemy, int damage)
    {
        for (int i = 0; i < damage; i++)
        {
            if(enemy.currentTierSo.canDropItem && enemy.currentTierSo.dropItem != null)
                GameManager.Instance.AddItemToInventory(enemy.currentTierSo.dropItem,enemy.currentTierSo.dropAmount);
            
            EnemyTierSO lower = GetLowerTier(enemy.currentTierSo);
            
            if (lower != null)
                ApplyTier(enemy, lower);
            else
            {
                // Enemy Death
                UnregisterEnemy(enemy);
                enemy.DieExtern();
                return;
            }
        }
    }

    public void ProcessDamageToBase(Enemy enemy)
    {
        int damage = enemy.currentTierSo.tierIndex + 1;
        GameManager.Instance.TakeDamage(damage);
    }

    public void ProcessDamage(Enemy enemy, Projectile projectile) => ProcessDamage(enemy, projectile.damage);
    
    public int GetEnemiesAliveCount() => enemies.Count;
}
