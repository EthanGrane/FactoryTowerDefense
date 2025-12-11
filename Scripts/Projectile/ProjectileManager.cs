using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager instance;
    public List<Projectile> projectiles = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    
    void Update()
    {
        float dt = Time.deltaTime;

        for (int i = 0; i < projectiles.Count; i++)
        {
            Projectile p = projectiles[i];

            float step = p.speed * dt;
            p.position += p.direction * step;
            p.lifetme -= dt;

            if (p.lifetme <= 0f || p.isDead)
            {
                projectiles.RemoveAt(i);
                i--;
                continue;
            }
        }
        
        CheckCollisionDetection();
    }

    void CheckCollisionDetection()
    {
        Enemy[] enemies = EnemyManager.Instance.GetAllEnemies();
        for (int i = 0; i < enemies.Length; i++)
        {
            for (int j = 0; j < projectiles.Count; j++)
            {
                Enemy enemy = enemies[i];
                Vector2 direction = projectiles[j].position - (Vector2)enemy.transform.position;
                if (Vector2.Distance(enemy.transform.position + (Vector3)(direction * enemy.collisionRadius), projectiles[j].position) <= projectiles[j].collisionRadius)
                {
                    EnemyManager.Instance.ProcessDamage(enemy, projectiles[j].damage);
                    projectiles[j].isDead = true;
                }            
            }
        }
    }

    public void RegisterProjectile(Projectile p)
    {
        projectiles.Add(p);
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(projectiles[i].position, projectiles[i].collisionRadius);
        }
    }
}