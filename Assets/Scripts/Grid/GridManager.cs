using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Tilemap (optional)")]
    [SerializeField] Tilemap floorTilemap;
    [SerializeField] Tilemap obstacleTilemap;

    [Header("Fallback when no Tilemap")]
    [SerializeField] float cellSize = 1f;
    [SerializeField] Vector2 origin = Vector2.zero;
    [SerializeField] int gridMinX = -50;
    [SerializeField] int gridMaxX = 50;
    [SerializeField] int gridMinY = -50;
    [SerializeField] int gridMaxY = 50;

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

    public Vector2Int WorldToCell(Vector2 world)
    {
        if (floorTilemap != null)
        {
            var c = floorTilemap.WorldToCell(new Vector3(world.x, world.y, 0));
            return new Vector2Int(c.x, c.y);
        }
        float x = (world.x - origin.x) / cellSize;
        float y = (world.y - origin.y) / cellSize;
        return new Vector2Int(Mathf.FloorToInt(x), Mathf.FloorToInt(y));
    }

    public Vector2 CellToWorld(Vector2Int cell)
    {
        if (floorTilemap != null)
        {
            var w = floorTilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
            return new Vector2(w.x, w.y);
        }
        return origin + new Vector2((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize);
    }

    public float CellSize
    {
        get
        {
            if (floorTilemap == null) return cellSize;
            var grid = floorTilemap.layoutGrid;
            if (grid != null && grid.cellSize.x > 0.001f) return grid.cellSize.x;
            return cellSize;
        }
    }

    public BoundsInt? TilemapBounds => floorTilemap != null ? floorTilemap.cellBounds : null;

    public void GetGridBounds(out int minX, out int minY, out int maxX, out int maxY)
    {
        if (floorTilemap != null)
        {
            var b = floorTilemap.cellBounds;
            if (b.xMax > b.xMin && b.yMax > b.yMin)
            {
                minX = b.xMin;
                minY = b.yMin;
                maxX = b.xMax - 1;
                maxY = b.yMax - 1;
                return;
            }
        }
        minX = gridMinX;
        minY = gridMinY;
        maxX = gridMaxX;
        maxY = gridMaxY;
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

    public bool IsCellOccupied(Vector2Int cell, Employee exclude)
    {
        foreach (var kv in employeeCells)
            if (kv.Value == cell && kv.Key != exclude)
                return true;
        return false;
    }

    public bool IsInBounds(Vector2Int cell)
    {
        if (floorTilemap != null)
            return floorTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0));
        return cell.x >= gridMinX && cell.x <= gridMaxX && cell.y >= gridMinY && cell.y <= gridMaxY;
    }

    bool IsObstacleTileAt(Vector2Int cell)
    {
        if (obstacleTilemap == null) return false;
        return obstacleTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0));
    }

    public bool IsWalkable(Vector2Int cell, Vector2Int goalCell, Employee excludeEmployee)
    {
        if (cell == goalCell) return true;
        if (!IsInBounds(cell)) return false;
        if (blockedCells.Contains(cell)) return false;
        if (IsObstacleTileAt(cell)) return false;
        return !IsCellOccupied(cell, excludeEmployee);
    }

    public Vector2Int? GetNearestWalkableCell(Vector2Int cell, int maxRadius = 25)
    {
        if (floorTilemap == null) return IsInBounds(cell) ? cell : (Vector2Int?)null;
        if (floorTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0))) return cell;
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
