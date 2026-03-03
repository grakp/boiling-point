using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class EmployeeSelectionManager : MonoBehaviour
{
    const string EmployeeTag = "Employee";
    const string MovementTargetTag = "MovementTarget";

    [SerializeField] EmployeeActionMenu actionMenu;

    Employee selectedEmployee;

    public void DeselectEmployee()
    {
        if (selectedEmployee != null)
            selectedEmployee.SetSelected(false);

        selectedEmployee = null;

        if (actionMenu != null)
            actionMenu.Hide();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Employee hitEmployee = GetHitEmployee(hits);
            if (hitEmployee != null)
            {
                if (selectedEmployee == hitEmployee)
                    DeselectEmployee();
                else
                {
                    if (selectedEmployee != null)
                        selectedEmployee.SetSelected(false);
                    selectedEmployee = hitEmployee;
                    selectedEmployee.SetSelected(true);
                    if (actionMenu != null)
                    {
                        foreach (var station in Object.FindObjectsByType<Station>(FindObjectsSortMode.None))
                        {
                            if (station != null && station.AssignedEmployee == selectedEmployee.gameObject &&
                                station.AvailableActions != null && station.AvailableActions.Length > 0)
                            {
                                actionMenu.Show(station, selectedEmployee, station.AvailableActions);
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame && selectedEmployee != null)
        {
            var movement = selectedEmployee.GetComponent<EmployeeMovement>();
            if (movement != null && GridManager.Instance != null)
            {
                IMovementTarget hitTarget = GetHitMovementTarget(hits);
                Vector2Int goalCell;
                Vector2 targetWorld;
                IMovementTarget pathTarget = null;
                if (hitTarget != null)
                {
                    Station hitStation = null;
                    foreach (var c in hits)
                    {
                        if (c.GetComponentInParent<Station>() is Station s) { hitStation = s; break; }
                    }
                    pathTarget = hitStation != null ? (IMovementTarget)hitStation : hitTarget;
                    targetWorld = pathTarget.GetTargetPosition(worldPos);
                    goalCell = GridManager.Instance.WorldToCell(targetWorld);
                }
                else
                {
                    goalCell = GridManager.Instance.WorldToCell(worldPos);
                    targetWorld = (Vector2)GridManager.Instance.CellToWorld(goalCell);
                }
                if (!GridManager.Instance.IsInBounds(goalCell))
                {
                    var snapped = GridManager.Instance.GetNearestWalkableCell(goalCell);
                    if (!snapped.HasValue) return;
                    goalCell = snapped.Value;
                    targetWorld = (Vector2)GridManager.Instance.CellToWorld(goalCell);
                }
                UnassignFromStations(selectedEmployee.gameObject);
                movement.OnArrived -= HandleEmployeeArrived;
                movement.OnArrived += HandleEmployeeArrived;
                movement.SetPendingGoal(goalCell, pathTarget);
            }
        }
    }

    static Employee GetHitEmployee(Collider2D[] hits)
    {
        foreach (var c in hits)
        {
            if (HasTagInHierarchy(c.transform, EmployeeTag))
                return c.GetComponentInParent<Employee>();
        }
        foreach (var c in hits)
        {
            if (c.GetComponentInParent<Employee>() is Employee e) return e;
        }
        return null;
    }

    static IMovementTarget GetHitMovementTarget(Collider2D[] hits)
    {
        foreach (var c in hits)
        {
            if (HasTagInHierarchy(c.transform, MovementTargetTag))
                return GetTransformWithTag(c.transform, MovementTargetTag)?.GetComponent<IMovementTarget>();
        }
        foreach (var c in hits)
        {
            if (c.GetComponentInParent<IMovementTarget>() is IMovementTarget t) return t;
        }
        return null;
    }

    static bool HasTagInHierarchy(Transform t, string tag)
    {
        while (t != null)
        {
            if (t.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }

    static Transform GetTransformWithTag(Transform t, string tag)
    {
        while (t != null)
        {
            if (t.CompareTag(tag)) return t;
            t = t.parent;
        }
        return null;
    }

    void HandleEmployeeArrived(Employee emp, IMovementTarget target)
    {
        var movement = emp.GetComponent<EmployeeMovement>();
        if (movement != null) movement.OnArrived -= HandleEmployeeArrived;
        if (target is Station station)
        {
            station.AssignEmployee(emp.gameObject);
            if (actionMenu != null && station.AvailableActions != null && station.AvailableActions.Length > 0)
                actionMenu.Show(station, emp, station.AvailableActions);
        }
        else if (actionMenu != null)
            actionMenu.Hide();
    }

    static void UnassignFromStations(GameObject employee)
    {
        foreach (var station in Object.FindObjectsByType<Station>(FindObjectsSortMode.None))
            if (station.AssignedEmployee == employee)
                station.ClearEmployee();
    }
}
