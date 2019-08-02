using UnityEngine;
using System.Collections;

public class Sheep2 : MonoBehaviour
{
    public float MovingForce;
    public int rayCount = 10;
    private Vector3 Origin;
    private Vector3 StartPoint;
    private int i;
    private RaycastHit HitInfo;
    private float LengthOfRay, DistanceBetweenRays, DirectionFactor;
    private float margin = 0.015f;
    private Ray ray;
    private BoxCollider2D collider;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        LengthOfRay = 0.1f;
        //Initialize DirectionFactor for upward direction
        DirectionFactor = Mathf.Sign(Vector3.up.y);
    }
    void FixedUpdate()
    {
        // First ray origin point for this frame
        StartPoint = new Vector3(collider.bounds.min.x + margin, transform.position.y, transform.position.z);
        if (!IsCollidingVertically())
        {
            transform.Translate(Vector3.up * MovingForce * Time.deltaTime * DirectionFactor);
        }
    }

    bool IsCollidingVertically()
    {
        Origin = StartPoint;
        DistanceBetweenRays = (collider.bounds.size.x - 2 * margin) / (rayCount - 1);
        for (i = 0; i < rayCount; i++)
        {
            // Ray to be casted.
            ray = new Ray(Origin, Vector3.up * DirectionFactor);
            //Draw ray on screen to see visually. Remember visual length is not actual length.
            Debug.DrawRay(Origin, Vector3.up * DirectionFactor, Color.yellow);
            if (Physics.Raycast(ray, out HitInfo, LengthOfRay))
            {
                print("Collided With " + HitInfo.collider.gameObject.name);
                // Negate the Directionfactor to reverse the moving direction of colliding cube(here cube2)
                DirectionFactor = -DirectionFactor;
                return true;
            }
            Origin += new Vector3(DistanceBetweenRays, 0, 0);
        }
        return false;
    }
}