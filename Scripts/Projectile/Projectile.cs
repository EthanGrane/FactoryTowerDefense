using UnityEngine;

public class Projectile
{
    public Vector2 position;
    public Vector2 direction;
    public float speed;

    public int damage = 1;
    public float collisionRadius;
    public float remainingRange;

    public bool isDead;

    public Projectile(Vector2 position, Vector2 direction, float speed, float collisionRadius, float remainingRange, int damage = 1)
    {
        this.position = position;
        this.direction = direction.normalized;
        this.speed = speed;
        this.collisionRadius = collisionRadius;
        this.remainingRange = remainingRange;
        this.damage = damage;
        isDead = false;
    }
}