using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
    public int weight;
    public int point;
    public float vel = 0.5f;
    public int direction = 1;
    public GameObject pushEffect;

    [HideInInspector]
    public int laneIndex;
    [HideInInspector]
    public bool isPushing = false;

    private bool isIncubating;
    private bool isMoving;
    private int collisionCount = 0;
    private Vector2 top;
    private LayerMask sheepMask;
    private float rayLength;
    private Ray ray;
    private RaycastHit HitInfo;

    void Awake()
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

        rayLength = GetComponent<BoxCollider>().size.y * 0.8f / 2f;
        sheepMask = LayerMask.GetMask("Sheep");
    }

    void Update()
    {
        if (!isMoving) return;
        if (!isPushing)
        {
            CheckCollision();
        }
        transform.Translate(Vector3.up * Time.deltaTime * direction * vel);
        Debug.DrawRay(transform.position, Vector3.up * rayLength * direction, Color.yellow);
    }

    void CheckCollision()
    {
        Vector3 origin = transform.position;
        ray = new Ray(origin, Vector3.up * direction * rayLength);
        if (Physics.Raycast(ray, out HitInfo, rayLength))
        {
            GetComponent<Animator>().SetTrigger("push");
            GameObject other = HitInfo.collider.gameObject;
            // print("Collided With " + other.name);
            Sheep otherSheep = other.GetComponent<Sheep>();
            if (direction == 1 && otherSheep.direction == -1)
            {
                GameManager.Instance.wWeights[laneIndex] += weight;
                GameManager.Instance.bWeights[laneIndex] += otherSheep.weight;
                Vector3 pushEffectPos = transform.position + Vector3.up * direction * rayLength;
                var effect = GameObject.Instantiate(pushEffect, pushEffectPos, Quaternion.identity);
                GameObject.Destroy(effect, 0.5f);
            }
            isPushing = true;
            AdjustVelocity();
        }
    }

    void AdjustVelocity()
    {
        direction = GameManager.Instance.LaneDirection(laneIndex);
        vel = 0.3f;
    }

    public void BeSpawned(int _laneIndex)
    {
        isIncubating = false;
        isMoving = true;
        laneIndex = _laneIndex;
        // body.velocity = direction * Vector3.up / 2f;
        var render = GetComponent<SpriteRenderer>();
        var color = render.color;
        color.a = 1f;
        render.color = color;
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
