using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lane : MonoBehaviour
{
    public GameController gameController;
    public GameObject laneEffect;
    public int index;

    void OnMouseDown()
    {
        var effect = Instantiate(laneEffect, transform.position, Quaternion.identity, transform);
        GameObject.Destroy(effect, 0.15f);
        gameController.SpawnLane(index);
    }
}
