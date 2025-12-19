using UnityEngine;

public class BaseLogic : BuildingLogic, IItemAcceptor
{

    public bool CanAccept(Item item)
    {
        if(item.canBeInsertedOnBase == false) return false;
        
        return true;
    }

    public bool Insert(Item item)
    {
        if (!CanAccept(item))
            return false;

        return GameManager.Instance.AddItemToInventory(item, 1);
    }

}
