using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Restaurant/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;
    public TaskStep[] steps;
}
