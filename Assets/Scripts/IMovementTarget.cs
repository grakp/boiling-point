using UnityEngine;

public interface IHoverable
{
    void SetHoverHighlight(bool on, Color colour);
}

public interface IMovementTarget : IHoverable
{
    Vector2 GetTargetPosition(Vector2? hitPoint = null);
}
