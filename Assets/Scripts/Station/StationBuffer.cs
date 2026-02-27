using UnityEngine;

[System.Serializable]
public class StationBuffer : MonoBehaviour
{
    [SerializeField] int maxTotalAmount = 10;
    [SerializeField] Inventory inventory = new Inventory();

    public int MaxTotalAmount => maxTotalAmount;
    public Inventory Inventory => inventory;

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
        return current + totalOutputs <= maxTotalAmount;
    }

    public void AddItems(ItemStack[] stacks)
    {
        if (inventory == null || stacks == null) return;
        for (int i = 0; i < stacks.Length; i++)
            inventory.Add(stacks[i], maxTotalAmount);
    }

    public void RemoveItems(ItemStack[] stacks)
    {
        if (inventory == null || stacks == null) return;
        for (int i = 0; i < stacks.Length; i++)
            inventory.Remove(stacks[i]);
    }
}

