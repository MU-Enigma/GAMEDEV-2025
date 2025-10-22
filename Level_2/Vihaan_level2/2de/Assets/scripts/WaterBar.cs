using UnityEngine;
using UnityEngine.UI;

public class WaterBar : MonoBehaviour
{
    public Image waterFill;   // drag WaterBarFill here in Inspector
    public float maxWater = 100f;
    private float currentWater;

    void Start()
    {
        currentWater = 0f;
        UpdateBar();
    }

    public void AddWater(float amount)
    {
        currentWater = Mathf.Clamp(currentWater + amount, 0, maxWater);
        UpdateBar();
    }

    public void UseWater(float amount)
    {
        currentWater = Mathf.Clamp(currentWater - amount, 0, maxWater);
        UpdateBar();
    }

    private void UpdateBar()
    {
        waterFill.fillAmount = currentWater / maxWater;
    }
}

