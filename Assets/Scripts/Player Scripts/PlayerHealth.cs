using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float collisionDamageAmount = 5f;
    public float knockbackForce = 10f;
    public float damageCooldown = 1f;

    [SerializeField] Transform playerTransform;

    private bool canTakeCollisionDamage = true;
    private bool isKnockbackActive = false;
    [SerializeField] Rigidbody2D rb;
    private Coroutine damageCooldownCoroutine;

    [SerializeField] AudioSource hurtSound;

    private float originalMaxHealth;

    [SerializeField] CapsuleCollider2D playerCollider;

    [SerializeField] PlayerMovement playerMovement;

    [SerializeField] GameObject playerArm;


    // UI Stuff

    [SerializeField] Slider healthSlider;

    [SerializeField] TextMeshProUGUI healthText;

    void Start()
    {
        originalMaxHealth = maxHealth;
        currentHealth = maxHealth;

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing from player!");
        }
    }

    public void UpdateHealthUI()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        healthText.text = currentHealth.ToString();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        UpdateHealthUI();
        hurtSound.Play();
        if (currentHealth <= 0)
        {
            //Destroy(gameObject);
            playerCollider.enabled = false;
            playerMovement.enabled = false;
            playerArm.SetActive(false);
            // Debug.Log("Player Destroyed");
        }
    }

    private void TakeCollisionDamage(float damageAmount, Transform enemyTransform)
    {
        if (canTakeCollisionDamage)
        {
            currentHealth -= damageAmount;
            UpdateHealthUI();
            hurtSound.Play();
            //Debug.Log("Collision Damage Taken: " + damageAmount + " | Current Health: " + currentHealth);

            if (currentHealth <= 0)
            {
                playerCollider.enabled = false;
                playerMovement.enabled = false;
                playerArm.SetActive(false);
                //Debug.Log("Player Destroyed");
                return;
            }

            Knockback(enemyTransform);

            if (damageCooldownCoroutine != null)
            {
                StopCoroutine(damageCooldownCoroutine);
            }
            damageCooldownCoroutine = StartCoroutine(CollisionDamageCooldown());
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && canTakeCollisionDamage)
        {
            TakeCollisionDamage(collisionDamageAmount, other.transform);
        }
    }

    private void Knockback(Transform enemyTransform)
    {
        isKnockbackActive = true;
        Vector3 knockbackDirection = (playerTransform.position - enemyTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        // Debug.Log("Knockback Applied with Force: " + knockbackForce + " " + knockbackDirection);

        StartCoroutine(EndKnockback());
    }

    private IEnumerator EndKnockback()
    {
        yield return new WaitForSeconds(0.2f);
        isKnockbackActive = false;
    }

    IEnumerator CollisionDamageCooldown()
    {
        canTakeCollisionDamage = false;
        //Debug.Log("Collision Damage Cooldown Started");
        yield return new WaitForSeconds(damageCooldown);
        canTakeCollisionDamage = true;
        //Debug.Log("Collision Damage Cooldown Ended");
    }

    public bool IsKnockbackActive()
    {
        return isKnockbackActive;
    }
}
