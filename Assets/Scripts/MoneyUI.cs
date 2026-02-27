using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] CustomerOrderQueueManager queueManager;
    [SerializeField] TextMeshProUGUI amountText;

    void Update()
    {
        if (amountText != null && queueManager != null)
            amountText.text = queueManager.Money.ToString();
    }
}
