using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActionScreenUI : MonoBehaviour
{
    public GameObject[] touristBoatIcons;
    public GameObject[] dolphinIcons;

    public int totalNumberOfTouristBoats;
    public int totalNumberOfDolphins;

    // Start is called before the first frame update
    void Start()
    {
        StartingTouristIcons();
        StartingDolphinsIcons();     
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void StartingTouristIcons()
    {
        for (int i = 0; i < totalNumberOfTouristBoats; i++)
        {
            touristBoatIcons[i].SetActive(true);
        }
    }

    void StartingDolphinsIcons()
    {
        for (int i = 0; i < totalNumberOfDolphins; i++)
        {
            dolphinIcons[i].SetActive(true);
        }
    }


    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
