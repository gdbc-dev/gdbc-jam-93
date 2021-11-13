using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class PlayerBoatsController : MonoBehaviour
{
    // Components
    Rigidbody2D rb;
    SteeringBasics sb;
    FollowPath fp;
    WallAvoidance wa;
    Pursue pu;

    // Variables
    public LinePath path;
    [SerializeField] MovementAIRigidbody pursueTarget;

    // Behaviour Settings
    [SerializeField] bool pathLoop;
    [SerializeField] bool reversePath;


    // Start is called before the first frame update
    void Start()
    {
        sb = GetComponent<SteeringBasics>();
        fp = GetComponent<FollowPath>();
        wa = GetComponent<WallAvoidance>();
        pu = GetComponent<Pursue>();

        path.CalcDistances();
    }


    private void FixedUpdate()
    {
        path.Draw();

        if (reversePath && fp.IsAtEndOfPath(path))
        {
            path.ReversePath();
        }

        var waAccel = wa.GetSteering();

        
        if (waAccel.magnitude > 0.005f)
        {
            sb.Steer(waAccel);
        }
        else if (pursueTarget != null)
        {
            var puAccel = pu.GetSteering(pursueTarget);
            sb.Steer(puAccel);
        }
        else
        {
            Vector3 accel = fp.GetSteering(path, pathLoop);
            sb.Steer(accel);
        }

        sb.LookWhereYoureGoing();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tourist"))
        {
            pursueTarget = other.GetComponent<MovementAIRigidbody>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tourist"))
        {
            var targetRB = other.GetComponent<MovementAIRigidbody>();
            // if it is the target that I am already chasing, then remove it
            if (pursueTarget == targetRB)
            {
                pursueTarget = null;
            }
        }
    }
}
