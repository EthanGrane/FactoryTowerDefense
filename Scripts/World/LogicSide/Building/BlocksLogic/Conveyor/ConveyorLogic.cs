using UnityEngine;

public class ConveyorLogic : LogisticBuilding
{
    public int conveyorTickSpeed;
    public  Item[] itemBuffer;
    public int[] itemProgress;  // Progreso individual de cada item (0 a conveyorTickSpeed)
    
    public static int SLOT_COUNT = 3;

    public override void Initialize(Block block)
    {
        var conveyor = (ConveyorBlock)block;
        conveyorTickSpeed = conveyor.ticksToMoveConveyorItem;
        itemBuffer = new Item[SLOT_COUNT];
        itemProgress = new int[SLOT_COUNT];
    }

    public override void Tick()
    {
        // Cada item avanza independientemente
        for (int i = 0; i < SLOT_COUNT; i++)
        {
            if (itemBuffer[i] != null)
            {
                itemProgress[i]++;
            }
        }
        
        MoveItems();
        TryCollectFromNearbyProviders();
        PullFromInventoryBehind();
    }
    
    private void MoveItems()
    {
        // CLAVE: Procesar de atrás hacia adelante para evitar procesar el mismo item dos veces
        for (int i = SLOT_COUNT - 1; i >= 0; i--)
        {
            if (itemBuffer[i] == null) continue;
            
            // Solo mover si ha completado su progreso
            if (itemProgress[i] < conveyorTickSpeed) continue;
            
            bool moved = false;
            
            // PRIORIDAD 1: Intentar salir al siguiente building (solo desde el último slot)
            if (i == SLOT_COUNT - 1)
            {
                moved = TryOutputToNextBuilding(i);
            }
            
            // PRIORIDAD 2: Si no salió (o no está en el último slot), mover al siguiente slot interno
            if (!moved && i < SLOT_COUNT - 1)
            {
                moved = TryMoveToNextSlot(i);
            }
            
            // Si se movió, resetear progreso del slot origen
            if (moved)
            {
                itemProgress[i] = 0;
            }
        }
    }

    private bool TryMoveToNextSlot(int fromIndex)
    {
        int toIndex = fromIndex + 1;
        
        // Solo mover si el siguiente slot está vacío O su item ya se movió este tick
        if (itemBuffer[toIndex] == null)
        {
            itemBuffer[toIndex] = itemBuffer[fromIndex];
            itemProgress[toIndex] = 0;
            itemBuffer[fromIndex] = null;
            return true;
        }
        
        return false;
    }

    private bool TryOutputToNextBuilding(int index)
    {
        Item item = itemBuffer[index];
        if (item == null) return false;

        // Get Forward tile
        Vector2Int fwd = ForwardFromRotation(building.rotation);
        Tile fwdTile = World.Instance.GetTile(building.position.x + fwd.x, building.position.y + fwd.y);
        
        if (fwdTile == null || fwdTile.building == null) return false;

        var logic = fwdTile.building.logic;
        
        if (logic is IItemAcceptor acceptor)
        {
            if (logic is ConveyorLogic)
            {
                // Si el proximo ItemAceptor es un conveyor y esta mirando a este conveyor, entonces no se movera
                if ((logic.building.rotation + 2) % 4 == building.rotation) return false;

                // Si el proximo ItemAceptor es un Conveyor el cual esta mirando a los laterales respecto al conveyor
                // insertara los items a medio camino
                ConveyorLogic conveyorLogic = logic as ConveyorLogic;
                if (logic.building.rotation != building.rotation)
                    if(conveyorLogic.TryToInsertFromSide(item))
                    {
                        itemBuffer[index] = null;
                        return true;
                    }   
                    else
                        return false;
            }            
            
            if (acceptor.CanAccept(item) && acceptor.Insert(item))
            {
                itemBuffer[index] = null;
                return true;
            }
        }

        return false;
    }
    
    void TryCollectFromNearbyProviders()
    {
        // No recoger si el primer slot está lleno
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
                // IMPORTANTE: solo extraer si hay hueco
                if (itemBuffer[0] != null)
                    return;

                Item extracted = provider.PeekFirst();
                if (extracted != null)
                {
                    itemBuffer[0] = extracted;
                    itemProgress[0] = 0;
                    return;
                }
            }
        }
    }

    private void InsertItemOnBelt(Item item)
    {
        itemBuffer[0] = item;
        itemProgress[0] = 0;
    }

    private void PullFromInventoryBehind()
    {
        // Solo intentar si el primer slot está vacío
        if (itemBuffer[0] != null) return;
    
        Vector2Int back = BackFromRotation(building.rotation);
        Tile backTile = World.Instance.GetTile(building.position.x + back.x, building.position.y + back.y);
    
        if (backTile == null || backTile.building == null) return;

        var logic = backTile.building.logic;
    
        // Extraer de StorageLogic
        if (logic is IItemProvider itemProvider)
        {
            Item extractedItem = itemProvider.PeekFirst();
            if (extractedItem != null)
            {
                itemBuffer[0] = extractedItem;
                itemProgress[0] = 0;
            }
        }
    }
    
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
            case 0:
                return Vector2Int.up;
            case 1:
                return Vector2Int.right;
            case 2:
                return Vector2Int.down;
            case 3:
                return Vector2Int.left;
            default:
                return Vector2Int.up;
        }
    }
    
    public override bool CanAccept(Item item)
    {
        // Solo puede aceptar si hay espacio en el primer slot
        return itemBuffer[0] == null;

    }

    public override bool Insert(Item item)
    {
        if (CanAccept(item))
        {
            itemBuffer[0] = item;
            itemProgress[0] = 0; // reinicia el progreso
            return true;
        }
        return false;
    }

    public bool TryToInsertFromSide(Item item)
    {
        if (CanAccept(item))
        {
            int index = 1;
            if (itemBuffer[index] == null)
            {
                itemBuffer[index] = item;
                itemProgress[index] = 15;
                return true;
            }
        }
        
        return false;
    }
}