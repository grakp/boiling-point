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

    readonly List<CustomerOrder> orderQueue = new List<CustomerOrder>();
    float nextSpawnTime;

    public IReadOnlyList<CustomerOrder> OrderQueue => orderQueue;

    void Start()
    {
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
                orderQueue.RemoveAt(i);
        }
    }

    void SpawnOrder()
    {
        if (availableRecipes == null || availableRecipes.Length == 0) return;
        var recipe = availableRecipes[UnityEngine.Random.Range(0, availableRecipes.Length)];
        if (recipe == null) return;
        orderQueue.Add(new CustomerOrder(recipe, patienceSeconds));
        Debug.Log($"[OrderQueue] New order: {recipe.recipeName} (patience {patienceSeconds}s, queue size {orderQueue.Count}).");
    }

    void OnServeStationWorkCompleted(Station station, StationWorkRequest request)
    {
        if (request?.Step?.outputs == null || request.Step.outputs.Length == 0) return;
        ItemType servedType = request.Step.outputs[0].type;

        if (orderQueue.Count == 0)
        {
            Debug.Log("[OrderQueue] Wrong order: no order in queue.");
            return;
        }

        var first = orderQueue[0];
        if (first.Recipe.GetServedItemType() == servedType)
        {
            orderQueue.RemoveAt(0);
            Debug.Log($"[OrderQueue] Order matched: {first.Recipe.recipeName}.");
        }
        else
        {
            Debug.Log("[OrderQueue] Wrong order: served item does not match first order.");
        }
    }
}
