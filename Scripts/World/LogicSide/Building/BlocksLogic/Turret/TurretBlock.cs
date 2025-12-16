using UnityEngine;

[CreateAssetMenu(fileName = "TurretBlock", menuName = "FACTORY/Block/TurretBlock")]
public class TurretBlock : Block<TurretLogic>
{
    [Header("Turret")] 
    public float turretRange;

    [Header("Projectile")] 
    public int projectileRateOnTicks = 20;
    public float projectileSpeed = 10;
    public float projectileLifetime = 1;
    public float projectileCollisionRadius = .25f;
    public int projectileDamage = 1;
    public int projectilePenetration = 1;
}
