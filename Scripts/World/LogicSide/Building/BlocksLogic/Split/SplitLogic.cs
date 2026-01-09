using System.Collections.Generic;
using UnityEngine;

public class SplitLogic : LogisticBuilding
{
    public int conveyorTickSpeed;
    public Item[] itemBuffer;
    public int[] itemProgress;

    public static int SLOT_COUNT = 1;

    // 🔁 Round-robin: recuerda la próxima salida
    private int nextOutputIndex = 0;
    
    public override void Initialize(Block block)
    {
        var split = (SplitBlock)block;
        conveyorTickSpeed = split.ticksToMoveConveyorItem;
        itemBuffer = new Item[SLOT_COUNT];
        itemProgress = new int[SLOT_COUNT];
    }

    public override void Tick()
    {
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (itemBuffer[i] != null)
                itemProgress[i]++;
        }

        MoveItems();
        TryCollectFromNearbyProviders();
        PullFromInventoryBehind();
    }

    private void MoveItems()
    {
        for (int i = SLOT_COUNT - 1; i >= 0; i--)
        {
            if (itemBuffer[i] == null) continue;
            if (itemProgress[i] < conveyorTickSpeed) continue;

            bool moved = false;

            // 👉 Solo el último slot puede sacar items
            if (i == SLOT_COUNT - 1)
            {
                moved = TryOutputRoundRobin(i);
            }

            // 👉 Movimiento interno
            if (!moved && i < SLOT_COUNT - 1)
            {
                moved = TryMoveToNextSlot(i);
            }

            if (moved)
                itemProgress[i] = 0;
        }
    }

    private bool TryMoveToNextSlot(int fromIndex)
    {
        int toIndex = fromIndex + 1;

        if (itemBuffer[toIndex] == null)
        {
            itemBuffer[toIndex] = itemBuffer[fromIndex];
            itemProgress[toIndex] = 0;
            itemBuffer[fromIndex] = null;
            return true;
        }

        return false;
    }

    // ==============================
    // 🔁 SPLITTER ROUND ROBIN
    // ==============================
    private bool TryOutputRoundRobin(int index)
    {
        Item item = itemBuffer[index];
        if (item == null) return false;

        // Intenta las 3 salidas empezando por nextOutputIndex
        for (int i = 0; i < 3; i++)
        {
            int outIndex = (nextOutputIndex + i) % 3;
            Vector2Int dir = GetOutputDir(outIndex);
            Vector2Int pos = building.position + dir;

            Tile outputTile = World.Instance.GetTile(pos.x, pos.y);
            if (outputTile == null || outputTile.building == null) continue;

            var logic = outputTile.building.logic;

            if (logic is IItemAcceptor acceptor)
            {
                if (World.IsBuildFacingPosition(outputTile.building, building.position))
                    continue;
                
                if (acceptor.CanAccept(item) && acceptor.Insert(item))
                {
                    itemBuffer[index] = null;

                    // 👉 avanzar el turno solo si salió
                    nextOutputIndex = (outIndex + 1) % 3;
                    return true;
                }
            }
        }

        return false;
    }
    

    // ==============================
    // 📐 DIRECCIONES DE SALIDA
    // 0 = forward, 1 = right, 2 = left
    // ==============================
    private Vector2Int GetOutputDir(int index)
    {
        Vector2Int fwd = ForwardFromRotation(building.rotation);
        Vector2Int right = new Vector2Int(-fwd.y, fwd.x);
        Vector2Int left = -right;

        switch (index)
        {
            case 0: return fwd;
            case 1: return right;
            case 2: return left;
            default: return fwd;
        }
    }

    // ==============================
    // 🔄 INPUT
    // ==============================
    void TryCollectFromNearbyProviders()
    {
        if (itemBuffer[0] != null)
            return;

        Vector2Int fwd = ForwardFromRotation(building.rotation);
        Vector2Int right = new Vector2Int(-fwd.y, fwd.x);
        Vector2Int left = -right;

        Vector2Int[] dirs = { right, left };

        foreach (var dir in dirs)
        {
            Vector2Int pos = building.position + dir;
            Building b = World.Instance.GetBuilding(pos);

            if (b == null) continue;

            if (b.logic is IItemProvider provider)
            {
                if (itemBuffer[0] != null)
                    return;

                Item extracted = provider.PeekFirst();
                if (extracted != null)
                {
                    itemBuffer[0] = extracted;
                    itemProgress[0] = conveyorTickSpeed - 1;
                    return;
                }

            }
        }
    }

    private void PullFromInventoryBehind()
    {
        if (itemBuffer[0] != null) return;

        Vector2Int back = BackFromRotation(building.rotation);
        Tile backTile = World.Instance.GetTile(building.position.x + back.x, building.position.y + back.y);

        if (backTile == null || backTile.building == null) return;

        if (backTile.building.logic is IItemProvider provider)
        {
            Item extracted = provider.PeekFirst();
            if (extracted != null)
            {
                itemBuffer[0] = extracted;
                itemProgress[0] = conveyorTickSpeed - 1;
            }

        }
    }

    // ==============================
    // 📦 INTERFACES
    // ==============================
    public override bool CanAccept(Item item)
    {
        return itemBuffer[0] == null;
    }

    public override bool Insert(Item item)
    {
        if (!CanAccept(item)) return false;

        itemBuffer[0] = item;
        itemProgress[0] = conveyorTickSpeed - 1;
        return true;
    }

    // ==============================
    // 🧭 HELPERS
    // ==============================
    public float GetItemProgressNormalized(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SLOT_COUNT) return 0f;
        if (itemBuffer[slotIndex] == null) return 0f;
        return (float)itemProgress[slotIndex] / conveyorTickSpeed;
    }

    public Vector2Int BackFromRotation(int rotation)
    {
        switch (rotation)
        {
            case 0: return Vector2Int.down;
            case 1: return Vector2Int.left;
            case 2: return Vector2Int.up;
            case 3: return Vector2Int.right;
            default: return Vector2Int.down;
        }
    }

    public Vector2Int ForwardFromRotation(int rotation = 0)
    {
        switch (rotation)
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.right;
            case 2: return Vector2Int.down;
            case 3: return Vector2Int.left;
            default: return Vector2Int.up;
        }
    }
}
