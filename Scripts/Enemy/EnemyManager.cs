using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Tiers")]
    public EnemyTierSO[] orderedTiers;
    
    public List<Enemy> enemies;

    public Action<Enemy> onEnemyDie;
    public Action onAllEnemiesDead;
    public List<Vector2> enemyPath;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        enemyPath = new List<Vector2>();
    }
    
    // Obtiene el tier inmediatamente inferior
    public EnemyTierSO GetLowerTier(EnemyTierSO current)
    {
        int index = System.Array.IndexOf(orderedTiers, current);

        if (index > 0)
            return orderedTiers[index - 1];

        return null; // Red = no tiene inferior
    }

    // Aplica sprite, color y velocidad
    public void ApplyTier(Enemy enemy, EnemyTierSO tierSo)
    {
        RegisterEnemy(enemy);
        
        enemy.currentTierSo = tierSo;
        enemy.moveSpeed = tierSo.moveSpeed;

        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        sr.sprite = tierSo.sprite;
        Color color = tierSo.color;
        color.a = 1;
        sr.color = color;
    }
    
    public void RegisterEnemy(Enemy enemy)
    {
        if(!enemies.Contains(enemy))
            enemies.Add(enemy);
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
            onEnemyDie?.Invoke(enemy);
            
            if(enemies.Count == 0)
                onAllEnemiesDead?.Invoke();
        }    
    }
    
    public void ProcessDamage(Enemy enemy, int dmg)
    {
        for (int i = 0; i < dmg; i++)
        {
            EnemyTierSO lower = GetLowerTier(enemy.currentTierSo);

            if (lower != null)
            {
                ApplyTier(enemy, lower);
            }
            else
            {
                UnregisterEnemy(enemy);
                enemy.DieExtern();
                return;
            }
        }
    }

    public Enemy[] GetEnemiesOnRadius(Vector2 center, float radius)
    {
        List<Enemy> enemiesDetected = new List<Enemy>();
        
        for (int i = 0; i < enemies.Count; i++)
        {
            if(Vector2.Distance(center, enemies[i].transform.position) <= radius)
            {
                enemiesDetected.Add(enemies[i]);
            }        
        }

        return enemiesDetected.ToArray();
    }
    
    public Enemy[] GetAllEnemies() => enemies.ToArray();
    public List<Vector2> CalculatePath()
    {
        if (Pathfinding.Instance == null)
            return null;

        Vector2Int? start = Pathfinding.Instance.GetStartNode();
        Vector2Int? end   = Pathfinding.Instance.GetTargetNode();

        if (!start.HasValue || !end.HasValue)
        {
            return null;
        }

        List<Vector2Int> gridPath =
            Pathfinding.Instance.FindPath(start.Value, end.Value);

        if (gridPath == null || gridPath.Count == 0)
            return null;

        List<Vector2> path = new List<Vector2>(gridPath.Count);
        foreach (Vector2Int pos in gridPath)
        {
            path.Add(new Vector2(
                pos.x + 0.5f,
                pos.y + 0.5f
            ));
        }

        return path;
    }

    public List<Vector2> GetPath()
    {
       if(enemyPath == null || enemyPath.Count == 0)
       {
           enemyPath = new List<Vector2>();
           enemyPath = CalculatePath();
       }
       
       return enemyPath;
    }
    
    #region EDITOR HELPER
    #if UNITY_EDITOR
    [ContextMenu("Sync Tiers to ScriptableObjects")]
    private void SyncTiersToSO()
    {
        if (orderedTiers == null || orderedTiers.Length == 0)
        {
            Debug.LogWarning("No hay tiers en orderedTiers.");
            return;
        }

        for (int i = 0; i < orderedTiers.Length; i++)
        {
            EnemyTierSO tier = orderedTiers[i];
            if (tier == null) continue;

            tier.tierIndex = i;
            EditorUtility.SetDirty(tier);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Tiers asignados correctamente a los ScriptableObjects.");
    }
    #endif
    #endregion
}