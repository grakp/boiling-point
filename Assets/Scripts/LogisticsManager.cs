using System;
using UnityEngine;

public class LogisticsManager : MonoBehaviour
{
    [SerializeField] PantryInventory pantry;
    [SerializeField] Station[] stations;

    void Update()
    {
        if (pantry == null || stations == null) return;

        foreach (var station in stations)
        {
            if (station == null) continue;

            // 1. Fill station input buffer for pending work from pantry
            var input = station.InputBuffer;
            if (input != null)
            {
                foreach (var work in station.PendingWork)
                {
                    var step = work.Step;
                    if (step == null || step.inputs == null) continue;

                    for (int i = 0; i < step.inputs.Length; i++)
                    {
                        var req = step.inputs[i];
                        int already = input.Inventory.GetAmount(req.type);
                        int needed = req.amount - already;
                        if (needed <= 0) continue;

                        int moved = pantry.Inventory.Remove(req.type, needed);
                        if (moved > 0)
                        {
                            Debug.Log($"[Logistics] Moved {moved}x {req.type} from pantry to input buffer of station '{station.name}'.");
                            input.Inventory.Add(req.type, moved, input.MaxTotalAmount);
                        }
                    }
                }
            }

            // 2. Move everything from station output buffer back into pantry
            var output = station.OutputBuffer;
            if (output != null)
            {
                foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
                {
                    if (type == ItemType.None) continue;

                    int amount = output.Inventory.GetAmount(type);
                    if (amount <= 0) continue;

                    int removed = output.Inventory.Remove(type, amount);
                    if (removed > 0)
                    {
                        Debug.Log($"[Logistics] Moved {removed}x {type} from output buffer of station '{station.name}' to pantry.");
                        pantry.Inventory.Add(type, removed);
                    }
                }
            }

            // 3. Try to start any newly feasible work on this station
            station.TryStartNextFeasibleWork();
        }
    }
}

