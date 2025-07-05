using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    [SerializeField] float leftX = -12f;
    [SerializeField] float rightX = 12f;
    [SerializeField] float topY = 4f;
    [SerializeField] float bottomY = -5.5f;

    [SerializeField] GameObject pistolEnemy;
    [SerializeField] GameObject shotgunEnemy;
    [SerializeField] GameObject smgEnemy;
    [SerializeField] GameObject sniperEnemy;

    [SerializeField] GameObject sparkleEffectPrefab;  // Particle effect prefab

    [SerializeField] float spawnTimer = 5f;
    [SerializeField] float spawnDelay = 1f;  // Delay between sparkle and actual spawn

    [SerializeField] float timeBetweenSpawn = 5f; 

    // Max number of enemies 
    public int maxCurrentNumOfEnemies = 15;

    // Max number of each type of enemy
    [SerializeField] int maxPistolEnemies = 8;
    [SerializeField] int maxShotgunEnemies = 4;
    [SerializeField] int maxSMGEnemies = 3;
    [SerializeField] int maxSniperEnemies = 2;

    [SerializeField] Transform playerTransform;
    [SerializeField] PlayerHealth playerHealth;

    [SerializeField] float spawnCheckRadius = 1f;
    [SerializeField] LayerMask groundLayerMask;

    // Track the current number of each enemy type
    public int currentPistolEnemies = 0;
    public int currentShotgunEnemies = 0;
    public int currentSMGEnemies = 0;
    public int currentSniperEnemies = 0;

    private void Start()
    {
        StartCoroutine(SpawnEnemiesCoroutine());
    }

    IEnumerator SpawnEnemiesCoroutine()
    {
        while (playerHealth.currentHealth > 0)
        {
            int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

            // Check if current number of enemies is less than max
            if (currentEnemyCount < maxCurrentNumOfEnemies)
            {
                yield return StartCoroutine(SpawnEnemyWithEffect());
            }
            else
            {
                // Wait for some time and check again
                yield return new WaitForSeconds(spawnTimer);
            }
        }
    }

    IEnumerator SpawnEnemyWithEffect()
    {
        Vector3 spawnPosition;
        int maxAttempts = 20;  // Maximum attempts to find a valid spawn position

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float spawnX = Random.Range(leftX, rightX);
            float spawnY = Random.Range(bottomY, topY);
            spawnPosition = new Vector3(spawnX, spawnY, 0);

            if (!Physics2D.OverlapCircle(spawnPosition, spawnCheckRadius, groundLayerMask))
            {
                if (sparkleEffectPrefab != null)
                {
                    GameObject sparkleEffect = Instantiate(sparkleEffectPrefab, spawnPosition, Quaternion.identity);
                    ParticleSystem ps = sparkleEffect.GetComponent<ParticleSystem>();
                    AudioSource soundEffect = sparkleEffect.GetComponent<AudioSource>();
                    if (ps != null)
                    {
                        ps.Play();
                        soundEffect.Play();
                    }

                    yield return new WaitForSeconds(spawnDelay);

                    Destroy(sparkleEffect);
                }

                List<GameObject> enemyTypes = new List<GameObject>();
                if (currentPistolEnemies < maxPistolEnemies) enemyTypes.Add(pistolEnemy);
                if (currentShotgunEnemies < maxShotgunEnemies) enemyTypes.Add(shotgunEnemy);
                if (currentSMGEnemies < maxSMGEnemies) enemyTypes.Add(smgEnemy);
                if (currentSniperEnemies < maxSniperEnemies) enemyTypes.Add(sniperEnemy);

                if (enemyTypes.Count > 0)
                {
                    GameObject chosenEnemy = enemyTypes[Random.Range(0, enemyTypes.Count)];
                    GameObject enemy = Instantiate(chosenEnemy, spawnPosition, Quaternion.identity);

                    // Assign the type for counting
                    EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (chosenEnemy == pistolEnemy)
                    {
                        currentPistolEnemies++;
                        enemyHealth.enemyType = "Pistol";
                    }
                    else if (chosenEnemy == shotgunEnemy)
                    {
                        currentShotgunEnemies++;
                        enemyHealth.enemyType = "Shotgun";
                    }
                    else if (chosenEnemy == smgEnemy)
                    {
                        currentSMGEnemies++;
                        enemyHealth.enemyType = "SMG";
                    }
                    else if (chosenEnemy == sniperEnemy)
                    {
                        currentSniperEnemies++;
                        enemyHealth.enemyType = "Sniper";
                    }

                    enemyHealth.spawnManager = this;

                    // Initialize enemy AI
                    EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                    if (enemyAI != null)
                    {
                        enemyAI.Initialize(playerTransform, playerHealth);
                    }

                    yield return new WaitForSeconds(timeBetweenSpawn);  // Exit after successfully spawning the enemy
                    break;
                }
            }
        }
    }

    // Method to reduce enemy counters when they are destroyed (to be called from enemy scripts)
    public void DecrementEnemyCount(string enemyType)
    {
        switch (enemyType)
        {
            case "Pistol":
                currentPistolEnemies--;
                break;
            case "Shotgun":
                currentShotgunEnemies--;
                break;
            case "SMG":
                currentSMGEnemies--;
                break;
            case "Sniper":
                currentSniperEnemies--;
                break;
        }
    }
}
