using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Particle : MonoBehaviour
{
    public GameObject Sphere;

    private float angle = 0;
    private float rows = 30;
    private float cols = 30;


    public float speed = (float)0.001;
    public float amplitude = (float)2;
    public float frequency = (float)1;

    private List<GameObject> goInt = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        for (int i =0; i<rows; i++)
        {
            for (int j =0; j<cols; j++)
            {
                var pos = new Vector3(j-cols/2,0,i-rows/2);
            //var pos = new Vector2(i - cols / 2, 0);
                var go = Instantiate(Sphere, pos, Quaternion.identity);

                goInt.Add(go);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var go in goInt)
        {
            var d = new Vector3(go.transform.localPosition.x, 0, go.transform.localPosition.z).magnitude;

            var a = (angle + d) * frequency;
            var h = Mathf.Sin(a) * amplitude;

            go.transform.localPosition = new Vector3(go.transform.localPosition.x, h, go.transform.localPosition.z  );
        }

        angle -= speed;

    }
}
