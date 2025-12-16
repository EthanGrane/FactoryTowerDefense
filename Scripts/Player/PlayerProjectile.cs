using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed;
    public float remainingRange;
    public float collisionRadius = .33f;
    public int projectileDamage = 1;
    public int projectilePenetration = 1;
    
    [Space]
    public float timeBetweenShots = 0.5f;

    private float time;
    
    void Update()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
            return;
        }
        
        if (Input.GetKey(KeyCode.Space))
        {
            time = timeBetweenShots;
            
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePos - (Vector2)transform.position;
            direction = direction.normalized;
            Projectile projectile = new Projectile(transform.position, direction, speed, collisionRadius, remainingRange,projectileDamage,projectilePenetration);
            ProjectileManager.instance.RegisterProjectile(projectile);
        }
    }
}
