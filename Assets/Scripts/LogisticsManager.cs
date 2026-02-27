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
                    if (step == null || step.Inputs == null) continue;

                    for (int i = 0; i < step.Inputs.Length; i++)
                    {
                        var req = step.Inputs[i];
                        int already = input.GetAmount(req.type);
                        int needed = req.amount - already;
                        if (needed <= 0) continue;

                        int moved = pantry.Remove(req.type, needed);
                        if (moved > 0)
                        {
                            input.Add(req.type, moved, input.MaxTotalAmount);
                            Debug.Log($"[Logistics] Moved {moved}x {req.type} from pantry to input buffer of station '{station.name}'.");
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

                    int amount = output.GetAmount(type);
                    if (amount <= 0) continue;

                    output.Remove(type, amount);
                    pantry.Add(type, amount);
                    Debug.Log($"[Logistics] Moved {amount}x {type} from output buffer of station '{station.name}' to pantry.");
                }
            }

            // 3. Try to start any newly feasible work on this station
            station.TryStartNextFeasibleWork();
        }
    }
}

