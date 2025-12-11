using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyTier currentTier;
    public float moveSpeed = 2f;
    private bool isAlive = true;

    private void Start()
    {
        EnemyManager.Instance.ApplyTier(this, currentTier);
    }

    private void Update()
    {
        if (!isAlive) return;
        
        // direction is pathFinsing route
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        
        // Collide with solidblocks? 
    }

    [ContextMenu("DEBUG_TakeDamage")]
    public void DEBUG_TakeDamage()
    {
        TakeDamage(1);
    }
    
    public void TakeDamage(int dmg)
    {
        if (!isAlive) return;
        EnemyManager.Instance.ProcessDamage(this, dmg);
    }

    // Solo llamado por el manager
    public void DieExtern()
    {
        if (!isAlive) return;
        isAlive = false;
        Destroy(gameObject);
    }
}