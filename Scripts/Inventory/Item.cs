using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "FACTORY/Item")]
public class Item : ScriptableObject
{
    public string name;
    public Sprite icon;
}
