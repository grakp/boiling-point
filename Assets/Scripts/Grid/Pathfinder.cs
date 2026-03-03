using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    static readonly Vector2Int[] Neighbors =
    {
        new(0, 1), new(0, -1), new(1, 0), new(-1, 0),
        new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
    };

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, Employee excludeEmployee)
    {
        var gm = GridManager.Instance;
        if (gm == null) return new List<Vector2Int>();
        if (!gm.IsInBounds(goal)) return new List<Vector2Int>();
        if (start == goal) return new List<Vector2Int> { goal };

        var open = new List<(Vector2Int cell, int g, int f)>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int> { [start] = 0 };
        open.Add((start, 0, Heuristic(start, goal)));

        while (open.Count > 0)
        {
            open.Sort((a, b) => a.f.CompareTo(b.f));
            var (current, g, _) = open[0];
            open.RemoveAt(0);

            if (current == goal)
            {
                var path = ReconstructPath(cameFrom, current, start);
                return path;
            }

            foreach (var delta in Neighbors)
            {
                var next = current + delta;
                if (!gm.IsWalkable(next, goal, excludeEmployee)) continue;

                int nextG = g + 1;
                if (gScore.TryGetValue(next, out var existing) && nextG >= existing) continue;
                cameFrom[next] = current;
                gScore[next] = nextG;
                open.Add((next, nextG, nextG + Heuristic(next, goal)));
            }
        }

        return new List<Vector2Int>();
    }

    static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int goal, Vector2Int start)
    {
        var path = new List<Vector2Int>();
        var current = goal;
        while (cameFrom.TryGetValue(current, out var prev))
        {
            path.Add(current);
            current = prev;
        }
        path.Reverse();
        if (path.Count > 0 && path[0] == start)
            path.RemoveAt(0);
        return path;
    }
}
