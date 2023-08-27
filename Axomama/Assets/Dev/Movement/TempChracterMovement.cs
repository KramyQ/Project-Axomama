using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TempChracterMovement : MonoBehaviour
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
           m_playerInputActions.PlayerMovement.Move.performed += movePlayer;
           m_playerInputActions.PlayerMovement.Move.canceled += movePlayer;
           rigidbody = GetComponent<Rigidbody>();
           currentMovement.x = 0;
           currentMovement.y = 0;
           currentMovement.z = 0;
       }
       
       private void movePlayer(InputAction.CallbackContext context)
       {
           Vector2 direction = context.ReadValue<Vector2>();
           setMovementInput(direction);
       }
       
       public void setMovementInput(Vector2 currentMovementInput)
       {
           currentMovement.x = currentMovementInput.x;
           currentMovement.z = currentMovementInput.y;
           isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
       }
       
       private void Update()
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
               Quaternion slerp = Quaternion.Slerp (transform.rotation, dirQ, velocityDirection.magnitude * rotationSpeed * Time.deltaTime);
               rigidbody.MoveRotation(slerp);
           }
       }

}
