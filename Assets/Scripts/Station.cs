using System;
using UnityEngine;

public enum StationType { Prep, Stove, Assemble, Serve }

public class Station : MonoBehaviour, IMovementTarget
{
    [SerializeField] StationType stationType;
    [SerializeField] GameObject assignedEmployee;
    [SerializeField] Transform progressFill;
    [SerializeField] Vector3 progressFillFullScale = Vector3.one;
    [SerializeField] Transform standPoint;
    [SerializeField] SpriteRenderer highlightVisual;

    TaskStep currentTask;
    float progress;

    public StationType Type => stationType;
    public GameObject AssignedEmployee => assignedEmployee;
    public TaskStep CurrentTask => currentTask;
    public float Progress => progress;

    public event Action<Station> OnTaskComplete;

    void Start()
    {
        SetProgressBarVisible(assignedEmployee != null);
    }

    void SetProgressBarVisible(bool visible)
    {
        if (progressFill != null && progressFill.parent != null)
        {
            progressFill.parent.gameObject.SetActive(visible);
            if (visible) RefreshProgressFill();
        }
    }

    float GetProgressFillScaleX()
    {
        float fullX = progressFillFullScale.x > 0.01f ? progressFillFullScale.x : 1f;
        return fullX * progress;
    }

    void RefreshProgressFill()
    {
        if (progressFill == null || currentTask == null) return;
        progressFill.localScale = new Vector3(GetProgressFillScaleX(), progressFillFullScale.y, progressFillFullScale.z);
    }

    void Update()
    {
        if (currentTask == null) return;

        if (assignedEmployee != null)
        {
            progress += Time.deltaTime / currentTask.duration;
            if (progress >= 1f)
            {
                progress = 1f;
                CompleteTask();
            }
        }

        if (assignedEmployee != null && progressFill != null)
            progressFill.localScale = new Vector3(GetProgressFillScaleX(), progressFillFullScale.y, progressFillFullScale.z);
    }

    public void AssignEmployee(GameObject employee)
    {
        assignedEmployee = employee;
        SetProgressBarVisible(true);
    }

    public void ClearEmployee()
    {
        assignedEmployee = null;
        SetProgressBarVisible(false);
    }

    public void AssignTask(TaskStep task)
    {
        currentTask = task;
        progress = 0f;
        if (progressFill != null)
            progressFill.localScale = new Vector3(0f, progressFillFullScale.y, progressFillFullScale.z);
    }

    public void ClearTask()
    {
        currentTask = null;
        progress = 0f;
        if (progressFill != null)
            progressFill.localScale = new Vector3(0f, progressFillFullScale.y, progressFillFullScale.z);
    }

    void CompleteTask()
    {
        OnTaskComplete?.Invoke(this);
        ClearTask();
    }

    public Vector2 GetTargetPosition(Vector2? hitPoint = null)
    {
        return standPoint != null ? (Vector2)standPoint.position : (Vector2)transform.position;
    }

    public void SetHoverHighlight(bool on)
    {
        if (highlightVisual != null) highlightVisual.enabled = on;
    }
}
