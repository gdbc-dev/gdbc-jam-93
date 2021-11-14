using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityMovementAI;
using Random = UnityEngine.Random;

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
    public Image healthBarTransform;
    public Canvas uiCanvas;
    private Camera _camera;

    private void OnEnable()
    {
        _camera = Camera.main;
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
                healthBarTransform.fillAmount = deathTimer / maxDeathTime;

                if (deathTimer > maxDeathTime)
                {
                    isUnwell = true;
                    print("This poor dolphin is now very unwell");
                    audioSource.PlayOneShot(audioUnwell);
                    Destroy(this.gameObject, 0.5f);
                }
            }
            else
            {
                deathTimer -= Time.deltaTime;

                if (deathTimer <= 0)
                {
                    deathTimer = 0;
                }

                healthBarTransform.fillAmount = deathTimer / maxDeathTime;
            }
        }
    }

    private void LateUpdate()
    {
        uiCanvas.transform.LookAt(uiCanvas.transform.position +
                                  _camera.transform.rotation * Vector3.back,
            _camera.transform.rotation * Vector3.down);
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
                    //print(distToArrive);
                    if (distToArrive < 1)
                    {
                        destination = Vector3.zero;
                    }
                }
                else
                {
                    var mapRadius = GameController.instance.getMapSize() / 2;
                    destination = new Vector2(Random.Range(5, mapRadius * 2), Random.Range(5, mapRadius * 2));
                    int attempts = 0;
                    while (attempts < 1000 && !GameController.instance.planningPhaseController.isValidPath(
                               new Vector2Int((int) transform.position.x, (int) transform.position.z),
                               new Vector2Int((int) destination.x, (int) destination.y)))
                    {
                        attempts++;
                        destination = Random.insideUnitCircle * mapRadius;
                        if (attempts > 250)
                        {
                            destination = Vector3.zero;
                        }
                    }

                    destination = new Vector3((int) destination.x, 0, (int) destination.y);
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
//                deathTimer = 0;
            }

            nearbyTourists.Add(other.gameObject);
            var status = other.GetComponent<TouristBoatController>().touristStatus;
            if (status != TouristBoatController.TouristStatus.Retreating)
            {
                TouristBoatController touristBoatController = other.GetComponent<TouristBoatController>();
                touristBoatController.touristStatus = TouristBoatController.TouristStatus.Photographing;
                if (touristBoatController.dialogOnDolphin.Count > 0)
                {
                    touristBoatController.bubbleCanvas.setDialog(touristBoatController.dialogOnDolphin[Random.Range(0, touristBoatController.dialogOnDolphin.Count - 1)], 5f);
                }
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