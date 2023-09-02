using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class V2AnimationStateController : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private V4Movement movementClass;

    public UnityEvent onStopMovingJumpEvent;
    public UnityEvent onStartMovingJumpEvent;
    public UnityEvent onCharacterJumpEvent;
    public UnityEvent onCharacterLandEvent;

    private void OnEnable()
    {
        V4Movement.onStartMoving += onStartMovingJumpEvent.Invoke;
        V4Movement.onStopMoving += onStopMovingJumpEvent.Invoke;
        V4Movement.onJump += onCharacterJumpEvent.Invoke;
        V4Movement.onLand += onCharacterLandEvent.Invoke;
    }

    private void OnDisable()
    {
        V4Movement.onStartMoving -= onStartMovingJumpEvent.Invoke;
        V4Movement.onStopMoving -= onStopMovingJumpEvent.Invoke;
        V4Movement.onJump -= onCharacterJumpEvent.Invoke;
        V4Movement.onLand -= onCharacterLandEvent.Invoke;
    }

    public void TryPlayMovementTrailVFX()
    {
        if (movementClass.isMovePressed)
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
        animator.SetBool("IsMoving", movementClass.isMovePressed && movementClass.isGrounded);
        animator.SetBool("IsGrounded" ,movementClass.isGrounded && !movementClass.isLatched);
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
