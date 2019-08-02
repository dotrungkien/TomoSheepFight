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

    [HideInInspector]
    public int laneIndex;
    [HideInInspector]
    public bool isPushing = false;
    private Rigidbody2D body;

    void Start()
    {
        isMoving = false;
        isIncubating = true;
        body = GetComponent<Rigidbody2D>();
        var render = GetComponent<SpriteRenderer>();
        if (direction == 1)
        {
            var color = render.color;
            color.a = 0.5f;
            render.color = color;
        }
    }

    public void BeSpawned(int _laneIndex)
    {

        isIncubating = false;
        isMoving = true;
        laneIndex = _laneIndex;
        body = GetComponent<Rigidbody2D>();
        body.velocity = direction * Vector3.up / 2f;
        var render = GetComponent<SpriteRenderer>();
        var color = render.color;
        color.a = 1f;
        render.color = color;
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log(other.gameObject.tag);
        if (isPushing) return;
        if (other.gameObject.tag == "Sheep")
        {
            GetComponent<Animator>().SetTrigger("push");
            Sheep otherSheep = other.gameObject.GetComponent<Sheep>();
            GameManager.Instance.wWeights[laneIndex] += weight;
            GameManager.Instance.bWeights[laneIndex] += otherSheep.weight;
            Vector3 pushEffectPos = transform.position + Vector3.up * GetComponent<BoxCollider2D>().size.y / 2f;
            var effect = GameObject.Instantiate(pushEffect, pushEffectPos, Quaternion.identity);
            GameObject.Destroy(effect, 0.5f);
        }
        if (other.gameObject.tag == "BlackTail")
        {
            GameManager.Instance.bWeights[laneIndex] += weight;
        }

        if (other.gameObject.tag == "WhiteTail")
        {
            GameManager.Instance.wWeights[laneIndex] += weight;
        }
        isPushing = true;
        AdjustVelocity();
    }

    void AdjustVelocity()
    {
        body.velocity = GameManager.Instance.LaneVelocity(laneIndex);
        // Debug.Log(string.Format("{2} velocity on lane {0} is {1}", laneIndex, GameManager.Instance.LaneVelocity(laneIndex).y, gameObject.name));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Finish")
        {
            if (isPushing)
            {
                isPushing = false;
                if (direction == 1)
                {
                    GameManager.Instance.wWeights[laneIndex] -= weight;
                }
                else
                {
                    GameManager.Instance.bWeights[laneIndex] -= weight;
                }
            }
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
