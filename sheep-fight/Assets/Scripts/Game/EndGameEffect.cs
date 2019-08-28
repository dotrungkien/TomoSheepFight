using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameEffect : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.RotateAround(transform.position, Vector3.back, 50 * Time.deltaTime);
    }
}
