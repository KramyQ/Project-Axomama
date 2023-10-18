using System.Collections;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        private void Awake() 
        { 
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            } 
        }

        public float respawnTime = 3f;
        public Transform[] spawnTransforms;

        public void NotifyCharacterDeath(Character character)
        {
            StartCoroutine(RespawnDelay(character));
        }

        private IEnumerator RespawnDelay(Character character)
        {
            yield return new WaitForSeconds(respawnTime);
            
            GameObject characterGameObject = character.gameObject;
            characterGameObject.transform.position = spawnTransforms[character.characterId].position;
            characterGameObject.transform.rotation = spawnTransforms[character.characterId].rotation;
            
            character.healthComponent.Spawn();
        }
    }
}