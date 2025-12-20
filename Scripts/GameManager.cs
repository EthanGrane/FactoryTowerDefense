using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Inventory playerInventory = new Inventory(99,9999);
    
    public Action onPlayerInventoryChanged;
    public Action onBaseHealthChanged;
    public Action onBaseDestroyed;

    public int baseHealth = 100;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool AddItemToInventory(Item item, int amount)
    {
        bool m = playerInventory.Add(item, amount);
        onPlayerInventoryChanged?.Invoke();
        return m;
    }

    public Item RemoveItem(Item item, int amount)
    {
        Item _item = playerInventory.Remove(item, amount);
        onPlayerInventoryChanged?.Invoke();
        return _item;
    }

    [ContextMenu("TAKE DAMAGE")]
    public void DEBUGTakeDamage()
    {
        TakeDamage(999);
    }
    
    public void TakeDamage(int dmg)
    {
        baseHealth -= dmg;
        onBaseHealthChanged?.Invoke();

        if(baseHealth <= 0)
            onBaseDestroyed?.Invoke();
    }

    void ResetLevel()
    {
        baseHealth = 100;
        playerInventory.Clear();
    }
    
    public Inventory GetPlayerInventory()
        => playerInventory;
}