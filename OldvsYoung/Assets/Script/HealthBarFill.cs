using UnityEngine;
using UnityEngine.UI;

public class HealthBarFillUI : MonoBehaviour
{
    [SerializeField] private Image fillImage; // Assign your Foreground Image (Type = Filled)

    private void Awake()
    {
        if (fillImage == null) fillImage = GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f; // start full
        }
    }

    public void SetHealth(float current, float max)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
    }
}