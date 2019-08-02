using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
    public int weight;
    public int point;
    public bool isMoving;
    public bool isIncubating;
    public int direction = 1;
    public GameObject pushEffect;

    private Rigidbody2D body;

    void Start()
    {
        isMoving = false;
        isIncubating = true;

        var render = GetComponent<SpriteRenderer>();
        if (direction == 1)
        {
            var color = render.color;
            color.a = 0.5f;
            render.color = color;
        }
    }

    public void BeSpawned()
    {

        isIncubating = false;
        isMoving = true;
        body = GetComponent<Rigidbody2D>();
        body.velocity = direction * Vector3.up / 2f;
        var render = GetComponent<SpriteRenderer>();
        var color = render.color;
        color.a = 1f;
        render.color = color;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Sheep")
        {
            GetComponent<Animator>().SetTrigger("push");
            Sheep otherSheep = other.gameObject.GetComponent<Sheep>();
            if (direction == 1)
            {
                Vector3 pushEffectPos = transform.position + Vector3.up * GetComponent<BoxCollider2D>().size.y / 2f;
                var effect = GameObject.Instantiate(pushEffect, pushEffectPos, Quaternion.identity);
                GameObject.Destroy(effect, 0.5f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Finish")
        {
            if (direction == 1 && other.transform.position.y > 0) // white up
            {
                GameManager.Instance.bScore = (GameManager.Instance.bScore < point) ? 0 : GameManager.Instance.bScore - point;
                GameManager.Instance.PostNotification(EVENT_TYPE.BLACK_FINISH, this, point);
            }
            if (direction == -1 && other.transform.position.y < 0) // white up
            {
                GameManager.Instance.wScore = (GameManager.Instance.wScore < point) ? 0 : GameManager.Instance.wScore - point;
                GameManager.Instance.PostNotification(EVENT_TYPE.WHITE_FINISH, this, point);
            }
            GameObject.Destroy(gameObject);
        }
    }
}
