using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderQueueUI : MonoBehaviour
{
    [SerializeField] CustomerOrderQueueManager queueManager;
    [SerializeField] OrderQueueRowView rowPrefab;
    [SerializeField] RectTransform container;
    [SerializeField] float slotWidth = 300f;
    [SerializeField] float padding = 0f;

    readonly List<OrderQueueRowView> rows = new List<OrderQueueRowView>();

    void Update()
    {
        if (queueManager == null || container == null) return;

        var queue = queueManager.OrderQueue;
        while (rows.Count < queue.Count)
        {
            var row = Instantiate(rowPrefab, container);
            rows.Add(row);
        }
        while (rows.Count > queue.Count)
        {
            var last = rows.Count - 1;
            Destroy(rows[last].gameObject);
            rows.RemoveAt(last);
        }

        for (int i = 0; i < queue.Count; i++)
        {
            var rowRect = rows[i].transform as RectTransform;
            if (rowRect != null)
            {
                rowRect.anchorMin = new Vector2(0f, 1f);
                rowRect.anchorMax = new Vector2(0f, 1f);
                rowRect.anchoredPosition = new Vector2(padding + i * slotWidth, -padding);
            }
            var order = queue[i];
            rows[i].SetIcon(order.Recipe != null ? order.Recipe.DisplayIcon : null);
            rows[i].SetProgress(order.PatienceSeconds > 0 ? order.TimeRemaining / order.PatienceSeconds : 0f);
        }
    }
}
