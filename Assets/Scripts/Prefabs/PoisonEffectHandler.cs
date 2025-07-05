using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEffectHandler : MonoBehaviour
{
    private class PoisonEffect
    {
        public float totalDamage;
        public float duration;
        public float damagePerTick;
        public float elapsedTime;

        public PoisonEffect(float totalDamage, float duration)
        {
            this.totalDamage = totalDamage;
            this.duration = duration;
            this.damagePerTick = totalDamage / duration; // Calculate damage per tick
            this.elapsedTime = 0f;
        }
    }

    private List<PoisonEffect> poisonEffects = new List<PoisonEffect>();  // List to store multiple poison effects
    private Coroutine poisonCoroutine;  // Reference to the running poison coroutine

    public void ApplyPoisonEffect(float totalPoisonDamage, float poisonDuration)
    {
        // Add new poison effect to the list
        poisonEffects.Add(new PoisonEffect(totalPoisonDamage, poisonDuration));

        // Start poison effect coroutine if not already running
        if (poisonCoroutine == null)
        {
            poisonCoroutine = StartCoroutine(ApplyPoison());
        }
    }

    private IEnumerator ApplyPoison()
    {
        while (poisonEffects.Count > 0)
        {
            float totalDamageThisTick = 0f;
            float maxDuration = 0f;

            // Update and apply damage for each poison effect
            for (int i = poisonEffects.Count - 1; i >= 0; i--)
            {
                PoisonEffect effect = poisonEffects[i];
                effect.elapsedTime += Time.deltaTime;
                if (effect.elapsedTime >= effect.duration)
                {
                    poisonEffects.RemoveAt(i); // Remove expired effect
                }
                else
                {
                    totalDamageThisTick += effect.damagePerTick * Time.deltaTime;
                    if (effect.duration > maxDuration)
                    {
                        maxDuration = effect.duration;
                    }
                }
            }

            if (totalDamageThisTick > 0)
            {
                EnemyHealth enemyHealth = GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(totalDamageThisTick);
                }
            }

            // Wait until the next frame
            yield return null;
        }

        // Coroutine cleanup
        poisonCoroutine = null;
    }

    private void OnDisable()
    {
        // Stop the coroutine if this script or the enemy is disabled
        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
        }
    }
}
