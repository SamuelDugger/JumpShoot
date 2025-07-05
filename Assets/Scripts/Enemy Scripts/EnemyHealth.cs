using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public float health = 30;
    public float currentHealth;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioSource hurtSound;
    [SerializeField] private Slider slider;

    [SerializeField] GameObject parent;

    public int enemyPoints = 100;
    public string enemyType;  // This should be set correctly by the Spawn Manager

    public SpawnEnemies spawnManager;  // Reference to the SpawnEnemies script

    private const float healthThreshold = 0.001f;  // Small threshold for checking health
    private const float soundInterval = 1f;  // Interval between sound plays in seconds

    private float lastSoundTime;  // Time when the sound was last played

    private void Start()
    {
        currentHealth = health;
        SetupHealth();
        gameManager = GameManager.Instance;
        lastSoundTime = -soundInterval;  // Initialize to allow immediate sound play on first damage

        if (spawnManager == null) 
        {
            Debug.LogError("SpawnEnemies reference is not set!");
        }
    }

    private void SetupHealth()
    {
        slider.maxValue = health;
        slider.value = currentHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        slider.value = currentHealth;
        // Check if health is below or close to zero using a small threshold
        if (currentHealth <= healthThreshold)
        {
            currentHealth = 0;  // Ensure health is exactly zero
            OnDeath();  // Call OnDeath method to handle enemy death
        }
        else
        {
            // Play hurt sound only if enough time has passed since the last sound
            if (Time.time - lastSoundTime >= soundInterval)
            {
                hurtSound.Play();
                lastSoundTime = Time.time;
            }
        }
    }

    private void OnDeath()
    {
        // Call the DecrementEnemyCount method from SpawnEnemies to update enemy count
        if (spawnManager != null)
        {
            spawnManager.DecrementEnemyCount(enemyType);  // Use enemyType instead of parentName
        }
        else
        {
            //Debug.LogError("SpawnManager is not assigned!");
        }

        // Destroy the enemy game object
        Destroy(gameObject);

        //Debug.Log("Enemy Dead");
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.AddScore(enemyPoints);
        }
    }
}
