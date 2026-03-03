using System;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeMovement : MonoBehaviour
{
    [SerializeField] float tilesPerSecond = 2f;
    [SerializeField] float arrivalThreshold = 0.05f;

    Employee employee;
    List<Vector2Int> path = new();
    int pathIndex;
    IMovementTarget destinationTarget;
    Vector2Int? pendingGoalCell;
    IMovementTarget pendingGoalTarget;
    public Action<Employee, IMovementTarget> OnArrived;

    void Awake()
    {
        employee = GetComponent<Employee>();
    }

    void Start()
    {
        if (GridManager.Instance != null && employee != null)
        {
            var cell = GridManager.Instance.WorldToCell(transform.position);
            GridManager.Instance.SetEmployeeCell(employee, cell);
        }
    }

    void OnDestroy()
    {
        if (GridManager.Instance != null && employee != null)
            GridManager.Instance.RemoveEmployee(employee);
    }

    public void SetPath(List<Vector2Int> newPath, IMovementTarget target = null)
    {
        path.Clear();
        if (newPath != null)
            path.AddRange(newPath);
        pathIndex = 0;
        destinationTarget = target;
    }

    public void SetPendingGoal(Vector2Int goalCell, IMovementTarget target = null)
    {
        if (GridManager.Instance == null || employee == null) return;
        pendingGoalCell = goalCell;
        pendingGoalTarget = target;
        if (pathIndex >= path.Count)
        {
            var cell = GridManager.Instance.GetEmployeeCell(employee);
            if (!cell.HasValue) return;
            var newPath = Pathfinder.FindPath(cell.Value, goalCell, employee);
            if (newPath.Count > 0)
            {
                SetPath(newPath, target);
                pendingGoalCell = null;
                pendingGoalTarget = null;
            }
        }
    }

    public bool IsMoving => path != null && pathIndex < path.Count;

    void Update()
    {
        if (GridManager.Instance == null || employee == null || pathIndex >= path.Count) return;

        Vector2 waypoint = GridManager.Instance.CellToWorld(path[pathIndex]);
        Vector2 pos = transform.position;
        float speed = tilesPerSecond * employee.SpeedMultiplier * Time.deltaTime;
        Vector2 next = Vector2.MoveTowards(pos, waypoint, speed);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        if (Vector2.Distance(next, waypoint) <= arrivalThreshold)
        {
            var currentCell = path[pathIndex];
            GridManager.Instance.SetEmployeeCell(employee, currentCell);
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                OnArrived?.Invoke(employee, destinationTarget);
                destinationTarget = null;
            }
            if (pendingGoalCell.HasValue)
            {
                var newPath = Pathfinder.FindPath(currentCell, pendingGoalCell.Value, employee);
                if (newPath.Count > 0)
                {
                    SetPath(newPath, pendingGoalTarget);
                    pendingGoalCell = null;
                    pendingGoalTarget = null;
                }
                else
                {
                    pendingGoalCell = null;
                    pendingGoalTarget = null;
                }
            }
        }
    }
}
