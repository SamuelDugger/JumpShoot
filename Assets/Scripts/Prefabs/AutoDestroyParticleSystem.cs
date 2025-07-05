using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
    private void Start()
    {
        // Ensure this script is only added to Particle Systems
        if (GetComponent<ParticleSystem>() == null)
        {
            Debug.LogError("AutoDestroyParticleSystem script should be attached to a ParticleSystem.");
            Destroy(this);
            return;
        }

        // Destroy the GameObject after the Particle System finishes playing
        var particleSystem = GetComponent<ParticleSystem>();
        float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
        Destroy(gameObject, duration);
    }
}
