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

    // set path to given list of cells and target
    public void SetPath(List<Vector2Int> newPath, IMovementTarget target = null)
    {
        path.Clear();
        if (newPath != null)
            path.AddRange(newPath);
        pathIndex = 0;
        destinationTarget = target;
    }

    // set pending goal cell and target
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

    // check if employee is moving
    public bool IsMoving => path != null && pathIndex < path.Count;

    void Update()
    {
        if (GridManager.Instance == null || employee == null || pathIndex >= path.Count) return;

        Vector2 waypoint = GridManager.Instance.CellToWorld(path[pathIndex]);
        Vector2 pos = transform.position;
        float speed = tilesPerSecond * employee.SpeedMultiplier * Time.deltaTime;
        float dist = Vector2.Distance(pos, waypoint);
        Vector2 next = dist <= speed ? waypoint : Vector2.MoveTowards(pos, waypoint, speed);
        transform.position = new Vector3(next.x, next.y, transform.position.z);

        // check if employee has arrived at waypoint
        if (Vector2.Distance(next, waypoint) <= arrivalThreshold)
        {
            var currentCell = path[pathIndex];
            // set employee cell to current cell
            GridManager.Instance.SetEmployeeCell(employee, currentCell);
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                OnArrived?.Invoke(employee, destinationTarget);
                destinationTarget = null;
            }
            // check if there is a pending goal cell
            if (pendingGoalCell.HasValue)
            {
                // find path to pending goal cell
                var newPath = Pathfinder.FindPath(currentCell, pendingGoalCell.Value, employee);
                // if path is found, set path and clear pending goal cell and target
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
