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
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
