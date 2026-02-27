using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,

    // Raw ingredients
    LettuceRaw,
    TomatoRaw,
    PattyRaw,
    Bun,

    // Prepped
    LettuceChopped,
    TomatoChopped,
    PattyCooked,

    // Dish states
    AssembledBurger,
    ServedBurger
}

[Serializable]
public struct ItemStack
{
    public ItemType type;
    public int amount;
}

[Serializable]
public class Inventory
{
    [SerializeField] List<ItemStack> stacks = new();

    public int GetAmount(ItemType type)
    {
        int total = 0;
        for (int i = 0; i < stacks.Count; i++)
        {
            if (stacks[i].type == type)
                total += stacks[i].amount;
        }
        return total;
    }

    public int GetTotalAmount()
    {
        int total = 0;
        for (int i = 0; i < stacks.Count; i++)
            total += stacks[i].amount;
        return total;
    }

    public bool Has(ItemType type, int amount)
    {
        if (amount <= 0) return true;
        return GetAmount(type) >= amount;
    }

    public bool HasAll(ItemStack[] requirements)
    {
        if (requirements == null) return true;
        for (int i = 0; i < requirements.Length; i++)
        {
            var r = requirements[i];
            if (!Has(r.type, r.amount))
                return false;
        }
        return true;
    }

    public int Add(ItemType type, int amount, int? maxTotalAmount = null)
    {
        if (amount <= 0) return 0;

        int currentTotal = GetTotalAmount();
        int allowedToAdd = amount;
        if (maxTotalAmount.HasValue)
        {
            int remaining = maxTotalAmount.Value - currentTotal;
            if (remaining <= 0) return amount;
            allowedToAdd = Mathf.Min(allowedToAdd, remaining);
        }

        if (allowedToAdd <= 0) return amount;

        bool found = false;
        for (int i = 0; i < stacks.Count; i++)
        {
            if (stacks[i].type == type)
            {
                var s = stacks[i];
                s.amount += allowedToAdd;
                stacks[i] = s;
                found = true;
                break;
            }
        }

        if (!found)
        {
            stacks.Add(new ItemStack { type = type, amount = allowedToAdd });
        }

        return amount - allowedToAdd;
    }

    public int Add(ItemStack stack, int? maxTotalAmount = null)
    {
        return Add(stack.type, stack.amount, maxTotalAmount);
    }

    public int Remove(ItemType type, int amount)
    {
        if (amount <= 0) return 0;

        int remainingToRemove = amount;
        for (int i = 0; i < stacks.Count && remainingToRemove > 0; i++)
        {
            if (stacks[i].type != type) continue;

            var s = stacks[i];
            int remove = Mathf.Min(s.amount, remainingToRemove);
            s.amount -= remove;
            remainingToRemove -= remove;

            if (s.amount <= 0)
            {
                stacks.RemoveAt(i);
                i--;
            }
            else
            {
                stacks[i] = s;
            }
        }

        return amount - remainingToRemove;
    }

    public int Remove(ItemStack stack)
    {
        return Remove(stack.type, stack.amount);
    }
}

