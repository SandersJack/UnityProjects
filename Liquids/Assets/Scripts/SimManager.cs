using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    Vector3[] positions;
    Vector3[] velocities;
    Vector3[] predictedPositions;
    Vector3[] pressureForces;

    Entry[] spatialLookup;
    int[] startIndicies;

    private int[,] cellOffsets = new int[9,2]{ { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 0 }, { 0, 1 }
        ,{ 1, -1 }, { 1, 0 }, { 1, 1 } };

    public float sphereSize = 1;
    public int nSpheres = 1;
    public float sphereSpacing = 0.5f;
    private const float mass = 1;

    public Vector3 boundsSize = new Vector3(10, 10, 10);
    private float lineWidth = 0.1f;

    public float collisionDamping = 0.7f;

    public float smoothingRadius = 1.5f;



    float[] densities;

    private List<GameObject> spheres = new List<GameObject>();

    void Start()
    {
        foreach (var go in spheres)
        {
            DestroyImmediate(go);
        }
        spheres.Clear();

        createBoundingBox();

        positions = new Vector3[Mathf.Abs(nSpheres)];
        velocities = new Vector3[Mathf.Abs(nSpheres)];
        predictedPositions = new Vector3[Mathf.Abs(nSpheres)];
        pressureForces = new Vector3[Mathf.Abs(nSpheres)];

        spatialLookup = new Entry[Mathf.Abs(nSpheres)];
        startIndicies = new int[Mathf.Abs(nSpheres)];

        densities = new float[Mathf.Abs(nSpheres)];

        int perRow = (int)Mathf.Sqrt(Mathf.Abs(nSpheres));
        int perCol = (nSpheres - 1)/ Mathf.Abs(nSpheres) + 1;

        float spacing = sphereSize + sphereSpacing;

        for (int i = 0; i < Mathf.Abs(nSpheres); i++)
        {
            float x = (i % perRow - perRow / 2f + 0.5f) * spacing;
            float y = (i / perRow - perCol / 2f + 0.5f) * spacing - boundsSize.y/2;
            positions[i] = new Vector3(x, y, 0);
            spheres.Add(spawnSphere(positions[i]));
        }



    }

    // Update is called once per frame
    void Update()
    {
        createBoundingBox();
        updateSphere();

        for (int i = 0; i < positions.Length; i++)
        {
            //velocities[i] += Vector3.down * gravity * Time.deltaTime;
            //positions[i] += velocities[i] * Time.deltaTime;
            //resolveCollisions(ref positions[i], ref velocities[i]);
            //spheres[i].transform.localPosition = positions[i];
            SImulationStep(Time.deltaTime);
            spheres[i].transform.localPosition = positions[i];
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

    static float SmoothingFunc(float dist, float radius)
    {
        if (dist >= radius) return 0;

        float volume = Mathf.PI * Mathf.Pow(radius, 4)/6;
        return (radius - dist) * (radius - dist) * volume; 
    }

    static float SmoothingFuncDeriv(float dist, float radius)
    {
        if(dist >= radius) return 0;
        float scale = 12 / (Mathf.PI * Mathf.Pow(radius, 4));
        return (dist - radius) * scale;
    }

    float CalculateDensity(Vector3 samplePoint)
    {
        float density = 0;
        

        foreach (Vector3 pos in positions)
        {
            float dist = (pos - samplePoint).magnitude;
            float influence = SmoothingFunc(dist, smoothingRadius);

            density += mass * influence;
        }

        return density;
    }

    void SImulationStep(float deltaTime)
    {
        Parallel.For(0, nSpheres, i =>
        {
            velocities[i] += Vector3.down * gravity * deltaTime;
            predictedPositions[i] = positions[i] + velocities[i] * deltaTime;
        });

        UpdateSpatialLookup(predictedPositions, smoothingRadius);

        Parallel.For(0, nSpheres, i =>
        {
            densities[i] = CalculateDensity(predictedPositions[i]);
        });

        Parallel.For(0, nSpheres, i =>
        {
            Vector3 pressureForce = CalculatePressureForce(i);
            Vector3 pressureAcceleration = pressureForce / densities[i];
            velocities[i] += pressureAcceleration * deltaTime;
        });
        Parallel.For(0, nSpheres, i =>
        {
            positions[i] += velocities[i] * deltaTime;
            resolveCollisions(ref positions[i], ref velocities[i]);
        });
    }
    public float targetDensity = 2.75f;
    public float pressureMutiplier = 0.5f;

    float ConvertDensityToPressure(float density)
    {
        float densityError = density - targetDensity;
        float pressure = densityError * pressureMutiplier;
        return pressure;
    }

    Vector3 GetRandomDir()
    {
        Vector3 min = new Vector3(0, 0, 0);
        Vector3 max = new Vector3(1, 1, 1);
        return max;
    }

    float CalculateSharedPressure(float densityA, float densityB)
    {
        float pressureA = ConvertDensityToPressure(densityA);
        float pressureB = ConvertDensityToPressure(densityB);
        return (pressureA + pressureB) / 2;
    }

    Vector3 CalculatePressureForce(int particleIndex)
    {
        /*Vector3 pressureForce = Vector3.zero;

        for(int otherParticleIndex = 0; otherParticleIndex < nSpheres; otherParticleIndex++)
        {
            if (particleIndex == otherParticleIndex) continue;
            Vector3 offset = positions[otherParticleIndex] - positions[particleIndex];
            float dist = offset.magnitude;
            Vector3 dir = dist == 0 ? GetRandomDir() : offset / dist;
            float slope = SmoothingFunc(dist, smoothingRadius);
            float density = densities[otherParticleIndex];
            float sharedPressure = CalculateSharedPressure(density, densities[particleIndex]);
            pressureForce += -sharedPressure * dir * slope * mass / density;
        }
        pressureForce.z = 0;
        return pressureForce;*/

        return ForeachPointWithinRadius(positions, particleIndex);
    }

    (int x, int y, int z) PositionToCellCoord(Vector3 point, float radius)
    {
        int cellX = (int)(point.x / radius);
        int cellY = (int)(point.y / radius);    
        int cellZ = (int)(point.z / radius);

        return (cellX, cellY, cellZ);
    }

    uint HashCell(int cellX, int cellY)
    {
        uint a = (uint)cellX * 15823;
        uint b = (uint)cellY * 9737333;
        return a + b;
    } 

    uint GetKeyFromHash(uint hash)
    {
        return hash % (uint)spatialLookup.Length;
    }
    void UpdateSpatialLookup(Vector3[] points, float radius)
    {
        Parallel.For(0, points.Length, i =>
        {
            (int cellX, int cellY, int cellZ) = PositionToCellCoord(points[i], radius);
            uint cellKey = GetKeyFromHash(HashCell(cellX, cellY));
            spatialLookup[i] = new Entry(i, cellKey);
            startIndicies[i] = int.MaxValue;
        });

        Array.Sort(spatialLookup);

        Parallel.For(0, points.Length, i =>
        {
            uint key = spatialLookup[i].cellKey;
            uint keyPrev = i == 0 ? uint.MaxValue : spatialLookup[i - 1].cellKey;
            if (key != keyPrev)
            {
                startIndicies[i] = i;
            }
        });

    }

    Vector3 ForeachPointWithinRadius(Vector3[] points, int sampleIndex)
    {
        float radius = smoothingRadius;
        (int centerX, int centerY, int centerZ) = PositionToCellCoord(points[sampleIndex], radius);
        float sqrRad = radius * radius;

        Vector3 pressureForce = Vector3.zero;

        for (int t=0; t< cellOffsets.GetLength(0); t++)
        {
            //Debug.Log(t + " " + cellOffsets[t, 0]);
            uint key = GetKeyFromHash(HashCell(centerX + cellOffsets[t,0], centerY + cellOffsets[t, 1]));
            int cellStartIndex = startIndicies[key];

            for (int i = cellStartIndex; i < spatialLookup.Length; i++)
            {
                if (spatialLookup[i].cellKey != key) break;

                int particleIndex = spatialLookup[i].particleIndex;
                float sqrDist = (points[particleIndex] - points[sampleIndex]).sqrMagnitude;

                if( sqrDist <= radius)
                {
                    //Do Somthing
                    if (sampleIndex == particleIndex) continue;
                    Vector3 offset = points[particleIndex] - points[sampleIndex];
                    float dist = offset.magnitude;
                    Vector3 dir = dist == 0 ? GetRandomDir() : offset / dist;
                    float slope = SmoothingFunc(dist, smoothingRadius);
                    float density = densities[particleIndex];
                    float sharedPressure = CalculateSharedPressure(density, densities[particleIndex]);
                    pressureForce += -sharedPressure * dir * slope * mass / density;
                }
            }
        }
        pressureForce.z = 0;
        return pressureForce;
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

public class Entry : IComparable<Entry>
{
    public int particleIndex;
    public uint cellKey; 

    public Entry(int pI, uint cK)
    {
        particleIndex = pI;
        cellKey = cK;
    }

    public int CompareTo(Entry objB)
    {
        return cellKey.CompareTo(objB.cellKey);
    }
}
