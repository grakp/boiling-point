using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Restaurant/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] string recipeName;
    [SerializeField] int baseCost = 10;
    [SerializeField] Sprite displayIcon;
    [SerializeField] TaskStep[] steps;

    public string RecipeName => recipeName;
    public int BaseCost => baseCost;
    public Sprite DisplayIcon => displayIcon;
    public TaskStep[] Steps => steps;

    public ItemType GetServedItemType()
    {
        if (steps == null || steps.Length == 0) return ItemType.None;
        var last = steps[steps.Length - 1];
        if (last.Outputs == null || last.Outputs.Length == 0) return ItemType.None;
        return last.Outputs[0].type;
    }
}
