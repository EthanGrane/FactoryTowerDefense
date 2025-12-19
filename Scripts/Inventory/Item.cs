using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "FACTORY/Item")]
public class Item : ScriptableObject
{
    public string name;
    public Sprite icon;

    [Header("Ammo")]
    public bool isAmmo;
    public ProjectileSO projectile;
    
    [Header("Base")]
    public bool canBeInsertedOnBase = true;
    
}
