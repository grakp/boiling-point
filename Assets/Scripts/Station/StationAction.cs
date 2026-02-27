using UnityEngine;

[CreateAssetMenu(fileName = "New Station Action", menuName = "Restaurant/Station Action")]
public class StationAction : ScriptableObject
{
    public string actionName;
    public Sprite icon;
    public StationType requiredStation;
    public float duration;
    public ItemStack[] inputs;
    public ItemStack[] outputs;
}

