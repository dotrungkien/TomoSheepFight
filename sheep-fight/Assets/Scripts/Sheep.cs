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
    private float offset;
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

        offset = GetComponent<BoxCollider>().size.y / 2f;
        sheepMask = LayerMask.GetMask("Sheep");
    }

    void Update()
    {
        if (!isMoving) return;
        if (!IsCollidingVertically())
        {
            transform.Translate(Vector3.up * Time.deltaTime * direction * vel);
        }
        Debug.DrawRay(transform.position, Vector3.up * offset * direction, Color.yellow);
    }

    bool IsCollidingVertically()
    {
        Vector3 origin = transform.position;
        ray = new Ray(origin, Vector3.up * direction * offset);
        Debug.DrawRay(origin, Vector3.up * direction * offset, Color.yellow);
        if (Physics.Raycast(ray, out HitInfo, offset))
        {
            // print("Collided With " + HitInfo.collider.gameObject.name);
            // direction = -direction;
            return true;
        }
        return false;
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
