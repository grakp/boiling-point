using System;
using UnityEngine;

[Serializable]
public class TaskStep
{
    public StationType requiredStation;
    public float duration;
    public ItemStack[] inputs;
    public ItemStack[] outputs;
}
