using UnityEngine;

public class Projectile
{
    public Vector2 position;
    public Vector2 direction;
    public float speed;

    public int damage = 1;
    public float collisionRadius;
    public float lifetme;

    public bool isDead;

    public Projectile(Vector2 position, Vector2 direction, float speed, float collisionRadius, float lifetme, int damage = 1)
    {
        this.position = position;
        this.direction = direction.normalized;
        this.speed = speed;
        this.collisionRadius = collisionRadius;
        this.lifetme = lifetme;
        this.damage = damage;
        isDead = false;
    }
}