using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Tilemap")]
    [SerializeField] Tilemap floorTilemap;
    [SerializeField] Tilemap obstacleTilemap;

    // set of cells that are blocked (obstacles or occupied by employees)
    readonly HashSet<Vector2Int> blockedCells = new();
    readonly Dictionary<Employee, Vector2Int> employeeCells = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // convert world position to cell position
    public Vector2Int WorldToCell(Vector2 world)
    {
        if (floorTilemap == null) return Vector2Int.zero;
        var c = floorTilemap.WorldToCell(new Vector3(world.x, world.y, 0));
        return new Vector2Int(c.x, c.y);
    }

    // convert cell position to world position
    public Vector2 CellToWorld(Vector2Int cell)
    {
        if (floorTilemap == null) return Vector2.zero;
        var w = floorTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
        return new Vector2(w.x, w.y);
    }

    // get cell size
    public float CellSize
    {
        get
        {
            if (floorTilemap == null) return 1f;
            var grid = floorTilemap.layoutGrid;
            if (grid != null && grid.cellSize.x > 0.001f) return grid.cellSize.x;
            return 1f;
        }
    }

    // get tilemap bounds
    public BoundsInt? TilemapBounds => floorTilemap != null ? floorTilemap.cellBounds : null;

    // get grid bounds
    public void GetGridBounds(out int minX, out int minY, out int maxX, out int maxY)
    {
        if (floorTilemap == null)
        {
            minX = minY = maxX = maxY = 0;
            return;
        }
        var b = floorTilemap.cellBounds;
        if (b.xMax > b.xMin && b.yMax > b.yMin)
        {
            minX = b.xMin;
            minY = b.yMin;
            maxX = b.xMax - 1;
            maxY = b.yMax - 1;
        }
        else
        {
            minX = minY = maxX = maxY = 0;
        }
    }

    public void RegisterBlockedCells(IEnumerable<Vector2Int> cells)
    {
        foreach (var c in cells)
            blockedCells.Add(c);
    }

    public void UnregisterBlockedCells(IEnumerable<Vector2Int> cells)
    {
        foreach (var c in cells)
            blockedCells.Remove(c);
    }

    public void SetEmployeeCell(Employee employee, Vector2Int cell)
    {
        if (employee == null) return;
        employeeCells[employee] = cell;
    }

    public Vector2Int? GetEmployeeCell(Employee employee)
    {
        return employee != null && employeeCells.TryGetValue(employee, out var cell) ? cell : null;
    }

    public void RemoveEmployee(Employee employee)
    {
        if (employee != null) employeeCells.Remove(employee);
    }

    // check if cell is occupied by an employee
    public bool IsCellOccupied(Vector2Int cell, Employee exclude)
    {
        foreach (var kv in employeeCells)
            if (kv.Value == cell && kv.Key != exclude)
                return true;
        return false;
    }

    // check if cell is in bounds
    public bool IsInBounds(Vector2Int cell)
    {
        if (floorTilemap == null) return false;
        return floorTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0));
    }

    // check if cell is an obstacle tile
    bool IsObstacleTileAt(Vector2Int cell)
    {
        if (obstacleTilemap == null) return false;
        return obstacleTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0));
    }

    // check if cell is walkable
    public bool IsWalkable(Vector2Int cell, Vector2Int goalCell, Employee excludeEmployee)
    {
        if (cell == goalCell) return true;
        if (!IsInBounds(cell)) return false;
        if (blockedCells.Contains(cell)) return false;
        if (IsObstacleTileAt(cell)) return false;
        return !IsCellOccupied(cell, excludeEmployee);
    }

    // get nearest walkable cell
    public Vector2Int? GetNearestWalkableCell(Vector2Int cell, int maxRadius = 25)
    {
        if (floorTilemap == null) return null;
        if (floorTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0))) return cell;
        // check cells in radius around given cell
        for (int r = 1; r <= maxRadius; r++)
            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                    if (Mathf.Abs(dx) == r || Mathf.Abs(dy) == r)
                    {
                        var c = new Vector2Int(cell.x + dx, cell.y + dy);
                        if (floorTilemap.HasTile(new Vector3Int(c.x, c.y, 0)))
                            return c;
                    }
        return null;
    }
}
