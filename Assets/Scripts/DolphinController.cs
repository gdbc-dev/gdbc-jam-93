using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class DolphinController : MonoBehaviour
{
    [SerializeField] float deathTimer;
    [SerializeField] float maxDeathTime;
    public bool isUnwell;

    public AudioClip audioDistressed;
    public AudioClip audioUnwell;
    public AudioSource audioSource;

    List<GameObject> nearbyTourists = new List<GameObject>();

    [SerializeField] Vector3 destination = Vector3.zero;

    SteeringBasics sb;
    Rigidbody rb;

    private void OnEnable()
    {
        // add to the dolphins list
        GameController.instance.addDolphin(GetComponent<MovementAIRigidbody>());
        sb = GetComponent<SteeringBasics>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isUnwell)
        {
            if (nearbyTourists.Count > 0)
            {
                deathTimer += Time.deltaTime;

                if (deathTimer > maxDeathTime)
                {
                    isUnwell = true;
                    print("This poor dolphin is now very unwell");
                    audioSource.PlayOneShot(audioUnwell);
                    Destroy(this.gameObject, 2);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameController.instance.gameState == GameController.GAME_STATE.PLAYING)
        {
            if (nearbyTourists.Count == 0)
            {
                if (destination != Vector3.zero)
                {
                    var accel = sb.Arrive(destination);
                    sb.Steer(accel);
                    sb.LookWhereYoureGoing();
                    var distToArrive = sb.ReturnDistanceToArriveTarget(destination);
                    print(distToArrive);
                    if (distToArrive < 1)
                    {
                        destination = Vector3.zero;
                    }
                }
                else
                {
                    var mapRadius = GameController.instance.getMapSize() / 2;
                    do
                    {
                        destination = Random.insideUnitCircle * mapRadius;
                    }
                    while (GameController.instance.isWater(
                        (int)destination.x, (int)destination.y));
                    destination = new Vector3((int)destination.x, 0, (int)destination.z);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tourist") && !isUnwell)
        {
            // if this is the first tourist boat to arrive
            if (nearbyTourists.Count == 0)
            {
                print("Dolphin is being photographed!!!");
                audioSource.PlayOneShot(audioDistressed);
                rb.velocity = Vector3.zero;
                deathTimer = 0;
            }
            nearbyTourists.Add(other.gameObject);
            var status = other.GetComponent<TouristBoatController>().touristStatus;
            if (status != TouristBoatController.TouristStatus.Retreating)
            {
                other.GetComponent<TouristBoatController>().touristStatus =
                    TouristBoatController.TouristStatus.Photographing;
            }
        }
        else if (other.gameObject.layer == 6)
        {
            destination = Vector3.zero;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tourist") && !isUnwell)
        {
            if (nearbyTourists.Contains(other.gameObject))
            {
                nearbyTourists.Remove(other.gameObject);
                other.GetComponent<TouristBoatController>().touristStatus =
                    TouristBoatController.TouristStatus.PursuingDolphin;
            }
        }
    }

    private void OnDestroy()
    {
        GameController.instance.removeDolphin(
            GetComponent<MovementAIRigidbody>());
    }
}
