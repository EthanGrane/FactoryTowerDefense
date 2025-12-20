using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public ProjectileSO projectile;
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
            ProjectileManager.Instance.SpawnProjectile(transform.position, direction, projectile);
        }
    }
}
