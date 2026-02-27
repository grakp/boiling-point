using System;
using UnityEngine;

[Serializable]
public class TaskStep
{
    [SerializeField] StationType requiredStation;
    [SerializeField] float duration;
    [SerializeField] ItemStack[] inputs;
    [SerializeField] ItemStack[] outputs;

    public StationType RequiredStation => requiredStation;
    public float Duration => duration;
    public ItemStack[] Inputs => inputs;
    public ItemStack[] Outputs => outputs;

    public TaskStep() { }

    public TaskStep(StationType requiredStation, float duration, ItemStack[] inputs, ItemStack[] outputs)
    {
        this.requiredStation = requiredStation;
        this.duration = duration;
        this.inputs = inputs;
        this.outputs = outputs;
    }
}
