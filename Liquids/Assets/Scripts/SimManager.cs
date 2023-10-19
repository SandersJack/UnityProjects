using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManager : MonoBehaviour
{
    // Start is called before the first frame update
    private LineRenderer lineRenderer;
    private SphereCollider sphereCollider;

    public float gravity = 9.8f;
    Vector3 position;
    Vector3 velocity;

    public float sphereSize = 1;

    public Vector3 boundsSize = new Vector3(10, 10, 10);
    private float lineWidth = 0.1f;

    public float collisionDamping = 0.7f;

    private List<GameObject> spheres = new List<GameObject>();

    void Start()
    {
        createBoundingBox();
        position = new Vector3(0, 5, 0);
        spheres.Add(spawnSphere(position));
    }

    // Update is called once per frame
    void Update()
    {
        velocity += Vector3.down * gravity * Time.deltaTime;
        position += velocity * Time.deltaTime;
        spheres[0].transform.localPosition = position;

        resolveCollisions();
    }

    GameObject spawnSphere(Vector3 pos)
    {
        
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;
        sphereCollider = sphere.GetComponent<SphereCollider>();
        sphereCollider.enabled = false;
        sphere.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);

        return sphere;
    }

    void resolveCollisions()
    {
        Vector3 halfboundsize = boundsSize / 2 - Vector3.one * (sphereSize/2 + lineWidth/2);

        if(Mathf.Abs(position.x) > halfboundsize.x)
        {
            position.x = halfboundsize.x * Mathf.Sign(position.x);
            velocity.x *= -1 * collisionDamping;
        }
        if (Mathf.Abs(position.y) > halfboundsize.y)
        {
            position.y = halfboundsize.y * Mathf.Sign(position.y);
            velocity.y *= -1 * collisionDamping;
        }
    }

    void createBoundingBox()
    {
        float halfx = boundsSize.x / 2;
        float halfy = boundsSize.y / 2;
        float halfz = boundsSize.z / 2;

        lineRenderer = GetComponent<LineRenderer>();
        Vector3[] positions = new Vector3[5] { new Vector3(-halfx, -halfy, 0), new Vector3(-halfx, halfy ,0), new Vector3(halfx, halfy, 0), new Vector3(halfx, -halfy, 0), new Vector3(-halfx, -halfy, 0) };
        lineRenderer.positionCount = 5;
        lineRenderer.SetPositions(positions);
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

}
