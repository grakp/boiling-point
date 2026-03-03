using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FloorMovementTarget : MonoBehaviour, IMovementTarget
{
    public Vector2 GetTargetPosition(Vector2? hitPoint = null)
    {
        return hitPoint ?? (Vector2)transform.position;
    }

    public void SetHoverHighlight(bool on) { }
}
