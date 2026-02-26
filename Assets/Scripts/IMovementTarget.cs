using UnityEngine;

public interface IMovementTarget
{
    Vector2 GetTargetPosition(Vector2? hitPoint = null);
    void SetHoverHighlight(bool on);
}
