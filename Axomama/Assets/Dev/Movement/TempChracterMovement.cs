using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempChracterMovement : NetworkBehaviour
{
    
    [SerializeField]
    GameObject serverCharacter;
    
    private TemporaryInputs m_playerInputActions;
    
    private Vector2 currentMovementInput;
    private Vector3 currentMovement;
    private bool isMovementPressed;
    private Rigidbody rigidbody;
    public float maxSpeed = 10f;
    public float rotationSpeed = 10f;

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
           rigidbody = GetComponent<Rigidbody>();
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

       private void Move()
       {
           if (isMovementPressed)
           {
               rigidbody.velocity = new Vector3 (currentMovement.x * maxSpeed, rigidbody.velocity.y, currentMovement.z * maxSpeed);
           }
           else
           {
               rigidbody.velocity = new Vector3(0, 0, 0);
           }

           Vector3 velocityDirection = rigidbody.velocity;

           if (rigidbody.velocity.magnitude > 0.1f)
           {
               Quaternion dirQ = Quaternion.LookRotation (velocityDirection);
               rigidbody.angularVelocity = new Vector3(0, 0, 0);
               Quaternion slerp = Quaternion.Slerp (transform.rotation, dirQ, velocityDirection.magnitude * rotationSpeed * Time.deltaTime);
               rigidbody.MoveRotation(slerp);
           }
           
       }



       private void Update()
       {
           Move();
       }

}
