using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public LineRenderer circleRender;
    // Start is called before the first frame update
    void Start()
    {
        DrawCircle(100,1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawCircle(int steps, float radius)
    {
        circleRender.positionCount = steps;

        for(int cStep = 0; cStep < steps; cStep++)
        {
            float circumferenceProgress = (float)cStep/steps;
            float currentRadians = circumferenceProgress * 2 * Mathf.PI;
            float xScaled = Mathf.Cos(currentRadians);
            float yScaled = Mathf.Sin(currentRadians);

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 currentPositons = new Vector3(x,y,0);

            circleRender.SetPosition(cStep, currentPositons);
        }
    }
}
