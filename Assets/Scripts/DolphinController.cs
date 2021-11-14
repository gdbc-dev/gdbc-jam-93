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

    Vector3 destination;

    private void OnEnable()
    {
        // add to the dolphins list
        GameController.instance.addDolphin(GetComponent<MovementAIRigidbody>());
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
            if (destination != null)
            {
                // go to the destination
            }
            else
            {
                // pick a random destination within a sphere of possible locations
                // within the bounds of the map
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
