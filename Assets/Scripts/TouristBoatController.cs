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

    // Variables
    [SerializeField] MovementAIRigidbody evadeTarget, puTarget;
    [SerializeField] Vector3 evadeAccel, arriveAccel, waAccel, puAccel, accel;
    [SerializeField] float evadeThreshold;
    [SerializeField] float wallAvoidWeight;

    // Start is called before the first frame update
    void Start()
    {
        sb = GetComponent<SteeringBasics>();
        evade = GetComponent<Evade>();
        wa = GetComponent<WallAvoidance>();
        pu = GetComponent<Pursue>();

        //SelectNearestTargetDolphin()
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
        // if not, go for those godamn dolphins
        else
        {
            puAccel = pu.GetSteering(puTarget);
            accel += puAccel;
        }

        sb.Steer(accel);
        sb.LookWhereYoureGoing();
    }

    private MovementAIRigidbody SelectNearestTargetDolphin(
        List<MovementAIRigidbody> _dolphins)
    {
        if (_dolphins.Count == 0)
        {
            return null;
        }

        MovementAIRigidbody nearestTarget = null;
        float minDist = float.PositiveInfinity;

        foreach (MovementAIRigidbody dolphin in _dolphins)
        {
            var thisDist = Vector3.Distance(
                dolphin.transform.position, transform.position);
            if (thisDist < minDist)
            {
                minDist = thisDist;
                nearestTarget = dolphin;
            }
        }
        return nearestTarget;
    }
}
