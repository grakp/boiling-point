using UnityEngine;

public class Employee : MonoBehaviour
{
    public float SpeedMultiplier = 1f;
    [SerializeField] GameObject selectionIndicator;

    public void SetSelected(bool selected)
    {
        if (selectionIndicator != null) selectionIndicator.SetActive(selected);
    }
}
