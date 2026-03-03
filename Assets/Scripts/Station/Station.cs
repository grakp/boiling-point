using System;
using System.Collections.Generic;
using UnityEngine;

public enum StationType { Prep, Stove, Assemble, Serve }

public class Station : MonoBehaviour, IMovementTarget
{
    [SerializeField] StationType stationType;
    [SerializeField] GameObject assignedEmployee;
    [SerializeField] Transform progressFill;
    [SerializeField] Vector3 progressFillFullScale = Vector3.one;
    [SerializeField] Transform standPoint;
    [SerializeField] StationBuffer inputBuffer;
    [SerializeField] StationBuffer outputBuffer;
    [SerializeField] PantryInventory pantry;
    [SerializeField] StationAction[] availableActions;

    TaskStep currentTask;
    float progress;

    readonly Queue<StationWorkRequest> workQueue = new();
    StationWorkRequest currentWork;
    List<Vector2Int> registeredBlockedCells;

    public StationType Type => stationType;
    public GameObject AssignedEmployee => assignedEmployee;
    public StationBuffer InputBuffer => inputBuffer;
    public StationBuffer OutputBuffer => outputBuffer;
    public StationAction[] AvailableActions => availableActions;

    public IEnumerable<StationWorkRequest> PendingWork => workQueue;

    public event Action<Station> OnTaskComplete;
    public event Action<Station, StationWorkRequest> OnWorkCompleted;

    void OnValidate()
    {
        if (progressFill != null)
        {
            if (progressFillFullScale.x <= 0.01f && progressFill.localScale.x > 0.01f)
                progressFillFullScale = progressFill.localScale;
        }
    }

    void Start()
    {
        SetProgressBarVisible(currentTask != null);
        if (registeredBlockedCells == null) RegisterGridObstacle();
    }

    void OnEnable()
    {
        RegisterGridObstacle();
    }

    void OnDisable()
    {
        if (GridManager.Instance != null && registeredBlockedCells != null)
        {
            GridManager.Instance.UnregisterBlockedCells(registeredBlockedCells);
            registeredBlockedCells = null;
        }
    }

    void RegisterGridObstacle()
    {
        if (GridManager.Instance == null) return;
        if (registeredBlockedCells != null) return;
        // register main station cell
        var cells = new List<Vector2Int> { GridManager.Instance.WorldToCell(transform.position) };
        // register stand point cell if exists
        if (standPoint != null)
        {
            var standCell = GridManager.Instance.WorldToCell(standPoint.position);
            if (!cells.Contains(standCell)) cells.Add(standCell);
        }
        GridManager.Instance.RegisterBlockedCells(cells);
        registeredBlockedCells = cells;
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
            if (currentTask.Duration <= 0f)
            {
                progress = 1f;
                CompleteTask();
                return;
            }

            progress += Time.deltaTime / currentTask.Duration;
            // check if task is complete
            if (progress >= 1f)
            {
                progress = 1f;
                CompleteTask();
                return;
            }
        }

        RefreshProgressFill();
    }

    public void AssignEmployee(GameObject employee)
    {
        assignedEmployee = employee;
    }

    public void ClearEmployee()
    {
        assignedEmployee = null;
    }

    void AssignTask(TaskStep task)
    {
        currentTask = task;
        progress = 0f;
        if (progressFill != null)
            progressFill.localScale = new Vector3(0f, progressFillFullScale.y, progressFillFullScale.z);
        SetProgressBarVisible(true);
    }

    void ClearTask()
    {
        currentTask = null;
        progress = 0f;
        if (progressFill != null)
            progressFill.localScale = new Vector3(0f, progressFillFullScale.y, progressFillFullScale.z);
        SetProgressBarVisible(false);
    }

    void CompleteTask()
    {
        if (currentWork != null)
        {
            // add outputs to output buffer
            if (outputBuffer != null && currentWork.Step.Outputs != null)
                outputBuffer.AddItems(currentWork.Step.Outputs);

            var finishedWork = currentWork;
            currentWork = null;
            OnWorkCompleted?.Invoke(this, finishedWork);
            OnTaskComplete?.Invoke(this);
            ClearTask();

            // handle order completion
            if (finishedWork.Order != null)
                finishedWork.Order.HandleStationWorkCompleted(finishedWork);

            // enqueue repeat work if needed
            if (finishedWork.Order == null && finishedWork.IsRepeat)
            {
                var repeatRequest = new StationWorkRequest(null, -1, finishedWork.Step, isRepeat: true);
                EnqueueWork(repeatRequest);
            }

            // try to start next feasible work
            TryStartNextFeasibleWork();
        }
        else
        {
            OnTaskComplete?.Invoke(this);
            ClearTask();
        }
    }

    public Vector2 GetTargetPosition(Vector2? hitPoint = null)
    {
        return standPoint != null ? (Vector2)standPoint.position : (Vector2)transform.position;
    }

    public void SetHoverHighlight(bool on) { }

    public void EnqueueWork(StationWorkRequest request)
    {
        if (request == null || request.Step == null) return;
        workQueue.Enqueue(request);
        TryStartNextFeasibleWork();
    }

    public void TryStartNextFeasibleWork()
    {
        if (currentWork != null || currentTask != null) return;
        if (assignedEmployee == null) return;
        if (inputBuffer == null || outputBuffer == null) return;

        int count = workQueue.Count;
        for (int i = 0; i < count; i++)
        {
            var request = workQueue.Dequeue();
            if (CanStartWork(request))
            {
                StartWork(request);
                return;
            }
            if (request.IsRepeat)
            {
                bool pantryHasInputs = pantry != null && request.Step.Inputs != null && pantry.HasAll(request.Step.Inputs);
                bool outputFull = outputBuffer != null && !outputBuffer.CanFit(request.Step.Outputs);
                if (!pantryHasInputs || outputFull) continue;
            }
            workQueue.Enqueue(request);
        }
    }

    bool CanStartWork(StationWorkRequest request)
    {
        if (request == null || request.Step == null) return false;
        var step = request.Step;
        // check if input buffer has all required inputs
        if (!inputBuffer.HasAll(step.Inputs)) return false;
        // check if output buffer can fit all required outputs
        if (!outputBuffer.CanFit(step.Outputs)) return false;
        return true;
    }

    void StartWork(StationWorkRequest request)
    {
        currentWork = request;
        // remove inputs from input buffer
        if (inputBuffer != null && request.Step.Inputs != null)
            inputBuffer.RemoveItems(request.Step.Inputs);
        AssignTask(request.Step);
    }

    public void CancelManualWork()
    {
        if (currentWork != null && currentWork.Order == null)
        {
            // refund inputs to pantry
            if (currentWork.Step.Inputs != null)
            {
                if (pantry != null)
                {
                    foreach (var stack in currentWork.Step.Inputs)
                        pantry.Add(stack.type, stack.amount);
                }
                else if (inputBuffer != null)
                    inputBuffer.AddItems(currentWork.Step.Inputs);
            }
            currentWork = null;
            ClearTask();
        }
    }

    public void ClearPendingManualRequests()
    {
        // enqueue pending manual requests
        int count = workQueue.Count;
        for (int i = 0; i < count; i++)
        {
            var request = workQueue.Dequeue();
            if (request.Order != null)
                workQueue.Enqueue(request);
        }
    }
}

