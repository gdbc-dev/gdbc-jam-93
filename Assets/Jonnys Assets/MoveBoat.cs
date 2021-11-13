using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBoat : MonoBehaviour
{
    public int speed;

    public ParticleSystem wavey;
    public ParticleSystem foam1;
    public ParticleSystem foam2;

    

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        transform.Translate(-Vector3.forward * Time.deltaTime * speed, Space.World);
        wavey.startSpeed = speed;
        foam1.startSpeed = speed;
        foam2.startSpeed = speed;

        //the foam needs to be bigger in size as the speed increases so i add its size with speen
        foam1.startSize = 10 + speed;
        foam2.startSize = 10 + speed;

        //the waves need to be bigger in size as the speed increases so i multiply its size with speed
        wavey.startSize = 5 * speed;

        //this ensures that the waves startSize is always at least 30 even if the boat is going slow
        if(wavey.startSize < 30)
        {
            wavey.startSize = 30;
        }
    }


}
