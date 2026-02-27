using UnityEngine;

[CreateAssetMenu(fileName = "New Station Action", menuName = "Restaurant/Station Action")]
public class StationAction : ScriptableObject
{
    [SerializeField] string actionName;
    [SerializeField] Sprite icon;
    [SerializeField] StationType requiredStation;
    [SerializeField] float duration;
    [SerializeField] ItemStack[] inputs;
    [SerializeField] ItemStack[] outputs;

    public string ActionName => actionName;
    public Sprite Icon => icon;
    public StationType RequiredStation => requiredStation;
    public float Duration => duration;
    public ItemStack[] Inputs => inputs;
    public ItemStack[] Outputs => outputs;
}

