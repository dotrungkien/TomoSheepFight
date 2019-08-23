using UnityEngine;
using System.Collections;

public class BackgroundScaling : MonoBehaviour
{
    public GameObject background;

    void Start()
    {
        float bgWidth = background.GetComponent<BoxCollider2D>().size.x;
        float bgHeight = background.GetComponent<BoxCollider2D>().size.y;
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;
        float scaleRatioX = screenWidth / bgWidth;
        float scaleRatioY = screenHeight / bgHeight;

        transform.localScale = new Vector3(scaleRatioX, scaleRatioY, 1);
    }
}
