using System;
using System.Collections;
using System.Collections.Generic;
using ActorComponents;
using DG.Tweening;
using Managers;
using UnityEngine;
using UnityEngine.Animations;

public class Character : MonoBehaviour
{
    public int characterId = 0;
    public InteractorComponent interactorComponent;
    public HealthComponent healthComponent;
    public Animator animator;
    public Transform itemHolderRightTransform;
    public Transform itemHolderLeftTransform;
    public GameObject meshGameObject;
    
    private TemporaryInputs m_playerInputActions;
    
    private void Awake()
    {
        m_playerInputActions = new TemporaryInputs();
    }

    private void OnEnable()
    {
        m_playerInputActions.Enable();
        m_playerInputActions.PlayerMovement.Interact.performed += interactorComponent.StartInteract;
        m_playerInputActions.PlayerMovement.Interact.canceled += interactorComponent.StopInteract;
    }

    private void OnDisable()
    {
        m_playerInputActions.Disable();
    }

    private void Start()
    {
        // Interactor
        interactorComponent.onInteractableStartInteract.AddListener(interactable =>
        {
            GameObject interactableGameObject = interactable.GetInteractableGameObject();
            if (interactableGameObject.name == "Chest")
            {
                DOVirtual.Float(0f, 1f, 0.5f, weight => {
                    animator.SetLayerWeight(1, weight);
                });
                animator.SetBool("IsHoldingChest", true);
                
                ParentConstraint parentConstraint = interactableGameObject.AddComponent<ParentConstraint>();
                ConstraintSource constraintSource = new ConstraintSource();
                constraintSource.sourceTransform = itemHolderLeftTransform;
                constraintSource.weight = 1f;
                parentConstraint.weight = 1f;
                parentConstraint.AddSource(constraintSource);
                parentConstraint.SetTranslationOffset(0, Vector3.zero);
                parentConstraint.constraintActive = true;
            }
        });
        interactorComponent.onInteractableStopInteract.AddListener(interactable =>
        {
            GameObject interactableGameObject = interactable.GetInteractableGameObject();
            if (interactableGameObject.name == "Chest")
            {
                DOVirtual.Float(1f, 0f, 0.5f, weight => {
                    animator.SetLayerWeight(1, weight);
                });
                animator.SetBool("IsHoldingChest", false);
                
                ParentConstraint parentConstraint = interactableGameObject.GetComponent<ParentConstraint>();
                if (parentConstraint)
                    Destroy(parentConstraint);
            }
        });
        
        // Health 
        healthComponent.onDie.AddListener(() =>
        {
            GameManager.Instance.NotifyCharacterDeath(this);
            meshGameObject.SetActive(false);
        });
        healthComponent.onSpawn.AddListener(() =>
        {
            meshGameObject.SetActive(true);
        });
    }
}
