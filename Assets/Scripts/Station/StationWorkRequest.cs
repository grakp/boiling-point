public class StationWorkRequest
{
    public Order Order { get; }
    public int StepIndex { get; }
    public TaskStep Step { get; }
    public bool IsRepeat { get; }

    public StationWorkRequest(Order order, int stepIndex, TaskStep step, bool isRepeat = false)
    {
        Order = order;
        StepIndex = stepIndex;
        Step = step;
        IsRepeat = isRepeat;
    }
}

