using System;
using ActorComponents;
using Interfaces;
using UnityEngine;

namespace Items
{
    public class Chest : Item, IInteractable
    {
        public BoxCollider boxCollider;
        public new Rigidbody rigidbody;
        
        public bool IsToggle()
        {
            return true;
        }

        public GameObject GetInteractableGameObject()
        {
            return gameObject;
        }

        public void StartInteract(InteractorComponent interactorComponent)
        {
            boxCollider.enabled = false;
            rigidbody.isKinematic = true;
        }
        
        public void StopInteract(InteractorComponent interactorComponent)
        {
            boxCollider.enabled = true;
            rigidbody.isKinematic = false;
        }
    }
}