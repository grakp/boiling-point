using System;
using UnityEngine;

public class Order
{
    readonly Recipe recipe;
    readonly Func<StationType, Station> getStation;
    int currentStepIndex;

    public Order(Recipe recipe, Func<StationType, Station> getStation)
    {
        this.recipe = recipe;
        this.getStation = getStation;
    }

    public event Action<Order> OnComplete;

    public void Begin()
    {
        currentStepIndex = 0;
        EnqueueCurrentStepWork();
    }

    void EnqueueCurrentStepWork()
    {
        if (currentStepIndex >= recipe.Steps.Length)
        {
            Debug.Log($"[Order] All steps done for {recipe.RecipeName}");
            OnComplete?.Invoke(this);
            return;
        }

        var action = recipe.Steps[currentStepIndex];
        if (action == null) return;
        TaskStep step = TaskStep.FromAction(action);
        Station station = getStation(step.RequiredStation);
        if (station == null)
        {
            Debug.LogWarning($"[Order] No station found for {step.RequiredStation}, step {currentStepIndex + 1}");
            return;
        }

        Debug.Log($"[Order] Enqueue step {currentStepIndex + 1}/{recipe.Steps.Length}: {step.RequiredStation} ({step.Duration}s) -> {station.name}");
        var request = new StationWorkRequest(this, currentStepIndex, step);
        station.EnqueueWork(request);
    }

    public void HandleStationWorkCompleted(StationWorkRequest request)
    {
        if (request == null || request.Order != this) return;
        if (request.StepIndex != currentStepIndex)
        {
            // Out-of-order completion; move forward if this step is ahead.
            if (request.StepIndex < currentStepIndex) return;
        }

        currentStepIndex = request.StepIndex + 1;
        EnqueueCurrentStepWork();
    }
}
