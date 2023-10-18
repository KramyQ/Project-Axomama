using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ActorComponents
{
    public class InteractorComponent : MonoBehaviour
    {
        public LayerMask itemLayerMask;
        
        private List<IInteractable> _interactables = new List<IInteractable>();
        private IInteractable _lastInteractable = null;
        private IInteractable _interactable = null;
        private IInteractable _toggelableInteractable = null;

        public UnityEvent<IInteractable> onInteractableStartInteract;
        public UnityEvent<IInteractable> onInteractableStopInteract;

        private void Update()
        {
            _interactable = GetClosestInteractable();
        }

        public void StartInteract(InputAction.CallbackContext value)
        {
            if (_toggelableInteractable != null)
            {
                _toggelableInteractable.StopInteract(this);
                onInteractableStopInteract.Invoke(_toggelableInteractable);
                _toggelableInteractable = null;
            }
            else if (_interactable != null)
            {
                _interactable.StartInteract(this);
                onInteractableStartInteract.Invoke(_interactable);
                
                if (_interactable.IsToggle())
                {
                    _toggelableInteractable = _interactable;
                }
            }
        }
        
        public void StopInteract(InputAction.CallbackContext value)
        {
            if (_interactable != null)
            {
                if (_interactable.IsToggle())
                {
                    return;
                }
                
                _interactable.StopInteract(this);
                onInteractableStopInteract.Invoke(_interactable);
            }
        }

        private IInteractable GetClosestInteractable()
        {
            _interactables = new List<IInteractable>();
            
            Collider[] colliders = Physics.OverlapSphere(transform.position, 3f, itemLayerMask);
            foreach (var collider in colliders)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    _interactables.Add(interactable);
                }
            }
            
            //print(String.Format("Found {0} interactables", _interactables.Count));
            
            float closestDistance = 1000f;
            IInteractable closestInteractable = null;
            
            foreach (var interactable in _interactables)
            {
                float distance = Vector3.Distance(interactable.GetInteractableGameObject().transform.position,
                    gameObject.transform.position);
                if (distance < closestDistance)
                {
                    closestInteractable = interactable;
                    closestDistance = distance;
                }
            }

            return closestInteractable;
        }
    }
}