using UnityEngine;

[CreateAssetMenu(fileName = "New enemy tier", menuName = "FACTORY/ENEMY/Enemy Tier")]
public class EnemyTierSO : ScriptableObject
{
    [Header("Tier Info")]
    public string tierName = "UNUSED";
    
    [Header("Visual")]
    public Sprite sprite;
    public Color color;

    [Header("Stats")]
    public float moveSpeed = 2f;

    [Header("Drops")] 
    public bool canDropItem = true;
    public Item dropItem;
    public int dropAmount;
    
    [HideInInspector] public int tierIndex;
}