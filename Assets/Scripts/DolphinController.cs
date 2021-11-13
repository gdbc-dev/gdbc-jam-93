using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DolphinController : MonoBehaviour
{
    [SerializeField] float deathTimer;
    [SerializeField] float maxDeathTime;

    List<GameObject> nearbyTourists = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (nearbyTourists.Count > 0)
        {
            deathTimer += Time.deltaTime;

            if (deathTimer > maxDeathTime)
            {
                print("This poor dolphin has died");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tourist"))
        {
            if (nearbyTourists.Count == 0)
            {
                print("Dolphin is being photographed!!!");
                deathTimer = 0;
            }
            nearbyTourists.Add(other.gameObject);            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tourist"))
        {
            if (nearbyTourists.Contains(other.gameObject))
            {
                nearbyTourists.Remove(other.gameObject);
            }
        }
    }
}
