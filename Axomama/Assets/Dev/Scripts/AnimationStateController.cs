using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private V3Movement movementClass;
    
    
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
