using UnityEngine;

public class PantryInventory : MonoBehaviour
{
    [SerializeField] Inventory inventory = new Inventory();

    public int GetAmount(ItemType type) => inventory != null ? inventory.GetAmount(type) : 0;

    public void Add(ItemType type, int amount)
    {
        if (inventory != null) inventory.Add(type, amount, null);
    }

    public int Remove(ItemType type, int amount)
    {
        return inventory != null ? inventory.Remove(type, amount) : 0;
    }

    public bool HasAll(ItemStack[] requirements) => inventory != null && inventory.HasAll(requirements);
}

