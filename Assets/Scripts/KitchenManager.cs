using System;
using System.Collections.Generic;
using UnityEngine;

public class KitchenManager : MonoBehaviour
{
    [SerializeField] Station[] stations;
    [SerializeField] Recipe[] availableRecipes;
    [SerializeField] bool startFirstRecipeOnLoad;

    readonly List<Order> activeOrders = new List<Order>();

    public event Action<Order> OnOrderComplete;

    void Start()
    {
        if (startFirstRecipeOnLoad && availableRecipes != null && availableRecipes.Length > 0)
        {
            StartOrder(availableRecipes[0]);
        }
    }

    public void StartOrder(Recipe recipe)
    {
        if (recipe == null || recipe.Steps == null || recipe.Steps.Length == 0)
        {
            return;
        }

        var order = new Order(recipe, GetStation);
        order.OnComplete += o =>
        {
            activeOrders.Remove(o);
            OnOrderComplete?.Invoke(o);
        };
        activeOrders.Add(order);
        order.Begin();
    }

    public void StartOrderByIndex(int recipeIndex)
    {
        if (availableRecipes == null || recipeIndex < 0 || recipeIndex >= availableRecipes.Length) return;
        StartOrder(availableRecipes[recipeIndex]);
    }

    Station GetStation(StationType type)
    {
        if (stations == null) return null;
        // find station by type
        foreach (var s in stations)
            if (s != null && s.Type == type)
                return s;
        return null;
    }

    internal IReadOnlyList<Order> GetActiveOrders()
    {
        return activeOrders;
    }
}
