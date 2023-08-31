using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private V3Movement movementClass;

    private void OnEnable()
    {
        V3Movement.onCharacterJump += playJumpVFX;
        V3Movement.onCharacterLand += playLandVFX;
        V3Movement.onCharacterStartMoving += playStartMovingVFX;
        V3Movement.onCharacterStopMoving += playStopMovingVFX;
    }

    private void OnDisable()
    {
        V3Movement.onCharacterJump -= playJumpVFX;
        V3Movement.onCharacterLand -= playLandVFX;
        V3Movement.onCharacterStartMoving -= playStartMovingVFX;
        V3Movement.onCharacterStopMoving -= playStopMovingVFX;
    }
    
    private void playStartMovingVFX()
    {
        Debug.Log("Start");
    }
    
    private void playStopMovingVFX()
    {
        Debug.Log("Stop");
    }

    private void playLandVFX()
    {
        // Debug.Log("landed");
    }
    
    private void playJumpVFX()
    {
        // Debug.Log("jumped");
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
