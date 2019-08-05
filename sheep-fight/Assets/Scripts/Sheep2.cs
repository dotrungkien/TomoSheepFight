using UnityEngine;
using System.Collections;

public class Sheep2 : MonoBehaviour
{

    public int weight;
    public int point;
    public float vel = 0.5f;
    private Ray ray;
    private RaycastHit HitInfo;
    private Vector3 origin;
    private BoxCollider myCollider;
    private int direction = 1;
    private float rayLength;

    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        rayLength = myCollider.size.y / 2f;
        Debug.Log(rayLength);
    }

    void Update()
    {
        if (!IsCollidingVertically())
        {
            transform.Translate(Vector3.up * Time.deltaTime * direction * vel);
        }
    }

    bool IsCollidingVertically()
    {
        origin = transform.position;
        ray = new Ray(origin, Vector3.up * direction * rayLength);
        Debug.DrawRay(origin, Vector3.up * direction * rayLength, Color.yellow);
        if (Physics.Raycast(ray, out HitInfo, rayLength))
        {
            print("Collided With " + HitInfo.collider.gameObject.name);
            direction = -direction;
            return true;
        }
        return false;
    }
}