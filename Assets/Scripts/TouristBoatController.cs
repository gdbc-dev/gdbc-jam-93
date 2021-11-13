using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class TouristBoatController : MonoBehaviour
{
    // Components
    Rigidbody rb;
    SteeringBasics sb;
    Evade evade;
    WallAvoidance wa;
    Pursue pu;

    // Variables
    [SerializeField] MovementAIRigidbody evadeTarget, puTarget;
    [SerializeField] Vector3 evadeAccel, arriveAccel, waAccel, puAccel, accel;
    [SerializeField] float evadeThreshold;
    [SerializeField] float wallAvoidWeight;
    float distanceCheckFrequency = 1;
    float timeOfNextDistanceCheck;
    private enum TouristStatus { Idle, PursuingDolphin, EvadingPatrol};
    [SerializeField] private TouristStatus touristStatus;

    // Start is called before the first frame update
    void Start()
    {
        sb = GetComponent<SteeringBasics>();
        evade = GetComponent<Evade>();
        wa = GetComponent<WallAvoidance>();
        pu = GetComponent<Pursue>();
    }

    private void OnEnable()
    {
        GameController.instance.addTouristBoat(
            GetComponent<MovementAIRigidbody>());
    }

    private void Update()
    {
        // check if dolphin has been destroyed
        if (!puTarget)
        {
            puTarget = ReturnNearestaIRigidbody(
                GameController.instance.aliveDolphins);
        }
        if (!evadeTarget)
        {
            evadeTarget = ReturnNearestaIRigidbody(
                GameController.instance.patrolBoats);
        }

        if (puTarget == null)
        {
            puTarget = ReturnNearestaIRigidbody(
                GameController.instance.aliveDolphins);
        }
        if (evadeTarget == null)
        {
            evadeTarget = ReturnNearestaIRigidbody(
                GameController.instance.patrolBoats);
        }

        // check if you should be evading a closer patrol boat
        if (evadeTarget != null)
        {
            if (Time.time > timeOfNextDistanceCheck)
            {
                timeOfNextDistanceCheck = Time.time + distanceCheckFrequency;

                var nearestPatrol = ReturnNearestaIRigidbody(
                    GameController.instance.patrolBoats);

                if (evadeTarget != nearestPatrol)
                {
                    evadeTarget = nearestPatrol;
                }
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
        if (evadeTarget != null)
        {
            evadeAccel = evade.GetSteering(evadeTarget);
            if (evadeAccel.magnitude > evadeThreshold)
            {
                accel += evadeAccel;
                touristStatus = TouristStatus.EvadingPatrol;
            }
            else
            {
                if (puTarget != null)
                {
                    puAccel = pu.GetSteering(puTarget);
                    accel += puAccel;
                    touristStatus = TouristStatus.PursuingDolphin;
                }
                else
                {
                    rb.velocity = Vector3.zero;
                    touristStatus = TouristStatus.Idle;
                }
            }
        }
        else
        {
            rb.velocity = Vector3.zero;
            touristStatus = TouristStatus.Idle;
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
