using UnityEngine;

public class StorageLogic : BuildingLogic, IItemAcceptor, IItemProvider
{
    public Inventory inventory;

    public override void Initialize(Block block)
    {
        var storageBlock = (StorageBlock)block;
        inventory = new Inventory(storageBlock.slotCount);
    }

    public bool CanAccept(Item item)
    {
        return inventory.Add(item, 0);
    }

    public bool Insert(Item item)
    {
        return inventory.Add(item, 1);
    }
    
    public Item Extract(Item item) => inventory.Remove(item, 1);

    public Item ExtractFirst() => inventory.ExtractFirst();
}
