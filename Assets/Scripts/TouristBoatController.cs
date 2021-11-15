using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;
using Random = UnityEngine.Random;

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
    public FlagBehavior flagBehavior;

    public enum TouristStatus
    {
        Idle,
        PursuingDolphin,
        Retreating,
        Photographing
    };

    public TouristStatus touristStatus;

    public AudioClip audioExcited;
    public AudioClip audioDisappointed;
    public AudioClip audioTakingPictures;
    public AudioSource audioSource;

    [SerializeField] float ticketDistance = 5;
    public BubbleCanvas bubbleCanvas;

    public List<string> dialogOnDolphin;
    public List<string> dialogOnFlee;

    // variables to track if the bastard is stuck
    private float stuckCheckFrequency = 3;
    private float stuckCheckNextCheckTime = 0;
    private float stuckCheckMovementThreshold = 5;
    [SerializeField] private Vector3 stuckCheckOldPos;
    [SerializeField] private float stuckCheckDist;
    [SerializeField] private bool stuckCheckIsUnsticking;
    [SerializeField] private Vector3 stuckCheckUnstickingPos;

    // for the camera flash
    [SerializeField] private float lengthOfCameraFlash = 0.1f;
    [SerializeField] private GameObject cameraFlash;

    private void Awake()
    {
        flagBehavior = GetComponent<FlagBehavior>();
    }

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
                if (!audioSource.isPlaying) {
                    audioSource.PlayOneShot(audioExcited, 1f);
                }
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
                // TODO
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
        if (stuckCheckIsUnsticking)
        {
            var accel = sb.Arrive(stuckCheckUnstickingPos);
            sb.Steer(accel);
            sb.LookWhereYoureGoing();

            var dist = Vector3.Distance(transform.position, stuckCheckUnstickingPos);
            if (dist < 1.5f)
            {
                print("I'm unstuck!");
                stuckCheckIsUnsticking = false;
            }
            return;
        }

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
                // check if you're stuck!
                if (stuckCheckNextCheckTime < Time.time)
                {
                    stuckCheckNextCheckTime = Time.time + stuckCheckFrequency;
                    stuckCheckDist = Vector3.Distance(transform.position, stuckCheckOldPos);
                    if (stuckCheckDist < stuckCheckMovementThreshold)
                    {
                        print("I'm stuck at " + transform.position.x +
                            "," + transform.position.z);

                        int attempts = 0;

                        do
                        {
                            var randomSpot = Random.insideUnitSphere * 10;
                            randomSpot = new Vector3(randomSpot.x, 0, randomSpot.z);
                            stuckCheckUnstickingPos = transform.position + randomSpot;
                            attempts++;
                        } while (attempts < 1000 && !GameController.instance.
                            isWater((int)stuckCheckUnstickingPos.x,
                            (int)stuckCheckUnstickingPos.z));

                        if (attempts == 999)
                        {
                            print("Unstucking failed");
                        }
                        else
                        {
                            stuckCheckIsUnsticking = true;
                        }
                    }
                    stuckCheckOldPos = transform.position;
                }

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
                Vector3 dest = transform.position.normalized;
                dest *= 200;
                var arriveAccel = sb.Arrive(dest);
                accel += arriveAccel;
                sb.Steer(accel);
                sb.LookWhereYoureGoing();
                break;

            default:
                break;
        }

        // Play sounds appropriate to state
        if (!audioSource.isPlaying) {
            switch (touristStatus)
            {
                case TouristStatus.PursuingDolphin:
                    audioSource.PlayOneShot(audioExcited, 1f);
                    break;
                case TouristStatus.Photographing:
                    audioSource.PlayOneShot(audioTakingPictures, 1f);
                    break;
                case TouristStatus.Retreating:
                    audioSource.PlayOneShot(audioDisappointed, 1f);
                    break;
            }
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

        if (distToPatrol < ticketDistance)
        {
            if (dialogOnFlee.Count > 0)
            {
                bubbleCanvas.setDialog(dialogOnFlee[Random.Range(0, dialogOnFlee.Count - 1)], 5f);
            }

            flagBehavior.ticketed = true;

            touristStatus = TouristStatus.Retreating;
            GameController.instance.removeTouristBoat(
                GetComponent<MovementAIRigidbody>());
            //print(this.gameObject.name + ": OK, I'm out of here.");
            Destroy(this.gameObject, 30);
        }
    }

    public IEnumerator CameraFlashingCoro()
    {
        while (touristStatus == TouristStatus.Photographing)
        {
            cameraFlash.SetActive(true);
            yield return new WaitForSeconds(lengthOfCameraFlash);
            cameraFlash.SetActive(false);
            var delay = Random.Range(0.5f, 3);
            yield return new WaitForSeconds(delay);
        }
    }
}
