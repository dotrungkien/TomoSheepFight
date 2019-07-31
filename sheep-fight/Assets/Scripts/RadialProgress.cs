using UnityEngine;
using UnityEngine.UI;

public class RadialProgress : MonoBehaviour
{
    public Image LoadingBar;
    private float currentValue;

    void Update()
    {
        if (currentValue < 3f)
        {
            currentValue += Time.deltaTime;
        }
        LoadingBar.fillAmount = currentValue / 3f;
    }

    public void ResetCooldown()
    {
        currentValue = 0f;
    }
}
