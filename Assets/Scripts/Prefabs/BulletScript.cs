using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private float life = 3f; // Lifespan of the bullet before it gets destroyed
    public float damage = 5f; // Base damage of the bullet
    public GameObject shooter; // Reference to the entity that fired the bullet (player or enemy)

    private PlayerHealth playerHealth; // Reference to player health script
    private EnemyHealth enemyHealth;   // Reference to enemy health script

    [SerializeField] Sprite poisonBullet; // Sprite for poison bullet
    [SerializeField] Sprite explosiveBullet; // Sprite for explosive bullet
    [SerializeField] SpriteRenderer bulletSpriteRenderer; // Reference to the bullet sprite renderer

    public ParticleSystem explosionEffectPrefab; // Prefab for explosion effect

    private BulletEffect bulletEffect; // Reference to the current bullet effect

    // Bullet Effects
    private PoisonEffect poisonEffect;
    private ExplosiveEffect explosiveEffect;

    // Layer indices
    private int playerLayer;
    private int enemyLayer;

    private void Awake()
    {
        // Get the layer indices for player and enemy
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");

        // Initialize the effects
        poisonEffect = gameObject.AddComponent<PoisonEffect>();
        explosiveEffect = gameObject.AddComponent<ExplosiveEffect>();
    }

    void Start()
    {
        // Destroy bullet after its lifespan ends
        Destroy(gameObject, life);

        // Check which power-up bullet type to use
        CheckPowerUpBullet();
    }

    private void CheckPowerUpBullet()
    {
        // Use the static instance of PowerUps to access bullet type
        PowerUps powerUps = PowerUps.Instance;

        if (powerUps == null)
        {
            Debug.LogError("PowerUps instance not found in the scene!");
            return; // Exit if no PowerUps component is found
        }

        // Access the current bullet type from the PowerUps script
        powerUps.bullet = (PowerUps.Bullet)PlayerPrefs.GetInt("SelectedBullet", 0);

        // Set the sprite and activate/deactivate effects based on the bullet type
        if (powerUps.bullet == PowerUps.Bullet.Poison)
        {
            bulletSpriteRenderer.sprite = poisonBullet;
            poisonEffect.enabled = true;  // Enable poison effect
            explosiveEffect.enabled = false;  // Disable explosive effect
        }
        else if (powerUps.bullet == PowerUps.Bullet.Explosive)
        {
            bulletSpriteRenderer.sprite = explosiveBullet;
            poisonEffect.enabled = false;  // Disable poison effect
            explosiveEffect.enabled = true;  // Enable explosive effect
        }
        else
        {
            // Standard Bullet
            poisonEffect.enabled = false;
            explosiveEffect.enabled = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (shooter == null)
        {
            HandleCollisionWithoutShooter(collision);
            return;
        }

        if (collision.gameObject.layer == playerLayer && shooter.layer == enemyLayer)
        {
            playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
        else if (collision.gameObject.layer == enemyLayer && shooter.layer == playerLayer)
        {
            enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // Instantiate explosion effect if the bullet is explosive
        if (PowerUps.Instance.bullet == PowerUps.Bullet.Explosive && explosionEffectPrefab != null)
        {
            // Instantiate the explosion effect at the collision point
            ParticleSystem explosion = Instantiate(explosionEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            // Apply explosive effect
            explosiveEffect.ApplyEffect(collision.gameObject, damage);
        }
        else if (PowerUps.Instance.bullet == PowerUps.Bullet.Poison)
        {
            // Apply poison effect if the bullet is poison
            poisonEffect.ApplyEffect(collision.gameObject, damage);
        }

        // Destroy the bullet
        Destroy(gameObject);
    }





    private void HandleCollisionWithoutShooter(Collision2D collision)
    {
        if (collision.gameObject.layer == playerLayer)
        {
            playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                // Apply effect based on bullet type
                ApplyBulletEffect(collision.gameObject);
            }
        }
        else if (collision.gameObject.layer == enemyLayer)
        {
            enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                // Apply effect based on bullet type
                ApplyBulletEffect(collision.gameObject);
            }
        }

        Destroy(gameObject);
    }

    private void ApplyBulletEffect(GameObject target)
    {
        // Apply the appropriate effect based on the current bullet type
        switch (PowerUps.Instance.bullet)
        {
            case PowerUps.Bullet.Poison:
                poisonEffect.ApplyEffect(target, damage);  // Pass base damage to poison effect
                break;
            case PowerUps.Bullet.Explosive:
                explosiveEffect.ApplyEffect(target, damage);  // Pass base damage to explosive effect
                break;
            default:
                break;
        }
    }
}
