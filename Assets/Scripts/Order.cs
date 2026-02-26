using System;
using UnityEngine;

public class Order
{
    readonly Recipe recipe;
    readonly Func<StationType, Station> getStation;
    int currentStepIndex;
    Station currentStation;

    public Order(Recipe recipe, Func<StationType, Station> getStation)
    {
        this.recipe = recipe;
        this.getStation = getStation;
    }

    public event Action<Order> OnComplete;

    public void Begin()
    {
        currentStepIndex = 0;
        AssignCurrentStep();
    }

    void AssignCurrentStep()
    {
        if (currentStepIndex >= recipe.steps.Length)
        {
            Debug.Log($"[Order] All steps done for {recipe.recipeName}");
            OnComplete?.Invoke(this);
            return;
        }

        TaskStep step = recipe.steps[currentStepIndex];
        currentStation = getStation(step.requiredStation);
        if (currentStation == null)
        {
            Debug.LogWarning($"[Order] No station found for {step.requiredStation}, step {currentStepIndex + 1}");
            return;
        }

        Debug.Log($"[Order] Step {currentStepIndex + 1}/{recipe.steps.Length}: {step.requiredStation} ({step.duration}s) -> {currentStation.name}");
        currentStation.OnTaskComplete += OnStationComplete;
        currentStation.AssignTask(step);
    }

    void OnStationComplete(Station station)
    {
        if (station != currentStation) return;
        Debug.Log($"[Order] Step complete at {station.name}, advancing");
        currentStation.OnTaskComplete -= OnStationComplete;
        currentStation = null;
        currentStepIndex++;
        AssignCurrentStep();
    }
}
