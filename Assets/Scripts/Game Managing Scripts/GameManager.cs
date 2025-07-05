using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    public SettingsScript settingsScript;
    public static GameManager Instance;
    public GameObject gameOver;
    public GameObject PowerUpMenu;

    private bool gameActive = true;
    public int currentScore;

    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI gameCurrentScoreText;

    [SerializeField] GameObject pauseScreen;

    [SerializeField] GameObject arm;

    private bool gameOverBool = false;

    private float timer = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Cursor.visible = true;
        string highScore = PlayerPrefs.GetInt("HighScore", 0).ToString();
        highScoreText.text = "High Score: " + highScore;
        settingsScript.LoadVolume();
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        currentScoreText.text = "Current Score: " + currentScore.ToString();
        gameCurrentScoreText.text = currentScore.ToString();

        if (currentScore > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            highScoreText.text = "High Score: " + currentScore.ToString();
        }
    }

    private void Update()
    {
        if (playerHealth.currentHealth <= 0 && gameActive)
        {
            GameOver();
            gameActive = false;
        }
        if (!gameOverBool)
        {
            if (Input.GetKeyDown("escape") && Time.timeScale == 1)
            {
                Pause();
            }
            else if (Input.GetKeyDown("escape") && Time.timeScale == 0)
            {
                Unpause();
            }
        }
    }

    public void GameOver()
    {
        gameOver.SetActive(true);
        gameOverBool = true;
        Time.timeScale = 0.5f;
        StartCoroutine(WaitTimer());
    }

    public void Pause()
    {
        pauseScreen.SetActive(true);
        Time.timeScale = 0f;
        arm.SetActive(false);
    }

    public void Unpause()
    {
        pauseScreen.SetActive(false);
        Time.timeScale = 1f;
        arm.SetActive(true);
    }

    private IEnumerator WaitTimer()
    {
        yield return new WaitForSeconds(timer);
        Time.timeScale = 0;
    }

    public void ClickPowerBtn()
    {
        PowerUpMenu.SetActive(true);
    }

    public void ClosePower()
    {
        PowerUpMenu.SetActive(false);
    }

    public void ClickRestartBtn()
    {
        SceneManager.LoadScene("MainScene");
        Time.timeScale = 1;
    }

    public void ClickMainBtn()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1;
        Destroy(gameObject);
    }

    public void ClickExitBtn()
    {
        Application.Quit();
    }
}
