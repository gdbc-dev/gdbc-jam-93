using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ActionScreenUI : MonoBehaviour
{
    public GameObject[] touristBoatIcons;
    public GameObject[] dolphinIcons;

    public int totalNumberOfTouristBoats;
    public int totalNumberOfDolphins;


    //for use with the time adjustment buttons
    public GameObject pausePlayButton;
    public Sprite pauseIconSprite;
    public Sprite playIconSprite;

    //for use with the countdown clock
    public GameObject countdownTimer;
    public int totalTimeForLevel;
    private float timeRemaining;
    

    // Start is called before the first frame update
    void Start()
    {
        timeRemaining = totalTimeForLevel;
        StartingTouristIcons();
        StartingDolphinsIcons();     
    }

    // Update is called once per frame
    void Update()
    {
        CountDownTimer();
        
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

    public void SlowSpeed()
    {
        Time.timeScale = 0.5f;
    }

    public void PausePlay()
    {
        Debug.Log("clicked");
        Debug.Log(pausePlayButton.GetComponent<Image>().sprite.name);
        if(pausePlayButton.GetComponent<Image>().sprite.name == "pauseIcon")
        {
            Time.timeScale = 0;
            pausePlayButton.GetComponent<Image>().sprite = playIconSprite;
        }
        else if (pausePlayButton.GetComponent<Image>().sprite.name == "playIcon")
        {
            Time.timeScale = 1;
            pausePlayButton.GetComponent<Image>().sprite = pauseIconSprite;
        }

    }


    public void FastForward()
    {
        Time.timeScale = 3;
    }


    public void CountDownTimer()
    {
        timeRemaining -= Time.deltaTime;
        countdownTimer.GetComponent<Text>().text = "Time Remaining: " + timeRemaining;
    }


    public void WhatIsGoingOn()
    {
        Debug.Log("I should see this message");
    }
}
