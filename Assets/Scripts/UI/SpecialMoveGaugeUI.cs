using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveGaugeUI : MonoBehaviour
{
    [SerializeField] Image gaugeFill; // Image with Image Type: Filled

    SpecialMoveSystem specialMoveSystem;
    
    Color originalColor;
    bool isFull = false;
    float hue = 0f;

    void Awake()
    {
        if (gaugeFill)
        {
            originalColor = gaugeFill.color;
        }
    }

    void OnEnable()
    {
        if (!specialMoveSystem) specialMoveSystem = FindObjectOfType<SpecialMoveSystem>();
        if (specialMoveSystem)
        {
            specialMoveSystem.OnGaugeChanged += UpdateGauge;
        }
    }

    void OnDisable()
    {
        if (specialMoveSystem)
        {
            specialMoveSystem.OnGaugeChanged -= UpdateGauge;
        }
    }
    
    void Update()
    {
        // When full, cycle through rainbow colors using HSV
        if (isFull && gaugeFill)
        {
            hue += Time.deltaTime * 2f; // Change the multiplier to adjust the speed of the rainbow effect
            if (hue > 1f) hue -= 1f;
            gaugeFill.color = Color.HSVToRGB(hue, 1f, 1f);
        }
    }

    public void UpdateGauge(float current, float max)
    {
        if (gaugeFill && max > 0)
        {
            gaugeFill.fillAmount = current / max;
            
            if (current >= max)
            {
                isFull = true;
            }
            else
            {
                // Revert to the original color when not full
                isFull = false;
                gaugeFill.color = originalColor;
            }
        }
    }
}
