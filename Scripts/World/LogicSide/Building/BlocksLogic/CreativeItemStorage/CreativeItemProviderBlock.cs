using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Creative Item Provider Block", menuName = "FACTORY/Block/Creative Item Provider Block")]
public class CreativeItemProviderBlock : Block<CreativeItemProviderLogic>
{
    [Header("Creative Item Provider Block")]
    public Item item;
}