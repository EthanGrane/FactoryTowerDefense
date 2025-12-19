using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Inventory playerInventory = new Inventory(99);
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    public bool AddItemToInventory(Item item, int amount)=> playerInventory.Add(item, amount);

    public Item RemoveItem(Item item, int amount)
        => playerInventory.Remove(item, amount);

    public Inventory GetPlayerInventory()
        => playerInventory;
}