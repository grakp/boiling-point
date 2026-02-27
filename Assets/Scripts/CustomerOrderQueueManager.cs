using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomerOrderQueueManager : MonoBehaviour
{
    [SerializeField] Recipe[] availableRecipes;
    [SerializeField] Station serveStation;
    [SerializeField] float spawnIntervalMin = 20f;
    [SerializeField] float spawnIntervalMax = 45f;
    [SerializeField] float patienceSeconds = 60f;
    [SerializeField] int startingMoney = 0;
    [SerializeField] int wrongOrderPenalty = 5;

    readonly List<CustomerOrder> orderQueue = new List<CustomerOrder>();
    float nextSpawnTime;
    int money;

    public IReadOnlyList<CustomerOrder> OrderQueue => orderQueue;
    public int Money => money;
    public event Action<int> OnMoneyEarned;

    void Start()
    {
        money = startingMoney;
        nextSpawnTime = Time.time;
        if (serveStation != null && serveStation.Type == StationType.Serve)
            serveStation.OnWorkCompleted += OnServeStationWorkCompleted;
    }

    void OnDestroy()
    {
        if (serveStation != null)
            serveStation.OnWorkCompleted -= OnServeStationWorkCompleted;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnOrder();
            nextSpawnTime = Time.time + UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
        }

        for (int i = orderQueue.Count - 1; i >= 0; i--)
        {
            orderQueue[i].TimeRemaining -= Time.deltaTime;
            if (orderQueue[i].IsExpired)
            {
                ApplyWrongOrderPenalty();
                orderQueue.RemoveAt(i);
            }
        }
    }

    void SpawnOrder()
    {
        if (availableRecipes == null || availableRecipes.Length == 0) return;
        var recipe = availableRecipes[UnityEngine.Random.Range(0, availableRecipes.Length)];
        if (recipe == null) return;
        orderQueue.Add(new CustomerOrder(recipe, patienceSeconds));
        Debug.Log($"[OrderQueue] New order: {recipe.RecipeName} (patience {patienceSeconds}s, queue size {orderQueue.Count}).");
    }

    void ApplyWrongOrderPenalty()
    {
        money -= wrongOrderPenalty;
        OnMoneyEarned?.Invoke(-wrongOrderPenalty);
    }

    void OnServeStationWorkCompleted(Station station, StationWorkRequest request)
    {
        if (request?.Step?.Outputs == null || request.Step.Outputs.Length == 0) return;
        ItemType servedType = request.Step.Outputs[0].type;

        if (orderQueue.Count == 0)
        {
            ApplyWrongOrderPenalty();
            Debug.Log("[OrderQueue] Wrong order: no order in queue.");
            return;
        }

        var first = orderQueue[0];
        if (first.Recipe.GetServedItemType() == servedType)
        {
            float fullPayoutThreshold = first.PatienceSeconds * 0.75f;
            float multiplier = first.TimeRemaining >= fullPayoutThreshold ? 1f : Mathf.Clamp01(first.TimeRemaining / fullPayoutThreshold);
            int payout = Mathf.Max(0, Mathf.RoundToInt(first.Recipe.BaseCost * multiplier));
            money += payout;
            orderQueue.RemoveAt(0);
            OnMoneyEarned?.Invoke(payout);
            Debug.Log($"[OrderQueue] Order matched: {first.Recipe.RecipeName}, earned {payout}.");
        }
        else
        {
            ApplyWrongOrderPenalty();
            Debug.Log("[OrderQueue] Wrong order: served item does not match first order.");
        }
    }
}
