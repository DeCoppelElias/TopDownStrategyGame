using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenuManager : MonoBehaviour
{
    private void Start()
    {
        Invoke("backToMainMenu", 2);
    }

    private void backToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
