using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public GameObject laneEffect;

    void OnMouseDown()
    {
        var effect = Instantiate(laneEffect, transform.position, Quaternion.identity, transform);
        GameObject.Destroy(effect, 0.15f);
    }
}
