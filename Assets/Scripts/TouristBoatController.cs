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
    public enum TouristStatus { Idle, PursuingDolphin, Retreating, Photographing};
    public TouristStatus touristStatus;
    [SerializeField] float ticketDistance = 5;

    // Start is called before the first frame update
    void Start()
    {
        sb = GetComponent<SteeringBasics>();
        evade = GetComponent<Evade>();
        wa = GetComponent<WallAvoidance>();
        pu = GetComponent<Pursue>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        GameController.instance.addTouristBoat(
            GetComponent<MovementAIRigidbody>());
    }

    private void Update()
    {
        switch (touristStatus)
        {
            case TouristStatus.Idle:
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
                if (evadeTarget != null && puTarget != null)
                {
                    touristStatus = TouristStatus.PursuingDolphin;
                }
                break;

            case TouristStatus.PursuingDolphin:
                // check if dolphin or patrol has been destroyed or is null
                if (!puTarget)
                {
                    puTarget = null;
                }
                if (!evadeTarget)
                {
                    evadeTarget = null;
                }
                if (puTarget == null)
                {
                    touristStatus = TouristStatus.Idle;
                }
                if (evadeTarget == null)
                {
                    touristStatus = TouristStatus.Idle;
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

                    CheckIfTouristReceivedTicket();
                }
                break;

            case TouristStatus.Retreating:
                break;

            case TouristStatus.Photographing:
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

                    CheckIfTouristReceivedTicket();
                }
                // check if dolphin is dead!
                if (!puTarget)
                {
                    touristStatus = TouristStatus.PursuingDolphin;
                }
                break;

            default:
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // reset the accel
        accel = Vector3.zero;

        // avoid walls, unless in range of the dolphin!
        if (touristStatus != TouristStatus.Photographing)
        {
            //print("I am checking for a wall!");
            waAccel = wa.GetSteering();
            accel += waAccel * wallAvoidWeight;
        }

        switch (touristStatus)
        {
            case TouristStatus.Idle:
                break;

            case TouristStatus.PursuingDolphin:
                // check if you should be evading
                if (evadeTarget != null)
                {
                    evadeAccel = evade.GetSteering(evadeTarget);
                    if (evadeAccel.magnitude > evadeThreshold)
                    {
                        accel += evadeAccel;
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
                break;

            case TouristStatus.Photographing:
                if (evadeTarget != null)
                {
                    evadeAccel = evade.GetSteering(evadeTarget);
                    if (evadeAccel.magnitude > evadeThreshold)
                    {
                        accel += evadeAccel;
                    }
                }
                sb.Steer(accel);
                sb.LookWhereYoureGoing();
                break;

            case TouristStatus.Retreating:
                var arriveAccel = sb.Arrive(new Vector3 (200, 0, 200));
                accel += arriveAccel;
                sb.Steer(accel);
                sb.LookWhereYoureGoing();
                break;

            default:
                break;
        }
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

    private void CheckIfTouristReceivedTicket()
    {
        // if within range of a patrol boat, receive a ticket
        var distToPatrol = Vector3.Distance(transform.position,
            evadeTarget.transform.position);
        //print("Checking distance " + distToPatrol);
        if (distToPatrol < ticketDistance)
        {
            touristStatus = TouristStatus.Retreating;
            GameController.instance.removeTouristBoat(
                GetComponent<MovementAIRigidbody>());
            //this.gameObject.tag = null;
            print(this.gameObject.name + ": OK, I'm out of here.");
        }
    }
}
