using UnityEngine;

public class PantryInventory : MonoBehaviour
{
    [SerializeField] Inventory inventory = new Inventory();

    public Inventory Inventory => inventory;

    public int GetAmount(ItemType type) => inventory.GetAmount(type);
}

