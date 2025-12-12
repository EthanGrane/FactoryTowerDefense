using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewBlock", menuName = "FACTORY/Block")]
public class Block : ScriptableObject
{
    public string blockName;
    public int size = 1;
    public bool solid;
    public TileBase blockTile;
    public Sprite sprite;
    [Space]
    public float blockHealth = 100;
    
    public System.Type logicType;
}

public abstract class Block<T> : Block where T : new()
{
    private void OnEnable()
    {
        logicType = typeof(T);
    }
}