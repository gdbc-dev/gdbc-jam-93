using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagBehavior : MonoBehaviour
{
    public GameObject flag;
    public bool ticketed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f, 45 * Time.deltaTime, 0.0f, Space.World);
        if (ticketed)
        {
            flag.SetActive(true);
        }
        else
        {
            flag.SetActive(false);
        }
    }
}
