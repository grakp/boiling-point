using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FloorMovementTarget : MonoBehaviour, IMovementTarget
{
    [SerializeField] GameObject highlightOverlay;

    Vector2Int? cell;

    public void SetCell(Vector2Int c)
    {
        cell = c;
    }

    public Vector2 GetTargetPosition(Vector2? hitPoint = null)
    {
        if (cell.HasValue && GridManager.Instance != null)
            return GridManager.Instance.CellToWorld(cell.Value);
        return hitPoint ?? (Vector2)transform.position;
    }

    public void SetHoverHighlight(bool on, Color colour)
    {
        GameObject overlay = highlightOverlay != null ? highlightOverlay : FindOverlayChild();
        if (overlay == null) return;
        overlay.SetActive(on);
        if (on)
        {
            var sr = overlay.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) HoverHighlightHelper.ApplyColourOnly(sr, colour);
        }
    }

    GameObject FindOverlayChild()
    {
        var t = transform.Find("HighlightOverlay");
        return t != null ? t.gameObject : null;
    }
}
