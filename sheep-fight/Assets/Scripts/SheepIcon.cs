using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SheepIcon : MonoBehaviour
{
    public Image LoadingBar;
    public Sprite[] sheeps;
    public Image icon;
    public bool isActive = false;

    public float cooldown = 5f;
    private float progress = 5f;
    private bool firstTime = true;

    public void SwitchSheep(int idx)
    {
        if (idx < 0 || idx >= sheeps.Length) return;
        icon.sprite = sheeps[idx];
        if (firstTime)
        {
            firstTime = false;
            return;
        }
        ResetCooldown();
    }

    public void ResetCooldown()
    {
        progress = 0f;
    }

    private void Update()
    {
        if (!isActive)
        {
            LoadingBar.fillAmount = 0f;
            return;
        }
        if (progress < cooldown) progress += Time.deltaTime;
        LoadingBar.fillAmount = progress / cooldown;
    }
}
