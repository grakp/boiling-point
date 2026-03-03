using UnityEngine;
using UnityEngine.Serialization;

public class Employee : MonoBehaviour
{
    [FormerlySerializedAs("SpeedMultiplier")] [SerializeField] float speedMultiplier = 1f;

    public float SpeedMultiplier => speedMultiplier;

    public void SetSelected(bool selected) { }

    public void SetHoverHighlight(bool on) { }
}
