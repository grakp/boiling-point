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
    [SerializeField] SpriteRenderer highlightVisual;
    [SerializeField] StationBuffer inputBuffer;
    [SerializeField] StationBuffer outputBuffer;
    [SerializeField] PantryInventory pantry;
    [SerializeField] StationAction[] availableActions;

    TaskStep currentTask;
    float progress;

    readonly Queue<StationWorkRequest> workQueue = new();
    StationWorkRequest currentWork;

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
            if (progress >= 1f)
            {
                progress = 1f;
                CompleteTask();
                return;
            }
        }

        if (progressFill != null)
            progressFill.localScale = new Vector3(GetProgressFillScaleX(), progressFillFullScale.y, progressFillFullScale.z);
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
            if (outputBuffer != null && currentWork.Step.Outputs != null)
            {
                foreach (var stack in currentWork.Step.Outputs)
                    Debug.Log($"[Station] '{name}' completed work: producing {stack.amount}x {stack.type} into output buffer.");
                outputBuffer.AddItems(currentWork.Step.Outputs);
            }

            var finishedWork = currentWork;
            currentWork = null;
            OnWorkCompleted?.Invoke(this, finishedWork);
            OnTaskComplete?.Invoke(this);
            ClearTask();

            if (finishedWork.Order != null)
                finishedWork.Order.HandleStationWorkCompleted(finishedWork);

            if (finishedWork.Order == null && finishedWork.IsRepeat)
            {
                var repeatRequest = new StationWorkRequest(null, -1, finishedWork.Step, isRepeat: true);
                EnqueueWork(repeatRequest);
            }

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

    public void SetHoverHighlight(bool on)
    {
        if (highlightVisual != null) highlightVisual.enabled = on;
    }

    public void EnqueueWork(StationWorkRequest request)
    {
        if (request == null || request.Step == null) return;
        Debug.Log($"[Station] Enqueue work on '{name}': step for {request.Step.RequiredStation}, duration {request.Step.Duration}s, manual={(request.Order == null)}.");
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
                if (!pantryHasInputs || outputFull)
                {
                    Debug.Log($"[Station] '{name}' stopping repeat (missing inputs or full output).");
                    continue;
                }
            }
            workQueue.Enqueue(request);
        }
    }

    bool CanStartWork(StationWorkRequest request)
    {
        if (request == null || request.Step == null) return false;
        var step = request.Step;
        if (!inputBuffer.HasAll(step.Inputs))
        {
            if (!request.IsRepeat)
                Debug.Log($"[Station] '{name}' cannot start work: missing inputs for step on {stationType}.");
            return false;
        }
        if (!outputBuffer.CanFit(step.Outputs))
        {
            if (!request.IsRepeat)
                Debug.Log($"[Station] '{name}' cannot start work: output buffer full for step on {stationType}.");
            return false;
        }
        return true;
    }

    void StartWork(StationWorkRequest request)
    {
        currentWork = request;
        if (inputBuffer != null && request.Step.Inputs != null)
        {
            foreach (var stack in request.Step.Inputs)
                Debug.Log($"[Station] '{name}' starting work: consuming {stack.amount}x {stack.type} from input buffer.");
            inputBuffer.RemoveItems(request.Step.Inputs);
        }
        Debug.Log($"[Station] '{name}' starting work: step for {request.Step.RequiredStation}, duration {request.Step.Duration}s, manual={(request.Order == null)}.");
        AssignTask(request.Step);
    }

    public void CancelManualWork()
    {
        if (currentWork != null && currentWork.Order == null)
        {
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
            Debug.Log($"[Station] '{name}' cancelling manual work in progress.");
            currentWork = null;
            ClearTask();
        }
    }

    public void ClearPendingManualRequests()
    {
        int count = workQueue.Count;
        for (int i = 0; i < count; i++)
        {
            var request = workQueue.Dequeue();
            if (request.Order != null)
                workQueue.Enqueue(request);
        }
    }
}

