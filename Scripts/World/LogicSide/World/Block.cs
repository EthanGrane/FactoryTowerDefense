using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewBlock", menuName = "FACTORY/Block")]
public class Block : ScriptableObject
{
    [Header("Settings")]
    public int size = 1;
    
    [Header("Visuals")]
    public string blockName;
    public TileBase blockTile;
    public Sprite sprite;
    
    [Header("Health")]
    public float blockHealth = 100;

    [Header("Building")] 
    public BuildingCost[] buildingCost;
    
    [Header("Flags")]
    public bool solid;
    public bool CanBeRemovedByPlayer = true;
    
    public System.Type logicType;
}

public abstract class Block<T> : Block where T : new()
{
    private void OnEnable()
    {
        logicType = typeof(T);
    }
}

[System.Serializable]
public struct BuildingCost
{
    [FormerlySerializedAs("buildingCost")] public Item requieredItem;
    public int amount;
}