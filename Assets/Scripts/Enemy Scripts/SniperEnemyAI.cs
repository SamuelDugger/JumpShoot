using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperEnemyAI : MonoBehaviour
{
    public GameObject bulletPref;
    public float bulletSpeed = 5f;
    public float fireRate = 1f;
    public Transform spawnPoint;  // The gun's barrel or spawn point
    public Transform player;  // Reference to the player
    public float moveSpeed = 2f;  // Speed at which the enemy moves toward the player
    public float jumpForce = 5f;  // Force applied for the enemy to jump
    public float dashForce = 10f; // Force applied for the enemy to dash forward
    public float groundCheckDistance = 0.1f;  // Distance to check if grounded
    public float wallCheckDistance = 0.1f;  // Distance to check if there is an obstacle in front
    public float edgeCheckDistance = 0.5f;  // Distance to check for platform edges
    public LayerMask obstacleLayer;  // Layer mask to detect obstacles
    public LayerMask groundLayer;  // Layer mask to detect the ground

    public float jumpCooldown = 2f;  // Cooldown period for jumping

    public float dashCooldown = 2f;  // Cooldown period for dashing
    private bool canShoot = true;
    private bool canJump = true;
    private bool canDash = true;

    public AudioSource shootSound;

    public AudioSource jumpSound;

    public AudioSource dashSound;
    public GameObject gun;
    private Rigidbody2D rb;

    public PlayerHealth playerHealth;

    public GameObject Parent;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Transform playerTransform, PlayerHealth playerHealths)
    {
        player = playerTransform;
        playerHealth = playerHealths;
    }

    private void Update()
    {
        if (Time.timeScale != 0 && player != null)
        {
            FacePlayer();
            AimAtPlayer();

            // Check if the player is above the enemy and if there is a line of sight
            bool playerAbove = player.position.y > transform.position.y;
            bool playerBelow = player.position.y < transform.position.y;
            bool inLineOfSight = IsPlayerInLineOfSight();

            if (playerAbove && !inLineOfSight)
            {
                if (IsPlatformAbove() && canJump)
                {
                    Jump();
                }
                else
                {
                    MoveTowardPlayer();
                }
            }
            else if (playerBelow && !inLineOfSight)
            {
                if (canDash)
                {
                    DashForward();
                }
            }
            else
            {
                if (inLineOfSight && canShoot)
                {
                    StartCoroutine(Shoot());
                }
                else
                {
                    MoveTowardPlayer();
                }
            }
        }
    }

    private void FacePlayer()
    {
        if (player.position.x < transform.position.x)
        {
            if (transform.localScale.x > 0) // Left
            {
                Turn();
            }
        }
        else
        {
            if (transform.localScale.x < 0) // Right
            {
                Turn();
            }
        }
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        gun.transform.localScale = scale;
    }

    private void AimAtPlayer()
    {
        Vector2 directionToPlayer = player.position - spawnPoint.position;

        if (directionToPlayer.magnitude < 1f)
        {
            Vector2 gunDirection = player.position - gun.transform.position;
            float angle = Mathf.Atan2(gunDirection.y, gunDirection.x) * Mathf.Rad2Deg;

            if (transform.localScale.x < 0) // Facing left
            {
                gun.transform.rotation = Quaternion.Euler(new Vector3(-180, 0, -angle));
            }
            else // Facing right
            {
                gun.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }
        }
        else
        {
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            if (transform.localScale.x < 0) // Facing left
            {
                gun.transform.rotation = Quaternion.Euler(new Vector3(-180, 0, -angle));
            }
            else // Facing right
            {
                gun.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            }
        }
    }


    private IEnumerator Shoot()
    {
        canShoot = false;

        var bullet = Instantiate(bulletPref, spawnPoint.position, spawnPoint.rotation);
        bullet.layer = LayerMask.NameToLayer("EnemyBullet");

        shootSound.Play();

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = spawnPoint.right * bulletSpeed;
        }

        BulletScript bulletScript = bullet.GetComponent<BulletScript>();
        bulletScript.shooter = Parent;

        yield return new WaitForSeconds(fireRate);

        canShoot = true;
    }

    private bool IsPlayerInLineOfSight()
    {
        Vector2 directionToPlayer = player.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, directionToPlayer.magnitude, obstacleLayer);

        // If no obstacle between enemy and player, player is in line of sight
        return hit.collider == null;
    }

    private void MoveTowardPlayer()
    {
        // Only move horizontally
        Vector2 directionToPlayer = (player.position - transform.position).normalized;

        // Set the velocity, preserving the vertical component (y)
        rb.velocity = new Vector2(directionToPlayer.x * moveSpeed, rb.velocity.y);
    }

    private bool IsPlatformAbove()
    {
        // Check if there is a platform directly above the enemy
        Vector2 direction = Vector2.up;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f, groundLayer);  // Adjust the distance as needed
        bool platformAbove = hit.collider != null && hit.collider.transform.position.y > transform.position.y;
        return platformAbove;
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        bool grounded = hit.collider != null;
        return grounded;
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            jumpSound.Play();
            MoveTowardPlayer();
            StartCoroutine(JumpCooldown());
        }
    }

    private IEnumerator JumpCooldown()
    {
        canJump = false;  // Disable jumping
        yield return new WaitForSeconds(jumpCooldown);  // Wait for the cooldown period
        canJump = true;  // Re-enable jumping
    }

    private void DashForward()
    {
        Vector2 dashDirection = Vector2.right * Mathf.Sign(transform.localScale.x);
        dashSound.Play();
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        canDash = false;  // Disable jumping
        yield return new WaitForSeconds(dashCooldown);  // Wait for the cooldown period
        canDash = true;  // Re-enable jumping
    }
}
