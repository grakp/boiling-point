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
            Debug.Log($"[KitchenManager] Starting order on load: {availableRecipes[0].recipeName}");
            StartOrder(availableRecipes[0]);
        }
    }

    public void StartOrder(Recipe recipe)
    {
        if (recipe == null || recipe.steps == null || recipe.steps.Length == 0)
        {
            Debug.LogWarning("[KitchenManager] StartOrder skipped: recipe null or has no steps.");
            return;
        }

        Debug.Log($"[KitchenManager] Starting order: {recipe.recipeName} ({recipe.steps.Length} steps)");
        var order = new Order(recipe, GetStation);
        order.OnComplete += o =>
        {
            activeOrders.Remove(o);
            Debug.Log($"[KitchenManager] Order complete: {recipe.recipeName}");
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
        foreach (var s in stations)
            if (s != null && s.Type == type)
                return s;
        return null;
    }
}
