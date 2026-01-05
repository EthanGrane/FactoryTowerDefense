using UnityEngine;

public class LogisticBuilding : BuildingLogic, IItemAcceptor
{
    public virtual bool CanAccept(Item item)
    {
        throw new System.NotImplementedException();
    }

    public virtual bool Insert(Item item)
    {
        throw new System.NotImplementedException();
    }
}
