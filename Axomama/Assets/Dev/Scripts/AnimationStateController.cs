using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private V3Movement movementClass;

    public UnityEvent onStopMovingJumpEvent;
    public UnityEvent onStartMovingJumpEvent;
    public UnityEvent onCharacterJumpEvent;
    public UnityEvent onCharacterLandEvent;

    private void OnEnable()
    {
        V3Movement.onCharacterStartMoving += onStartMovingJumpEvent.Invoke;
        V3Movement.onCharacterStopMoving += onStopMovingJumpEvent.Invoke;
        V3Movement.onCharacterJump += onCharacterJumpEvent.Invoke;
        V3Movement.onCharacterLand += onCharacterLandEvent.Invoke;
    }

    private void OnDisable()
    {
        V3Movement.onCharacterStartMoving -= onStartMovingJumpEvent.Invoke;
        V3Movement.onCharacterStopMoving -= onStopMovingJumpEvent.Invoke;
        V3Movement.onCharacterJump -= onCharacterJumpEvent.Invoke;
        V3Movement.onCharacterLand -= onCharacterLandEvent.Invoke;
    }

    public void TryPlayMovementTrailVFX()
    {
        if (movementClass.isMovementPressed)
        {
            onStartMovingJumpEvent.Invoke();
        }
    }


    void Start()
    {
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("IsMoving" ,movementClass.isMovementPressed);
        animator.SetBool("IsGrounded" ,movementClass.grounded);
        if (movementClass.wantsToJump)
        {
            animator.SetTrigger("Jump");
        }

        float speedRatio = movementClass._rb.velocity.magnitude /4.18f;
        animator.SetFloat("Forward", speedRatio);
        
        
        // maxSpeed*speedFactor*currentMovement
        // animator.SetFloat("Forward" ,);
    }
}
