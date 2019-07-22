﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
    public int weight;
    public int point;
    public bool isMoving;
    public int direction = 1;
    public float force = 10f;

    private Rigidbody2D body;

    void Start()
    {
        isMoving = true;
        body = GetComponent<Rigidbody2D>();
        body.velocity = direction * Vector3.up;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Sheep")
        {
            Sheep otherSheep = other.gameObject.GetComponent<Sheep>();
            body.velocity = weight * direction * Vector3.up;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Finish")
        {
            if (direction == 1 && other.transform.position.y > 0) // white up
            {
                Debug.Log("Black sheep finished!");
                GameManager.GetInstance().bScore -= point;
                EventManager.GetInstance().PostNotification(EVENT_TYPE.BLACK_FINISH, this, point);
            }
            if (direction == -1 && other.transform.position.y < 0) // white up
            {
                Debug.Log("White sheep finished!");
                GameManager.GetInstance().wScore -= point;
                EventManager.GetInstance().PostNotification(EVENT_TYPE.WHITE_FINISH, this, point);
            }
            GameObject.Destroy(gameObject);
        }
    }
}