using UnityEngine;

public class ConveyorLogic : BuildingLogic
{
    private int conveyorTickSpeed;
    private Item[] itemBuffer;
    private int[] itemProgress;  // Progreso individual de cada item (0 a conveyorTickSpeed)
    
    const int slotCount = 4; // Mindustry usa 4 slots

    public override void Initialize(Block block)
    {
        var conveyor = (ConveyorBlock)block;
        conveyorTickSpeed = conveyor.ticksToMoveConveyorItem;
        itemBuffer = new Item[slotCount];
        itemProgress = new int[slotCount];
    }

    public override void Tick()
    {
        DrawDebug();
        
        // Cada item avanza independientemente
        for (int i = 0; i < slotCount; i++)
        {
            if (itemBuffer[i] != null)
            {
                itemProgress[i]++;
            }
        }
        
        MoveItems();
    }

    public override void OnPlaced()
    {
        itemBuffer[0] = new Item();
        itemProgress[0] = 0;
    }

    private void DrawDebug()
    {
        Vector3 basePos = new Vector3(building.position.x + 0.5f, building.position.y + 0.5f, 0); 
        float offset = 1f / slotCount;
        
        for (int i = 0; i < slotCount; i++)
        {
            // Mostrar progreso con color gradual
            float progressRatio = itemBuffer[i] != null ? (float)itemProgress[i] / conveyorTickSpeed : 0;
            Color c = itemBuffer[i] == null ? Color.red : Color.green;
            
            for (int size = -1; size <= 1; size++)
            {
                Vector3 start = basePos + new Vector3(size * 0.05f, i * offset, 0); 
                Vector3 end = start + Vector3.up * 0.1f;
                Debug.DrawLine(start, end, c, 0.01f, false);
            }
        }
    }

    private void MoveItems()
    {
        // CLAVE: Procesar de atrás hacia adelante para evitar procesar el mismo item dos veces
        for (int i = slotCount - 1; i >= 0; i--)
        {
            if (itemBuffer[i] == null) continue;
            
            // Solo mover si ha completado su progreso
            if (itemProgress[i] < conveyorTickSpeed) continue;
            
            bool moved = false;
            
            // PRIORIDAD 1: Intentar salir al siguiente building (solo desde el último slot)
            if (i == slotCount - 1)
            {
                moved = TryOutputToNextBuilding(i);
            }
            
            // PRIORIDAD 2: Si no salió (o no está en el último slot), mover al siguiente slot interno
            if (!moved && i < slotCount - 1)
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

        // Obtener tile adelante según dirección (aquí asumo dirección UP)
        Tile fwdTile = World.Instance.GetTile(building.position.x, building.position.y + 1);
        if (fwdTile == null || fwdTile.building == null) return false;

        var logic = fwdTile.building.logic;

        // Intentar insertar en el siguiente building
        if (logic is ConveyorLogic conveyorLogic)
        {
            // IMPORTANTE: Solo insertar si el otro conveyor puede aceptar (primer slot libre)
            if (conveyorLogic.CanAcceptItem() && conveyorLogic.TryInsert(item))
            {
                itemBuffer[index] = null;
                return true;
            }
        }
        else if (logic is StorageLogic storageLogic)
        {
            if (storageLogic.inventory.Add(item, 1))
            {
                itemBuffer[index] = null;
                return true;
            }
        }

        return false;
    }

    public bool CanAcceptItem()
    {
        return itemBuffer[0] == null;
    }

    public bool TryInsert(Item item)
    {
        // Solo insertar en el primer slot si está vacío
        if (itemBuffer[0] == null)
        {
            itemBuffer[0] = item;
            itemProgress[0] = 0;
            return true;
        }
        return false;
    }
}