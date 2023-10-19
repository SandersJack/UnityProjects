using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimManager : MonoBehaviour
{
    // Start is called before the first frame update
    private LineRenderer lineRenderer;
    private SphereCollider sphereCollider;

    public float gravity = 9.8f;
    Vector3[] position;
    Vector3[] velocity;

    public float sphereSize = 1;
    public int nSpheres = 1;
    public float sphereSpacing = 0.5f;


    public Vector3 boundsSize = new Vector3(10, 10, 10);
    private float lineWidth = 0.1f;

    public float collisionDamping = 0.7f;

    private List<GameObject> spheres = new List<GameObject>();

    void Start()
    {
        foreach (var go in spheres)
        {
            DestroyImmediate(go);
        }
        spheres.Clear();

        createBoundingBox();

        position = new Vector3[Mathf.Abs(nSpheres)];
        velocity = new Vector3[Mathf.Abs(nSpheres)];

        int perRow = (int)Mathf.Sqrt(Mathf.Abs(nSpheres));
        int perCol = (nSpheres - 1)/ Mathf.Abs(nSpheres) + 1;

        float spacing = sphereSize + sphereSpacing;

        for (int i = 0; i < Mathf.Abs(nSpheres); i++)
        {
            float x = (i % perRow - perRow / 2f + 0.5f) * spacing;
            float y = (i / perRow - perCol / 2f + 0.5f) * spacing;
            position[i] = new Vector3(x, y, 0);
            spheres.Add(spawnSphere(position[i]));
        }

    }

    // Update is called once per frame
    void Update()
    {
        createBoundingBox();
        updateSphere();

        for (int i = 0; i < position.Length; i++)
        {
            velocity[i] += Vector3.down * gravity * Time.deltaTime;
            position[i] += velocity[i] * Time.deltaTime;
            resolveCollisions(ref position[i], ref velocity[i]);
            spheres[i].transform.localPosition = position[i];
        }
        

        
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

    void resolveCollisions(ref Vector3 pos, ref Vector3 vel)
    {
        Vector3 halfboundsize = boundsSize / 2 - Vector3.one * (sphereSize/2 + lineWidth/2);

        if(Mathf.Abs(pos.x) > halfboundsize.x)
        {
            pos.x = halfboundsize.x * Mathf.Sign(pos.x);
            vel.x *= -1 * collisionDamping;
        }
        if (Mathf.Abs(pos.y) > halfboundsize.y)
        {
            pos.y = halfboundsize.y * Mathf.Sign(pos.y);
            vel.y *= -1 * collisionDamping;
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

    void updateSphere()
    {
        foreach (var go in spheres)
        {
            go.transform.localScale = new Vector3(sphereSize, sphereSize, sphereSize);
        }
    }

    #if UNITY_EDITOR
        void Awake()
        {
            EditorApplication.update += EditorUpdate;
        }

        void EditorUpdate()
        {
            if ((!EditorApplication.isPlaying) || EditorApplication.isPaused)
            {
                Start();
            }
        }

        void OnDestroy()
        {
            EditorApplication.update -= EditorUpdate;
        }
    #endif


}
