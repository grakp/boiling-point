using UnityEngine;
using UnityEngine.Serialization;

public class Employee : MonoBehaviour, IHoverable
{
    [FormerlySerializedAs("SpeedMultiplier")] [SerializeField] float speedMultiplier = 1f;

    public float SpeedMultiplier => speedMultiplier;

    public void SetSelected(bool selected) { }

    public void SetHoverHighlight(bool on, Color colour)
    {
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) HoverHighlightHelper.ApplyHighlight(sr, on, colour);
    }
}
