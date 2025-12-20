using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance;
    public List<Projectile> projectiles = new();

    ProjectileVisual projectileVisual;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        if(projectileVisual == null)
            projectileVisual = GetComponent<ProjectileVisual>();
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
                projectileVisual.UnregisterProjectile(p);
                projectiles.RemoveAt(i);
                i--;
                continue;
            }
        }
        
        CheckCollisionDetection();
    }

    void CheckCollisionDetection()
    {
        Enemy[] enemies = EnemyManager.Instance.enemies.ToArray();
        if(enemies == null || enemies.Length == 0) return;
        
        for (int i = 0; i < enemies.Length; i++)
        {
            for (int j = 0; j < projectiles.Count; j++)
            {
                Enemy enemy = enemies[i];
                Vector2 toProjectile = projectiles[j].position - (Vector2)enemy.transform.position;
                float dist = toProjectile.magnitude;
                if (dist <= enemy.collisionRadius + projectiles[j].collisionRadius)
                { 
                    Projectile p = projectiles[j];

                    if (p.hitEnemies.Contains(enemy))
                        continue;
                    p.hitEnemies.Add(enemy);
                    
                    EnemyManager.Instance.ProcessDamage(enemy, p);
                    
                    p.penetration--;
                    
                    if(p.penetration <= 0)
                        p.isDead = true;
                }            
            }
        }
    }
    
    public void SpawnProjectile(Vector2 pos, Vector2 dir, ProjectileSO data)
    {
        Projectile p = new Projectile(pos, dir, data);

        projectileVisual.RegisterProjectile(p,data);
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