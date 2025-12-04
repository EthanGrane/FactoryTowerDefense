using UnityEngine;

public class InventorySlot
{
    public Item item;
    public int amount;
    public int capacity = 100;
    
    public bool IsFull => amount >= capacity;
}
