using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    private bool decay = false;
    private float decayPerInterval;
    private float lastDecay = 0f;
    private float currentDecay = 1f;
    private float changeDecayInterval = 0.25f;
    public void decayOverTime(float time)
    {
        decayPerInterval = changeDecayInterval / time;
        decay = true;
    }
    void Update()
    {
        if (decay)
        {
            if(Time.time - lastDecay > changeDecayInterval)
            {
                LineRenderer lineRenderer = this.GetComponent<LineRenderer>();
                currentDecay -= decayPerInterval;
                if (currentDecay <= 0)
                {
                    currentDecay = 1f;
                    decay = false;
                    lineRenderer.positionCount = 0;
                }
                this.GetComponent<CanvasRenderer>().SetAlpha(currentDecay);
                lastDecay = Time.time;
            }
        }
    }
}
