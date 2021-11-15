using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ActionScreenUI : MonoBehaviour
{
    public GameObject[] touristBoatIcons;
    public GameObject[] dolphinIcons;

    public int totalNumberOfTouristBoats;
    public int totalNumberOfDolphins;


    //for use with the time adjustment buttons
    public GameObject pauseButton;
    public GameObject playButton;
    public GameObject fastForwardButton;

    public Sprite pauseIconSprite;
    public Sprite playIconSprite;
    public Color selectedColor;
    public Color defaultColor;

    //for use with the countdown clock
    public GameObject countdownTimer;
    public TMP_Text countDownTimerTextObject;

    public int totalTimeForLevel;
    [NonSerialized] public float timeRemaining;

    


    // Start is called before the first frame update
    void Start()
    {
//        timeRemaining = totalTimeForLevel;
//        StartingTouristIcons(3);
//        StartingDolphinsIcons(3);
    }

    // Update is called once per frame
    void Update()
    {
        CountDownTimer();
    }


    public void StartingTouristIcons(int newAmount)
    {
        totalNumberOfTouristBoats = newAmount;
        for (int i = 0; i < touristBoatIcons.Length; i++)
        {
            touristBoatIcons[i].SetActive(i < totalNumberOfTouristBoats);
        }
    }

    public void StartingDolphinsIcons(int newAmount)
    {
        totalNumberOfDolphins = newAmount;
        for (int i = 0; i < dolphinIcons.Length; i++)
        {
            dolphinIcons[i].SetActive(i < totalNumberOfDolphins);
        }
    }


    public void Retry()
    {
        GameController.instance.restartLevel();
    }

    public void Pause()
    {
       Time.timeScale = 0;
        pauseButton.GetComponent<Image>().color = selectedColor;
        playButton.GetComponent<Image>().color = defaultColor;
        fastForwardButton.GetComponent<Image>().color = defaultColor; 
    }

   

    public void Play()
    {
        Time.timeScale = 1;
        playButton.GetComponent<Image>().color = selectedColor;
        pauseButton.GetComponent<Image>().color = defaultColor;
        fastForwardButton.GetComponent<Image>().color = defaultColor;
    }


    public void FastForward()
    {
        Time.timeScale = 5;
        fastForwardButton.GetComponent<Image>().color = selectedColor;
        playButton.GetComponent<Image>().color = defaultColor;
        pauseButton.GetComponent<Image>().color = defaultColor;
    }


    public void CountDownTimer()
    {
        timeRemaining -= Time.deltaTime;

        countDownTimerTextObject.text = "Time Remaining: " + timeRemaining.ToString("F2");
    }


    public void WhatIsGoingOn()
    {
        Debug.Log("I should see this message");
    }
}