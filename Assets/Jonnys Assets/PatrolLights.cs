using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolLights : MonoBehaviour
{
    public Color blueColor;
    public Color redColor;
   

    public int defaultSpeedOfRotation = 90;
    public int inPursuitSpeedOfRotation = 180;

    public float defaultIntensity = 60;
    public float inPursuitIntensity = 90;

    public bool inPursuit;

   

    // Update is called once per frame
    void Update()
    {
        if(inPursuit)
        {
            PursuingTourist();
        }
        else
        {
            DefaultPatrol();
        }
    }

    public void DefaultPatrol()
    {
        transform.Rotate(0.0f, defaultSpeedOfRotation * Time.deltaTime, 0.0f, Space.World);
        GetComponent<Light>().color = blueColor;
        GetComponent<Light>().intensity = defaultIntensity;
    }

    public void PursuingTourist()
    {
        transform.Rotate(0.0f, inPursuitSpeedOfRotation * Time.deltaTime, 0.0f, Space.World);
        GetComponent<Light>().intensity = inPursuitIntensity;
        if (transform.rotation.y <= 0.0)
        {
            GetComponent<Light>().color = blueColor;
        }
        else
        {
            GetComponent<Light>().color = redColor;
            //FOR SOME REASON THE RED LIGHT IS FAR LESS NOTICEABLE THAN THE BLUE SO I TRIPLE ITS INTENSITY
            GetComponent<Light>().intensity = inPursuitIntensity * 3;
        }
    }
}
