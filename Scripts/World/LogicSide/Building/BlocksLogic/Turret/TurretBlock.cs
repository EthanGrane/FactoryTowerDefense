using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TurretBlock", menuName = "FACTORY/Block/TurretBlock")]
public class TurretBlock : Block<TurretLogic>
{
    [Header("Turret")]
    public float turretRange;
    public int projectileRateOnTicks = 20;
    public int maxAmmo = 20;

    [FormerlySerializedAs("AvaliableProjectiles")] [Header("Projectile")]
    public List<ProjectileSO> avaliableProjectiles;
}