using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{

    [SerializeField] GameObject howScreen;

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenHow()
    {
        howScreen.SetActive(true);
    }

    public void CloseHow()
    {
        howScreen.SetActive(false);
    }
}
