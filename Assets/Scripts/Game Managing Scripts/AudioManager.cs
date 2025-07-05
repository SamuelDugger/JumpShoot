using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance; // Singleton instance
    [SerializeField] AudioSource audioSource; // Reference to the AudioSource component

    private void Awake()
    {
        // Check if an instance already exists
        if (instance == null)
        {
            instance = this; // Set this instance as the singleton instance
            DontDestroyOnLoad(gameObject); // Prevent this object from being destroyed when loading a new scene
            audioSource = GetComponent<AudioSource>(); // Get the AudioSource component

            // Subscribe to the sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // Destroy the duplicate instance
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Destroy this instance if the loaded scene is not "MainScene"
        if (scene.name != "MainScene")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from the sceneLoaded event when the object is destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Method to play music
    public void PlayMusic()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
