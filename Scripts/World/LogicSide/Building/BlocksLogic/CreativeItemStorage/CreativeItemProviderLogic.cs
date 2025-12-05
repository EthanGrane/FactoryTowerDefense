using UnityEngine;

public class CreativeItemProviderLogic : BuildingLogic, IItemProvider
{
    public Item item;

    public override void Initialize(Block block)
    {
        var creativeItemProviderBlock = (CreativeItemProviderBlock)block;
        this.item = creativeItemProviderBlock.item;

    }

    public Item Extract(Item item)
    {
        if(this.item == item)
            return item;
        
        return null;
    }

    public Item ExtractFirst()
    {
        return item;
    }
}