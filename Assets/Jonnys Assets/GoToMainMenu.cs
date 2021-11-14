using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("StartMenu");
    }

    public void DontReturnToMainMenu()
    {
        //However we originally activate this UI when escape is pressed, we should use that logic here to deactivate it
    }
}
