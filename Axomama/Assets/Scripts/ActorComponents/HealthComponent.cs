using System;
using Managers;
using UnityEngine;
using UnityEngine.Events;

namespace ActorComponents
{
    public class HealthComponent : MonoBehaviour
    {
        public UnityEvent onDie;
        public UnityEvent onSpawn;
        
        public void Die()
        {
            onDie.Invoke();
        }

        public void Spawn()
        {
            onSpawn.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("KillVolume"))
            {
                Die();
            }
        }
    }
}