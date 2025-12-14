using System;
using UnityEditor;
using UnityEngine;

public class PlayerBasePoint : MonoBehaviour
{
    public Block playerBaseBlock;
    
    private void Start()
    {
        BuildingManager.Instance.Build(new Vector2Int((int)transform.position.x,(int)transform.position.y),playerBaseBlock);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.25f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, FindFirstObjectByType<EnemySpawnPoint>().transform.position);
    }
}
