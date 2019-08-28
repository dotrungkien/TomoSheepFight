using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class Lane : MonoBehaviour
{
    public GameController gameController;
    public GameObject laneEffect;
    public int index;

    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!gameController.isPlaying) return;
        var effect = Instantiate(laneEffect, transform.position, Quaternion.identity, transform);
        GameObject.Destroy(effect, 0.15f);
        gameController.SpawnLane(index);
    }
}
