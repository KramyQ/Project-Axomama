using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class V2TempCharacterMovement : NetworkBehaviour
{
    [SerializeField]
    GameObject serverCharacter;
    
    private TemporaryInputs m_playerInputActions;
    
    private Vector2 currentMovementInput;
    private Vector3 currentMovement;
    private bool isMovementPressed;
    private Rigidbody rb;
    //Movement
    public float moveSpeed = 20;
    public float maxSpeed = 20;
    public bool grounded;
    public LayerMask whatIsGround;
    
    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;
    
    public float rotationSpeed = 10f;
    
    //Jumping
    private bool jumping;
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    private void Awake()
       {
           m_playerInputActions = new TemporaryInputs();
       }

       private void OnEnable()
       {
           m_playerInputActions.Enable();
       }

       private void OnDisable()
       {
           m_playerInputActions.Disable();
       }
       
       private void Start()
       {
           m_playerInputActions.PlayerMovement.Move.performed += setMovementInput;
           m_playerInputActions.PlayerMovement.Move.canceled += setMovementInput;
           m_playerInputActions.PlayerMovement.Jump.performed += jumpInputed;
           rb = GetComponent<Rigidbody>();
           currentMovement.x = 0;
           currentMovement.y = 0;
           currentMovement.z = 0;
       }
       
       private void setMovementInput(InputAction.CallbackContext context)
       {
           Vector2 direction = context.ReadValue<Vector2>();
           currentMovement.x = direction.x;
           currentMovement.z = direction.y;
           isMovementPressed = direction.x != 0 || direction.y != 0;
       }

       private void jumpInputed(InputAction.CallbackContext _)
       {
           jumping = true;
       }

       private void Move()
       {
           if (IsLocalPlayer && IsOwner)
           {
               if (readyToJump && jumping) Jump();
               
               if (isMovementPressed)
               {
                   float currentGroundVelocityMag = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
                   
                   rb.AddForce(new Vector3(currentMovement.x , 0, currentMovement.z ));
                   
               }
               else
           {
               Vector3 nullVector = new Vector3(0, rb.velocity.y, 0);
               rb.velocity = nullVector;
           }

           Vector3 velocityDirection = rb.velocity;
           Vector3 speedForce = velocityDirection;
           speedForce.y = 0;

           if (speedForce.magnitude > 0.1f)
           {
               Quaternion dirQ = Quaternion.LookRotation(velocityDirection);
               rb.angularVelocity = new Vector3(0, 0, 0);
               Quaternion slerp = Quaternion.Slerp(transform.rotation, dirQ,
                   velocityDirection.magnitude * rotationSpeed * Time.deltaTime);
               rb.MoveRotation(Quaternion.Euler(0, slerp.eulerAngles.y, 0));
           }
           }
       }
       
       private void Jump() {
           if (grounded && readyToJump) {
               readyToJump = false;

               //Add jump forces
               rb.AddForce(Vector2.up * jumpForce * 1.5f, ForceMode.Impulse);
               jumping = false;
               // rb.AddForce(normalVector * jumpForce * 0.5f);
            
               //If jumping while falling, reset y velocity.
               Vector3 vel = rb.velocity;
               if (rb.velocity.y < 0.5f)
                   rb.velocity = new Vector3(vel.x, 0, vel.z);
               else if (rb.velocity.y > 0) 
                   rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            
               Invoke(nameof(ResetJump), jumpCooldown);
           }
       }
       
       private void ResetJump() {
           readyToJump = true;
       }


private void FixedUpdate()
       {
           //Extra gravity
           if(!grounded)rb.AddForce(Vector3.down * 100);

           if (readyToJump && jumping) Jump();
           // Calculate the target velocity based on movementInput and moveSpeed
           Vector3 targetVelocity = new Vector3(currentMovement.x,0, currentMovement.z) * moveSpeed;

           // Calculate the velocity change needed to reach the target velocity
           Vector3 velocityChange = targetVelocity - rb.velocity;

           // Apply the necessary force to achieve the desired velocity change
           rb.AddForce(velocityChange, ForceMode.Impulse);
           
           Vector3 velocityDirection = rb.velocity;
           Vector3 speedForce = velocityDirection;
           speedForce.y = 0;
           
           if (speedForce.magnitude > 0.1f)
           {
               Quaternion dirQ = Quaternion.LookRotation(velocityDirection);
               rb.angularVelocity = new Vector3(0, 0, 0);
               Quaternion slerp = Quaternion.Slerp(transform.rotation, dirQ,
                   velocityDirection.magnitude * rotationSpeed * Time.deltaTime);
               rb.MoveRotation(Quaternion.Euler(0, slerp.eulerAngles.y, 0));
           }
       }
}
