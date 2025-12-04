using UnityEngine;

public class ConveyorLogic : BuildingLogic, IItemAcceptor
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
        PullFromInventoryBehind();
    }

    public override void OnPlaced()
    {
        itemBuffer[0] = new Item();
        itemProgress[0] = 0;
    }

    private void DrawDebug()
    {
        return;
        Vector2Int fwd = ForwardFromRotation(building.rotation);
        
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
                Debug.DrawLine(start, end, c, 0.1f, false);
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

        // Get Forward tile
        Vector2Int fwd = ForwardFromRotation(building.rotation);
        Tile fwdTile = World.Instance.GetTile(building.position.x + fwd.x, building.position.y + fwd.y);
        
        if (fwdTile == null || fwdTile.building == null) return false;

        var logic = fwdTile.building.logic;

        if (logic is IItemAcceptor acceptor)
        {
            if (acceptor.CanAccept(item) && acceptor.Insert(item))
            {
                itemBuffer[index] = null;
                return true;
            }
        }

        return false;
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
            Item extractedItem = itemProvider.ExtractFirst();
            if (extractedItem != null)
            {
                itemBuffer[0] = extractedItem;
                itemProgress[0] = 0;
            }
        }
    }
    
    Vector2Int BackFromRotation(int rotation)
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

    Vector2Int ForwardFromRotation(int rotation = 0)
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

    public bool CanAccept(Item item)
    {
        // Solo puede aceptar si hay espacio en el primer slot
        return itemBuffer[0] == null;

    }

    public bool Insert(Item item)
    {
        if (CanAccept(item))
        {
            itemBuffer[0] = item;
            itemProgress[0] = 0; // reinicia el progreso
            return true;
        }
        return false;
    }
}