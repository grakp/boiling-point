using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Restaurant/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public Sprite displayIcon;
    public TaskStep[] steps;

    public ItemType GetServedItemType()
    {
        if (steps == null || steps.Length == 0) return ItemType.None;
        var last = steps[steps.Length - 1];
        if (last.outputs == null || last.outputs.Length == 0) return ItemType.None;
        return last.outputs[0].type;
    }
}
