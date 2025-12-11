using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed;
    public float remainingRange;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePos - (Vector2)transform.position;
            direction = direction.normalized;
            Projectile projectile = new Projectile(transform.position, direction, speed, .25f, remainingRange);
            ProjectileManager.instance.RegisterProjectile(projectile);
        }
    }
}
