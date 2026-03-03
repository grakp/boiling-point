using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GridObstacle : MonoBehaviour
{
    [SerializeField] Vector2Int footprintSize = Vector2Int.one;

    List<Vector2Int> registeredCells;

    void OnEnable()
    {
        Register();
    }

    void Start()
    {
        if (registeredCells == null) Register();
    }

    void OnDisable()
    {
        if (GridManager.Instance != null && registeredCells != null)
        {
            GridManager.Instance.UnregisterBlockedCells(registeredCells);
            registeredCells = null;
        }
    }

    void Register()
    {
        if (GridManager.Instance == null) return;
        if (registeredCells != null) return;
        var origin = GridManager.Instance.WorldToCell(transform.position);
        registeredCells = new List<Vector2Int>();
        // register all cells in footprint
        for (int x = 0; x < footprintSize.x; x++)
            for (int y = 0; y < footprintSize.y; y++)
                registeredCells.Add(origin + new Vector2Int(x, y));
        GridManager.Instance.RegisterBlockedCells(registeredCells);
    }
}
