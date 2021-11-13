using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public GameObject creditsContainer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //on mouse click, check if the CreditsContainer is active, if so, turn it off
        if (Input.GetMouseButtonDown(0))
        {
            if(creditsContainer.activeSelf)
            {
                creditsContainer.SetActive(false);
            }
        }
    }


    public void OpenUpCredits()
    {
        creditsContainer.SetActive(true);
    }
}
