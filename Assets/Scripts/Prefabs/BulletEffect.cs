using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class BulletEffect : MonoBehaviour
{
    public abstract void ApplyEffect(GameObject target, float damage);
}

public class PoisonEffect : BulletEffect
{
    public float poisonDamageMultiplier = 0.5f; // Multiplier for poison damage based on bullet base damage
    public float poisonDuration = 3f; // Duration for poison effect in seconds

    private float totalPoisonDamage; // Total poison damage to be applied over the duration

    public override void ApplyEffect(GameObject target, float baseDamage)
    {
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Debug.Log("Poison Apply Effect started on target: " + target.name);

            // Calculate total poison damage based on the base damage
            totalPoisonDamage = baseDamage * poisonDamageMultiplier;

            Debug.Log("Total Poison Damage to be applied over " + poisonDuration + " seconds: " + totalPoisonDamage);

            // Ensure the enemy has a PoisonEffectHandler component
            PoisonEffectHandler poisonEffectHandler = target.GetComponent<PoisonEffectHandler>();
            if (poisonEffectHandler == null)
            {
                poisonEffectHandler = target.AddComponent<PoisonEffectHandler>();
            }

            // Apply poison effect using the handler on the enemy
            poisonEffectHandler.ApplyPoisonEffect(totalPoisonDamage, poisonDuration);
        }
        else
        {
            //Debug.LogWarning("No EnemyHealth component found on target: " + target.name);
        }
    }
}

public class ExplosiveEffect : BulletEffect
{
    public float explosionDamageMultiplier = 1f;  // Multiplier for explosion damage based on bullet base damage
    public float explosionRadius = 1f;   // Radius of explosion

    private float explosionDamage;       // Calculated explosion damage based on bullet base damage

    public override void ApplyEffect(GameObject target, float baseDamage)
    {

        //Debug.Log("Explosion Apply Effect");
        // Calculate explosion damage
        explosionDamage = baseDamage * explosionDamageMultiplier;

        // Get all colliders within the explosion radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(target.transform.position, explosionRadius);
        foreach (Collider2D collider in colliders)
        {
            // Apply damage only if the collider belongs to an enemy
            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(explosionDamage);  // Apply calculated explosion damage
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}