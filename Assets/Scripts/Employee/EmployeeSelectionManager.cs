using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class EmployeeSelectionManager : MonoBehaviour
{
    const string EmployeeTag = "Employee";
    const string MovementTargetTag = "MovementTarget";

    [SerializeField] EmployeeActionMenu actionMenu;

    Employee selectedEmployee;
    IMovementTarget lastHoveredTarget;

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
            Employee hitEmployee = null;
            IMovementTarget hitTarget = null;

            foreach (var c in hits)
            {
                if (hitEmployee == null && HasTagInHierarchy(c.transform, EmployeeTag))
                    hitEmployee = c.GetComponentInParent<Employee>();
                if (hitTarget == null && HasTagInHierarchy(c.transform, MovementTargetTag))
                    hitTarget = GetTransformWithTag(c.transform, MovementTargetTag)?.GetComponent<IMovementTarget>();
            }

            if (hitEmployee == null || hitTarget == null)
            {
                foreach (var c in hits)
                {
                    if (hitEmployee == null && c.GetComponentInParent<Employee>() is Employee e) hitEmployee = e;
                    if (hitTarget == null && c.GetComponentInParent<IMovementTarget>() is IMovementTarget t) hitTarget = t;
                }
            }

            if (hitTarget != null && selectedEmployee != null)
            {
                Station hitStation = null;
                foreach (var c in hits)
                {
                    if (c.GetComponentInParent<Station>() is Station s)
                    {
                        hitStation = s;
                        break;
                    }
                }
                if (hitStation != null)
                    hitTarget = hitStation;
            }

            if (hitEmployee != null)
            {
                if (selectedEmployee == hitEmployee)
                {
                    DeselectEmployee();
                }
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
            else if (hitTarget != null && selectedEmployee != null)
            {
                UnassignFromStations(selectedEmployee.gameObject);
                Vector2 targetPos = hitTarget.GetTargetPosition(worldPos);
                selectedEmployee.transform.position = new Vector3(targetPos.x, targetPos.y, selectedEmployee.transform.position.z);

                if (hitTarget is Station station)
                {
                    station.AssignEmployee(selectedEmployee.gameObject);
                    if (actionMenu != null && station.AvailableActions != null && station.AvailableActions.Length > 0)
                        actionMenu.Show(station, selectedEmployee, station.AvailableActions);
                }
                else if (actionMenu != null)
                {
                    actionMenu.Hide();
                }
            }
        }

        IMovementTarget hoveredTarget = null;
        foreach (var c in hits)
        {
            if (HasTagInHierarchy(c.transform, MovementTargetTag))
                hoveredTarget = GetTransformWithTag(c.transform, MovementTargetTag)?.GetComponent<IMovementTarget>();
            if (hoveredTarget == null && c.GetComponentInParent<IMovementTarget>() is IMovementTarget t)
                hoveredTarget = t;
            if (hoveredTarget != null) break;
        }

        if (hoveredTarget != lastHoveredTarget)
        {
            if (lastHoveredTarget != null) lastHoveredTarget.SetHoverHighlight(false);
            lastHoveredTarget = hoveredTarget;
            if (lastHoveredTarget != null) lastHoveredTarget.SetHoverHighlight(true);
        }
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

    static void UnassignFromStations(GameObject employee)
    {
        foreach (var station in Object.FindObjectsByType<Station>(FindObjectsSortMode.None))
            if (station.AssignedEmployee == employee)
                station.ClearEmployee();
    }
}
