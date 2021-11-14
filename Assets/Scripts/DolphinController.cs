using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityMovementAI;

public class DolphinController : MonoBehaviour
{
    [SerializeField] float deathTimer;
    [SerializeField] float maxDeathTime;
    public bool isUnwell;

    List<GameObject> nearbyTourists = new List<GameObject>();

    Vector3 destination = Vector3.zero;

    SteeringBasics sb;

    private void OnEnable()
    {
        // add to the dolphins list
        GameController.instance.addDolphin(GetComponent<MovementAIRigidbody>());
        sb = GetComponent<SteeringBasics>();
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
                    Destroy(this.gameObject, 5);
                    // needs to remove itself from the alive dolphins
                    // and then that list needs to inform the tourists
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (nearbyTourists.Count == 0)
        {
            if (destination != Vector3.zero)
            {
                var accel = sb.Arrive(destination);
                sb.Steer(accel);
                sb.LookWhereYoureGoing();
                var distToArrive = sb.ReturnDistanceToArriveTarget(destination);
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tourist") && !isUnwell)
        {
            // if this is the first tourist boat to arrive
            if (nearbyTourists.Count == 0)
            {
                print("Dolphin is being photographed!!!");
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
