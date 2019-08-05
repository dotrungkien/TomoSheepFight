using UnityEngine;
using System.Collections;

public class Sheep2 : MonoBehaviour
{
    private int rayCount = 10;
    private Vector3 origin;
    private Vector3 startPoint;
    private RaycastHit HitInfo;
    private float LengthOfRay, DistanceBetweenRays, DirectionFactor;
    private float margin = 0.015f;
    private Ray ray;
    private BoxCollider myCollider;

    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        LengthOfRay = myCollider.size.y / 2f;
        Debug.Log(LengthOfRay);
        DirectionFactor = Mathf.Sign(Vector3.up.y);
    }

    void Update()
    {
        startPoint = new Vector3(myCollider.bounds.min.x + margin, transform.position.y, transform.position.z);
        if (!IsCollidingVertically())
        {
            transform.Translate(Vector3.up * Time.deltaTime * DirectionFactor * 0.5f);
        }
    }

    bool IsCollidingVertically()
    {
        origin = transform.position;
        DistanceBetweenRays = (myCollider.bounds.size.x - 2 * margin) / (rayCount - 1);
        for (int i = 0; i < rayCount; i++)
        {
            ray = new Ray(origin, Vector3.up * DirectionFactor * LengthOfRay);
            Debug.DrawRay(origin, Vector3.up * DirectionFactor * LengthOfRay, Color.yellow);
            if (Physics.Raycast(ray, out HitInfo, LengthOfRay))
            {
                print("Collided With " + HitInfo.collider.gameObject.name);
                DirectionFactor = -DirectionFactor;
                return true;
            }
            origin += new Vector3(DistanceBetweenRays, 0, 0);
        }
        return false;
    }
}