using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Prefabs")]
    [SerializeField] private GameObject[] platformPrefabs;  // Array of platform types for easier management

    [Header("Spawn Area Settings")]
    [SerializeField] private Transform grid;               // Parent transform to organize spawned platforms
    [SerializeField] private float leftX = -16.5f;
    [SerializeField] private float rightX = 12f;
    [SerializeField] private float topY = 12.5f;
    [SerializeField] private float bottomY = -1f;

    [Header("Spawn Settings")]
    [SerializeField] private int minPlatforms = 5;
    [SerializeField] private int maxPlatforms = 10;
    [SerializeField] private float spawnCheckRadius = 2f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private int maxAttempts = 20;  // Max attempts to find a valid position

    [Header("Platform Pattern Settings")]
    [SerializeField] private bool usePatterns = true;  // Option to use predefined patterns
    [SerializeField] private List<PlatformPattern> patterns;  // List of predefined patterns

    private void Start()
    {
        if (usePatterns && patterns.Count > 0)
        {
            SpawnPlatformsWithPatterns();
        }
        else
        {
            StartCoroutine(SpawnPlatformsRandomly());
        }
    }

    private IEnumerator SpawnPlatformsRandomly()
    {
        int numOfPlatforms = Random.Range(minPlatforms, maxPlatforms);

        for (int i = 0; i < numOfPlatforms; i++)
        {
            bool platformSpawned = false;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 spawnPosition = GenerateRandomPosition();

                if (IsValidSpawnPosition(spawnPosition))
                {
                    SpawnPlatformAtPosition(spawnPosition);
                    platformSpawned = true;
                    break;
                }
            }

            if (!platformSpawned)
            {
                Debug.LogWarning($"Could not find a valid spawn position for platform after {maxAttempts} attempts.");
            }
        }

        yield return null;
    }

    private void SpawnPlatformsWithPatterns()
    {
        PlatformPattern pattern = patterns[Random.Range(0, patterns.Count)];
        foreach (var position in pattern.platformPositions)
        {
            Vector3 spawnPosition = position + new Vector3(Random.Range(leftX, rightX), Random.Range(bottomY, topY), 0);
            if (IsValidSpawnPosition(spawnPosition))
            {
                SpawnPlatformAtPosition(spawnPosition);
            }
        }
    }

    private Vector3 GenerateRandomPosition()
    {
        float spawnX = Random.Range(leftX, rightX);
        float spawnY = Random.Range(bottomY, topY);
        return new Vector3(spawnX, spawnY, 0);
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        return !Physics2D.OverlapCircle(position, spawnCheckRadius, groundLayerMask);
    }

    private void SpawnPlatformAtPosition(Vector3 position)
    {
        GameObject chosenPlatform = platformPrefabs[Random.Range(0, platformPrefabs.Length)];
        GameObject platform = Instantiate(chosenPlatform, position, Quaternion.identity);
        platform.transform.parent = grid;  // Set parent to organize hierarchy in the editor
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3((leftX + rightX) / 2, (topY + bottomY) / 2, 0), new Vector3(rightX - leftX, topY - bottomY, 0));

        // Optional: Draw circles to visualize the radius check (for debugging)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, spawnCheckRadius);
    }
#endif
}

[System.Serializable]
public class PlatformPattern
{
    public List<Vector3> platformPositions;  // List of positions relative to a certain point or grid
}
