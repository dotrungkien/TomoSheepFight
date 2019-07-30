using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepIcon : MonoBehaviour
{

    public Sprite[] sheeps;

    public void SwitchSheep(int idx)
    {
        if (idx < 0 || idx >= sheeps.Length) return;
        GetComponent<SpriteRenderer>().sprite = sheeps[idx];
    }
}
