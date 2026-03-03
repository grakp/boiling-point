using UnityEngine;
using UnityEngine.UI;

public class OrderQueueRowView : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image progressFillImage;

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null) iconImage.sprite = sprite;
    }

    // set progress fill amount
    public void SetProgress(float normalized)
    {
        if (progressFillImage == null) return;
        progressFillImage.type = Image.Type.Filled;
        progressFillImage.fillMethod = Image.FillMethod.Horizontal;
        progressFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        progressFillImage.fillAmount = Mathf.Clamp01(normalized);
    }
}
