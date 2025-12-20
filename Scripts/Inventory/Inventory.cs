using UnityEngine;

public class Inventory
{
    public InventorySlot[] slots;

    public Inventory(int slotCount, int capacity = 100)
    {
        slots = new InventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            slots[i] = new InventorySlot();
            slots[i].capacity = capacity;
        }
    }


    // Agregar items al inventario
    public bool Add(Item item, int amount)
    {
        foreach (var slot in slots)
        {
            // Si el slot está vacío o tiene el mismo item
            if ((slot.item == null || slot.item == item) && !slot.IsFull)
            {
                // Si está vacío, asigna el item
                if (slot.item == null)
                    slot.item = item;

                int space = slot.capacity - slot.amount;
                int toAdd = Mathf.Min(space, amount);

                slot.amount += toAdd;
                amount -= toAdd;

                if (amount <= 0)
                    return true;
            }
        }

        return amount <= 0;
    }


    // Sacar items del inventario
    public Item Remove(Item item, int amount)
    {
        // Lógica original
        foreach(var slot in slots)
        {
            if(slot.item == item && slot.amount > 0)
            {
                int toRemove = Mathf.Min(slot.amount, amount);
                slot.amount -= toRemove;
                amount -= toRemove;
                if(amount <= 0) return slot.item;
            }
        }
        
        return null;
    }
    
    // En Inventory.cs - nuevo método
    public Item ExtractFirst()
    {
        foreach(var slot in slots)
        {
            if(slot.item != null && slot.amount > 0)
            {
                Item extracted = slot.item;
                slot.amount--;
                if(slot.amount <= 0)
                {
                    slot.item = null;
                }
                return extracted;
            }
        }
        return null;
    }

    public bool Contains(Item item, int amount)
    {
        foreach(var slot in slots)
        {
            if(slot.item == item && slot.amount > 0)
            {
                if(slot.item == item && slot.amount > amount)
                    return true;
            }
        }    
        return false;
    }

    public void Clear()
    {
        slots = null;
    }
}