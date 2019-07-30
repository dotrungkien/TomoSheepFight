using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    public Image loading;
    public int sec;
    int totalSeconds;
    float fillamount;

    void Start()
    {
        StartCoroutine(second());
        totalSeconds = sec;
    }

    void Update()
    {
        if (sec == 0) StopCoroutine(second());
    }

    IEnumerator second()
    {
        yield return new WaitForSeconds(1f);
        if (sec > 0) sec--;

        fillLoading();
        StartCoroutine(second());
    }

    void fillLoading()
    {
        float fill = (float)sec / totalSeconds;
        loading.fillAmount = fill;
    }
}