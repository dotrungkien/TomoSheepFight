using UnityEngine;
using UnityEngine.UI;

public class RadialProgress : MonoBehaviour
{
    public Image LoadingBar;
    float currentValue;
    public float speed;

    void Update()
    {
        if (currentValue < 100)
        {
            currentValue += speed * Time.deltaTime;
        }
        LoadingBar.fillAmount = currentValue / 100;
    }
}
