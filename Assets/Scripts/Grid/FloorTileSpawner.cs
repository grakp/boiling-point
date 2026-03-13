using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorTileSpawner : MonoBehaviour
{
    [SerializeField] GameObject floorTilePrefab;
    [SerializeField] Transform parent;

    void Start()
    {
        if (GridManager.Instance == null || floorTilePrefab == null) return;
        var tilemap = GridManager.Instance.FloorTilemap;
        if (tilemap == null) return;

        var bounds = tilemap.cellBounds;
        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                var cell = new Vector3Int(x, y, 0);
                if (!tilemap.HasTile(cell)) continue;

                Vector2 world = GridManager.Instance.CellToWorld(new Vector2Int(x, y));
                var go = Instantiate(floorTilePrefab, new Vector3(world.x, world.y, 0f), Quaternion.identity, parent != null ? parent : transform);
                if (go.TryGetComponent<FloorMovementTarget>(out var tile))
                    tile.SetCell(new Vector2Int(x, y));
            }
        }
    }
}
