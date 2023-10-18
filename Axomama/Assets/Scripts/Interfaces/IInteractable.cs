using ActorComponents;
using UnityEngine;

namespace Interfaces
{
    public interface IInteractable
    {
        bool IsToggle();
        GameObject GetInteractableGameObject();
        void StartInteract(InteractorComponent interactorComponent);
        void StopInteract(InteractorComponent interactorComponent);
    }
}