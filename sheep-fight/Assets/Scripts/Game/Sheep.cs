using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour, IListener
{
    public int weight;
    public int point;
    public float vel = 0.5f;
    public int direction = 1;
    public bool isWhite = true;
    public GameObject pushEffect;

    [HideInInspector]
    public int laneIndex;
    [HideInInspector]
    public bool isPushing = false;

    private bool isIncubating;
    private bool isMoving;
    private int collisionCount = 0;
    private Vector2 top;
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
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void Update()
    {
        if (!isMoving) return;
        if (!isPushing)
        {
            CheckCollision();
        }
        else
        {
            AdjustVelocity();
        }
        transform.Translate(Vector3.up * Time.deltaTime * direction * vel);
    }

    void CheckCollision()
    {
        Vector3 origin = transform.position;
        ray = new Ray(origin, Vector3.up * direction * rayLength);
        if (Physics.Raycast(ray, out HitInfo, rayLength))
        {
            GetComponent<Animator>().SetTrigger("push");
            GameObject other = HitInfo.collider.gameObject;
            if (other.tag == "Sheep" && !isPushing)
            {
                Sheep otherSheep = other.GetComponent<Sheep>();
                if (isWhite && !otherSheep.isWhite)
                {
                    GameManager.Instance.wWeights[laneIndex] += weight;
                    GameManager.Instance.bWeights[laneIndex] += otherSheep.weight;
                    Vector3 pushEffectPos = transform.position + Vector3.up * direction * rayLength;
                    var effect = GameObject.Instantiate(pushEffect, pushEffectPos, Quaternion.identity);
                    GameObject.Destroy(effect, 0.5f);
                }
                if (isWhite && otherSheep.isWhite) GameManager.Instance.wWeights[laneIndex] += weight;
                if (!isWhite && !otherSheep.isWhite) GameManager.Instance.bWeights[laneIndex] += weight;

                isPushing = true;
            }
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
        var render = GetComponent<SpriteRenderer>();
        var color = render.color;
        color.a = 1f;
        render.color = color;
        GetComponent<BoxCollider>().isTrigger = false;
        GameManager.Instance.AddListener(EVENT_TYPE.GAMEOVER, this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Finish") return;
        if (isWhite)
        {
            if (isPushing) GameManager.Instance.wWeights[laneIndex] -= weight;
            if (other.transform.position.y > 0)
            {
                GameManager.Instance.bScore = (GameManager.Instance.bScore < point) ? 0 : GameManager.Instance.bScore - point;
                if (GameManager.Instance.bScore <= 0) GameManager.Instance.PostNotification(EVENT_TYPE.GAMEOVER, this, true); // white win
                GameManager.Instance.PostNotification(EVENT_TYPE.BLACK_FINISH, this, point);
            }
        }
        else
        {
            if (isPushing) GameManager.Instance.bWeights[laneIndex] -= weight;
            if (other.transform.position.y < 0)
            {
                GameManager.Instance.wScore = (GameManager.Instance.wScore < point) ? 0 : GameManager.Instance.wScore - point;
                if (GameManager.Instance.wScore <= 0) GameManager.Instance.PostNotification(EVENT_TYPE.GAMEOVER, this, false); // white win
                GameManager.Instance.PostNotification(EVENT_TYPE.WHITE_FINISH, this, point);
            }
        }
        GameObject.Destroy(gameObject);
    }

    public void GameOver()
    {
        isMoving = false;
        isPushing = false;
    }

    public void OnEvent(EVENT_TYPE eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EVENT_TYPE.GAMEOVER:
                GameOver();
                break;
            default:
                break;
        }
    }
}
