using UnityEngine;

public class TurretLogic : BuildingLogic
{
    public int projectileRateCount = 0;
    public override void Initialize(Block block)
    {
        base.Initialize(block);
    }

    public override void Tick()
    {
        if (projectileRateCount > 0)
        {
            projectileRateCount--;
            return;
        }

        TurretBlock turretBlock = building.block as TurretBlock;
        Vector2 pos = building.position;

        Enemy[] enemies = EnemyManager.Instance.GetEnemiesOnRadius(pos, turretBlock.turretRange);
        if (enemies.Length == 0) return;

        // Encontrar enemigo más cercano
        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (var e in enemies)
        {
            float d = Vector2.Distance(pos, e.GetPosition());
            if (d < nearestDistance)
            {
                nearestDistance = d;
                nearestEnemy = e;
            }
        }

        if (nearestEnemy == null) return;

        // Calcular predicción
        Vector2 enemyPos = nearestEnemy.GetPosition();
        Vector2 enemyVel = nearestEnemy.GetVelocity();
        float travelTime = nearestDistance / turretBlock.projectileSpeed;
        Vector2 predictedPos = enemyPos + enemyVel * travelTime;

        // Calcular dirección normalizada
        Vector2 direction = (predictedPos - pos).normalized;

        // Registrar proyectil
        projectileRateCount = turretBlock.projectileRateOnTicks;
        ProjectileManager.instance.RegisterProjectile(
            new Projectile(
                pos,
                direction,
                turretBlock.projectileSpeed,
                turretBlock.projectileCollisionRadius,
                turretBlock.projectileLifetime,
                turretBlock.projectileDamage
            ));
    }

 }
