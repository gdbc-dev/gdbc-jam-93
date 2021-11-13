using System.Collections;
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
    Pursue pu;
    GameController gc;

    // Variables
    [SerializeField] MovementAIRigidbody evadeTarget, puTarget;
    [SerializeField] Vector3 evadeAccel, arriveAccel, waAccel, puAccel, accel;
    [SerializeField] float evadeThreshold;
    [SerializeField] float wallAvoidWeight;
    float distanceCheckFrequency = 1;
    float timeOfNextDistanceCheck;

    // Start is called before the first frame update
    void Start()
    {
        sb = GetComponent<SteeringBasics>();
        evade = GetComponent<Evade>();
        wa = GetComponent<WallAvoidance>();
        pu = GetComponent<Pursue>();
        gc = GameObject.Find("Main Camera").GetComponent<GameController>();

        puTarget = ReturnNearestaIRigidbody(gc.aliveDolphins);
        evadeTarget = ReturnNearestaIRigidbody(gc.patrolBoats);
    }

    private void Update()
    {
        // check if you should be evading a closer patrol boat
        if (Time.time > timeOfNextDistanceCheck)
        {
            timeOfNextDistanceCheck = Time.time + distanceCheckFrequency;

            var nearestPatrol = ReturnNearestaIRigidbody(gc.patrolBoats);

            if (evadeTarget != nearestPatrol)
            {
                evadeTarget = nearestPatrol;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // reset the accel
        accel = Vector3.zero;

        // always avoid walls!
        waAccel = wa.GetSteering();
        accel += waAccel * wallAvoidWeight;

        // check if you should be evading
        evadeAccel = evade.GetSteering(evadeTarget);
        if (evadeAccel.magnitude > evadeThreshold)
        {
            accel += evadeAccel;
        }
        else
        {
            puAccel = pu.GetSteering(puTarget);
            accel += puAccel;
        }

        sb.Steer(accel);
        sb.LookWhereYoureGoing();
    }

    private MovementAIRigidbody ReturnNearestaIRigidbody(
        List<MovementAIRigidbody> aIRigidbodies)
    {
        if (aIRigidbodies.Count == 0)
        {
            return null;
        }

        MovementAIRigidbody nearestTarget = null;
        float minDist = float.PositiveInfinity;

        foreach (MovementAIRigidbody aIRigidbody in aIRigidbodies)
        {
            var thisDist = Vector3.Distance(
                aIRigidbody.transform.position, transform.position);
            if (thisDist < minDist)
            {
                minDist = thisDist;
                nearestTarget = aIRigidbody;
            }
        }
        return nearestTarget;
    }
}
