using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletScript : MonoBehaviour
{
     private float life = 3f;

    public float damage = 5f;
    public GameObject shooter;  

    private PlayerHealth playerHealth;
    private EnemyHealth enemyHealth;

    // Layer indices
    private int playerLayer;
    private int enemyLayer;
    private int playerLayerBullet;
    private int enemyLayerBullet;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        playerLayerBullet = LayerMask.NameToLayer("Player");
        enemyLayerBullet = LayerMask.NameToLayer("Enemy");
    }

    void Start()
    {
        Destroy(gameObject, life);
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
            }
        }
        else if (collision.gameObject.layer == enemyLayer)
        {
            enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}
