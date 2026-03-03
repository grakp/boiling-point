using UnityEngine;

[System.Serializable]
public class StationBuffer : MonoBehaviour
{
    [SerializeField] int maxTotalAmount = 10;
    [SerializeField] Inventory inventory = new Inventory();

    public int MaxTotalAmount => maxTotalAmount;

    public int GetAmount(ItemType type) => inventory != null ? inventory.GetAmount(type) : 0;

    public void Add(ItemType type, int amount, int? maxTotal = null)
    {
        if (inventory == null) return;
        inventory.Add(type, amount, maxTotal ?? maxTotalAmount);
    }

    public void Remove(ItemType type, int amount)
    {
        if (inventory == null) return;
        inventory.Remove(type, amount);
    }

    public bool HasAll(ItemStack[] requirements)
    {
        if (inventory == null) return false;
        return inventory.HasAll(requirements);
    }

    public bool CanFit(ItemStack[] outputs)
    {
        if (outputs == null || outputs.Length == 0) return true;

        int totalOutputs = 0;
        for (int i = 0; i < outputs.Length; i++)
            totalOutputs += Mathf.Max(0, outputs[i].amount);

        int current = inventory != null ? inventory.GetTotalAmount() : 0;
        // check if total outputs can fit in buffer
        return current + totalOutputs <= maxTotalAmount;
    }

    public void AddItems(ItemStack[] stacks)
    {
        if (inventory == null || stacks == null) return;
        // add items to inventory
        for (int i = 0; i < stacks.Length; i++)
            inventory.Add(stacks[i], maxTotalAmount);
    }

    public void RemoveItems(ItemStack[] stacks)
    {
        if (inventory == null || stacks == null) return;
        // remove items from inventory
        for (int i = 0; i < stacks.Length; i++)
            inventory.Remove(stacks[i]);
    }
}

