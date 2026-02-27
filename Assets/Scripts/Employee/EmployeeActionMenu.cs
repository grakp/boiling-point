using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EmployeeActionMenu : MonoBehaviour
{
    [SerializeField] RectTransform container;
    [SerializeField] Button actionButtonPrefab;
    [SerializeField] Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);

    readonly List<Button> spawnedButtons = new();
    Station currentStation;
    Employee currentEmployee;
    [SerializeField] EmployeeSelectionManager selectionManager;

    public void Show(Station station, Employee employee, StationAction[] actions)
    {
        currentStation = station;
        currentEmployee = employee;

        if (station == null || employee == null || actions == null || actions.Length == 0)
        {
            Hide();
            return;
        }

        if (container == null || actionButtonPrefab == null) return;

        ClearButtons();

        Vector3 worldPos = employee.transform.position + worldOffset;
        transform.position = worldPos;

        if (Camera.main != null)
            transform.rotation = Camera.main.transform.rotation;

        for (int i = 0; i < actions.Length; i++)
        {
            var action = actions[i];
            if (action == null) continue;
            if (action.RequiredStation != station.Type) continue;

            var button = Instantiate(actionButtonPrefab, container);
            spawnedButtons.Add(button);

            var label = button.GetComponentInChildren<Text>();
            if (label != null) label.text = action.ActionName;

            var image = button.GetComponentInChildren<Image>();
            if (image != null && action.Icon != null) image.sprite = action.Icon;

            var handler = button.gameObject.AddComponent<EmployeeActionMenuButtonHandler>();
            handler.Initialize(this, action);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        ClearButtons();
        gameObject.SetActive(false);
        currentStation = null;
        currentEmployee = null;
    }

    void ClearButtons()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
                Destroy(spawnedButtons[i].gameObject);
        }
        spawnedButtons.Clear();
    }

    public void HandleActionClick(StationAction action, bool repeat)
    {
        if (currentStation == null || action == null) return;

        // Cancel in-progress manual work (refund inputs) and drop any other queued manual requests.
        currentStation.CancelManualWork();
        currentStation.ClearPendingManualRequests();

        var step = new TaskStep(action.RequiredStation, action.Duration, action.Inputs, action.Outputs);

        var request = new StationWorkRequest(null, -1, step, isRepeat: repeat);
        var who = currentEmployee != null ? currentEmployee.name : "(no employee)";
        Debug.Log($"[ActionMenu] {who} chose action '{action.ActionName}' on station '{currentStation.name}'.");
        currentStation.EnqueueWork(request);

        if (selectionManager != null)
            selectionManager.DeselectEmployee();
        else
            Hide();
    }
}

public class EmployeeActionMenuButtonHandler : MonoBehaviour, IPointerClickHandler
{
    EmployeeActionMenu menu;
    StationAction action;

    public void Initialize(EmployeeActionMenu menu, StationAction action)
    {
        this.menu = menu;
        this.action = action;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (menu == null || action == null) return;
        bool repeat = eventData.button == PointerEventData.InputButton.Right;
        menu.HandleActionClick(action, repeat);
    }
}

