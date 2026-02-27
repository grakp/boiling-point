using UnityEngine;
using UnityEngine.Serialization;

public class Employee : MonoBehaviour
{
    [FormerlySerializedAs("SpeedMultiplier")] [SerializeField] float speedMultiplier = 1f;
    [SerializeField] GameObject selectionIndicator;

    public float SpeedMultiplier => speedMultiplier;

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null) selectionIndicator.SetActive(selected);
    }
}
