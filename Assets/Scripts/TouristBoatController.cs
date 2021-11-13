﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class TouristBoatController : MonoBehaviour
{
    // Components
    Rigidbody2D rb;
    SteeringBasics sb;
    Evade evade;
    WallAvoidance wa;

    // Variables
    [SerializeField] Transform arriveTarget;
    [SerializeField] MovementAIRigidbody evadeTarget;
    [SerializeField] Vector3 evadeAccel, arriveAccel, waAccel, accel;
    [SerializeField] float evadeThreshold;
    [SerializeField] float wallAvoidWeight;


    // Start is called before the first frame update
    void Start()
    {
        sb = GetComponent<SteeringBasics>();
        evade = GetComponent<Evade>();
        wa = GetComponent<WallAvoidance>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        evadeAccel = evade.GetSteering(evadeTarget);
        arriveAccel = sb.Arrive(arriveTarget.position);
        waAccel = wa.GetSteering();

        accel = Vector3.zero;

        /*
        if (wallAvoidAccel.magnitude > 0.005f)
        {
            accel = wallAvoidAccel;
        }
        else if (evadeAccel.magnitude > evadeThreshold)
        {
            accel = evadeAccel;
        }
        else
        {
            accel = arriveAccel;
        }
        */

        accel += waAccel * wallAvoidWeight;

        if (evadeAccel.magnitude > evadeThreshold)
        {
            accel += evadeAccel;
        }
        else
        {
            accel += arriveAccel;
        }

        sb.Steer(accel);
        sb.LookWhereYoureGoing();
    }
}
