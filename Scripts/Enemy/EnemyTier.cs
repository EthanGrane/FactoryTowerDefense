using UnityEngine;

[CreateAssetMenu(fileName = "New enemy tier", menuName = "FACTORY/ENEMY/Enemy Tier")]
public class EnemyTier : ScriptableObject
{
    [Header("Tier Info")]
    public string tierName = "UNUSED";
    
    [Header("Visual")]
    public Sprite sprite;
    public Color color;

    [Header("Stats")]
    public float moveSpeed = 2f;
}