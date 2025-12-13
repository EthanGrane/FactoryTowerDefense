using UnityEngine;

public class BaseLogic : BuildingLogic, IItemAcceptor
{
    public bool CanAccept(Item item)
    {
        return true;
    }

    public bool Insert(Item item)
    {
        Debug.Log($"Item: {item.name} inserted");
        return true;
    }
}
