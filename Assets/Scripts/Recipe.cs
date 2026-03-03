using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Restaurant/Recipe")]
public class Recipe : ScriptableObject
{
    [SerializeField] string recipeName;
    [SerializeField] int baseCost = 10;
    [SerializeField] Sprite displayIcon;
    [SerializeField] StationAction[] steps;

    public string RecipeName => recipeName;
    public int BaseCost => baseCost;
    public Sprite DisplayIcon => displayIcon;
    public StationAction[] Steps => steps;

    public ItemType GetServedItemType()
    {
        if (steps == null || steps.Length == 0) return ItemType.None;
        var last = steps[steps.Length - 1];
        if (last == null || last.Outputs == null || last.Outputs.Length == 0) return ItemType.None;
        return last.Outputs[0].type;
    }
}
