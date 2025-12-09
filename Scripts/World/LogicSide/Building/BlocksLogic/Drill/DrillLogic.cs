using UnityEngine;

public class DrillLogic : BuildingLogic, IItemProvider
{
    private Item item;

    private int itemsExtracted = 0;
    public static int SLOT_COUNT = 9;
    private float itemExtractionValue = 0;

    public override void Initialize(Block block)
    {
        var drillBlock = (DrillBlock)block;
        this.item = drillBlock.outputItem;
    }

    public override void Tick()
    {
        if (itemsExtracted < SLOT_COUNT)
            TryToDrill();
    }

    void TryToDrill()
    {
        World world = World.Instance;

        Tile tile = world.GetTile(building.position);
        DrillBlock drillBlock = building.block as DrillBlock;
        Vector2Int pos = building.position;

        for (int x = 0; x < tile.building.block.size; x++)
        {
            for (int y = 0; y < tile.building.block.size; y++)
            {
                if (World.Instance.GetTile(x + pos.x,y+ pos.y).terrainSO == drillBlock.terrainSO)
                    itemExtractionValue += drillBlock.efficiencyPerTile / LogicManager.TICKS_PER_SECOND;
            }
        }
        
        if(itemExtractionValue >= 1)
        {
            itemExtractionValue -= 1;
            itemsExtracted++;
        }
    }

    public Item Extract(Item item)
    {
        if (itemsExtracted == 0)
            return null;

        itemsExtracted--;
        return this.item;
    }

    public Item ExtractFirst()
    {
        return Extract(this.item);
    }
}
