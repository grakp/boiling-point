using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WalkableFloor : MonoBehaviour, IMovementTarget
{
    [SerializeField] SpriteRenderer highlightVisual;

    public Vector2 GetTargetPosition(Vector2? hitPoint = null)
    {
        return hitPoint ?? (Vector2)transform.position;
    }

    public void SetHoverHighlight(bool on)
    {
        if (highlightVisual != null) highlightVisual.enabled = on;
    }
}
