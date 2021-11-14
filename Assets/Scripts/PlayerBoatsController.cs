using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class PlayerBoatsController : MonoBehaviour
{
    // Components
    Rigidbody rb;
    SteeringBasics sb;
    FollowPath fp;
    WallAvoidance wa;
    Pursue pu;

    // Variables
    public LinePath path;
    [SerializeField] MovementAIRigidbody pursueTarget;
    TouristBoatController targetTourist;
    Vector3 accel;
    List<MovementAIRigidbody> futureTargets = new List<MovementAIRigidbody>();
    Vector3 centrePointOfPatrol;
    float distanceCheckFrequency = 1;
    float timeOfNextDistanceCheck;
    public PatrolLights patrolLights;
    public BubbleCanvas bubbleCanvas;

    public List<string> onFoundTourist;

    // Behaviour Settings
    [SerializeField] bool pathLoop;
    [SerializeField] bool reversePath;
    [SerializeField] float wallAvoidWeight;

    [SerializeField] float maxPursueDistance; // how far to stray from patrol
    // the max pursue distance should possibly be dynamic based on size of patrol route?

    private void OnEnable()
    {
        // add to the patrol boats list
        GameController.instance.addPatrolBoat(GetComponent<MovementAIRigidbody>());
    }

    // Start is called before the first frame update
    void Start()
    {
        sb = GetComponent<SteeringBasics>();
        fp = GetComponent<FollowPath>();
        wa = GetComponent<WallAvoidance>();
        pu = GetComponent<Pursue>();
        rb = GetComponent<Rigidbody>();

        path.CalcDistances();
        centrePointOfPatrol = ReturnCentrePointOfPatrolRoute(path.nodes);
    }

    private void Update()
    {
        if (!pursueTarget)
        {
            pursueTarget = null;
        }

        patrolLights.inPursuit = pursueTarget != null;

        // check if you're pursuing too far away from the patrol route
        if (pursueTarget != null)
        {
            if (targetTourist != null)
            {
                if (targetTourist.touristStatus ==
                    TouristBoatController.TouristStatus.Retreating)
                {
                    pursueTarget = null;
                    targetTourist = null;
                }
            }

            if (Time.time > timeOfNextDistanceCheck)
            {
                timeOfNextDistanceCheck = Time.time + distanceCheckFrequency;
                if (Vector3.Distance(transform.position, centrePointOfPatrol) >
                    maxPursueDistance)
                {
                    pursueTarget = null;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        accel = Vector3.zero;

        path.Draw();

        if (reversePath && fp.IsAtEndOfPath(path))
        {
            path.ReversePath();
        }

        var waAccel = wa.GetSteering();

        accel += waAccel * wallAvoidWeight;

        if (pursueTarget != null)
        {
            var puAccel = pu.GetSteering(pursueTarget);
            accel += puAccel;
        }
        else
        {
            var arriveAccel = fp.GetSteering(path, pathLoop);
            accel += arriveAccel;
        }

        sb.Steer(accel);
        sb.LookWhereYoureGoing();
    }

    private void OnTriggerEnter(Collider other)
    {
        // a tourist has been spotted!
        if (other.CompareTag("Tourist"))
        {
            if (onFoundTourist.Count > 0)
            {
                bubbleCanvas.setDialog(onFoundTourist[Random.Range(0, onFoundTourist.Count - 1)], 5f);
            }

            var targetAiRb = other.GetComponent<MovementAIRigidbody>();
            // if you don't have a target, pick it up
            if (pursueTarget == null)
            {
                pursueTarget = targetAiRb;
                targetTourist = targetAiRb.GetComponent<TouristBoatController>();
                if (targetTourist.touristStatus ==
                    TouristBoatController.TouristStatus.Retreating)
                {
                    pursueTarget = null;
                    targetTourist = null;
                }
            }
            // if you already do, add it to a list to potentially target in future
            else
            {
                futureTargets.Add(targetAiRb);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // a tourist is no longer in sight...
        if (other.CompareTag("Tourist"))
        {
            var targetAiRb = other.GetComponent<MovementAIRigidbody>();
            // if it is the target that I am chasing, then remove it
            if (pursueTarget == targetAiRb)
            {
                pursueTarget = null;
                targetTourist = null;
                // and check if there are any other targets to go after
                if (futureTargets.Count > 0)
                {
                    pursueTarget = futureTargets[0];
                    targetTourist = pursueTarget.GetComponent<TouristBoatController>();
                    if (targetTourist.touristStatus ==
                        TouristBoatController.TouristStatus.Retreating)
                    {
                        futureTargets.Remove(pursueTarget);
                        pursueTarget = null;
                        targetTourist = null;
                    }
                }
            }

            // if it is one of my potential future targets, remove it from the list
            /*
            else if (futureTargets.Contains(targetAiRb))
            {
                futureTargets.Remove(targetAiRb);
            }
            */
        }
    }

    private Vector3 ReturnCentrePointOfPatrolRoute(Vector3[] _waypointsArray)
    {
        if (_waypointsArray.Length == 0)
        {
            return Vector3.zero;
        }

        var meanVector = Vector3.zero;

        foreach (Vector3 pos in _waypointsArray)
        {
            meanVector += pos;
        }

        return (meanVector / _waypointsArray.Length);
    }
}