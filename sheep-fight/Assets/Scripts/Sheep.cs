using System.Collections;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log(string.Format("on collision {0}", other.gameObject.name));
    }
}
