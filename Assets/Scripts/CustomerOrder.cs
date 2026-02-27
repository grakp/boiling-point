using UnityEngine;

public class CustomerOrder
{
    public Recipe Recipe { get; }
    public float PatienceSeconds { get; }
    public float TimeRemaining { get; set; }

    public bool IsExpired => TimeRemaining <= 0f;

    public CustomerOrder(Recipe recipe, float patienceSeconds)
    {
        Recipe = recipe;
        PatienceSeconds = patienceSeconds;
        TimeRemaining = patienceSeconds;
    }
}
