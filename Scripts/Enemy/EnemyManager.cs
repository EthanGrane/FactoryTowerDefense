using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Tiers")]
    public EnemyTier[] orderedTiers;
    
    public List<Enemy> enemies;

    private void Awake()
    {
        Instance = this;
    }

    // Obtiene el tier inmediatamente inferior
    public EnemyTier GetLowerTier(EnemyTier current)
    {
        int index = System.Array.IndexOf(orderedTiers, current);

        if (index > 0)
            return orderedTiers[index - 1];

        return null; // Red = no tiene inferior
    }

    // Aplica sprite, color y velocidad
    public void ApplyTier(Enemy enemy, EnemyTier tier)
    {
        RegisterEnemy(enemy);

        enemy.currentTier = tier;
        enemy.moveSpeed = tier.moveSpeed;

        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        sr.sprite = tier.sprite;
        Color color = tier.color;
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
            enemies.Remove(enemy);
    }
    
    public void ProcessDamage(Enemy enemy, int dmg)
    {
        for (int i = 0; i < dmg; i++)
        {
            EnemyTier lower = GetLowerTier(enemy.currentTier);

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
}