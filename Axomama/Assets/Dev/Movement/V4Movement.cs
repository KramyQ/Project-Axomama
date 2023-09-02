using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;

public class V4Movement : MonoBehaviour
{
    
    // Movement Events
    public delegate void OnLand();
    public static OnLand onLand;
    public delegate void OnJump();
    public static OnJump onJump;
    public delegate void OnStopMoving();
    public static OnStopMoving onStopMoving;
    public delegate void OnStartMoving();
    public static OnStartMoving onStartMoving;
    
    // INPUTS
    private TemporaryInputs m_playerInputActions;
    private Vector3 moveInput = Vector3.zero;
    public bool isMovePressed = false;
    public bool wantsToJump = false;
    
    // Movement States
    public bool isGrounded = true;
    public bool isLatched = false;
    private bool isJumping = false;
    
    // Physics
    
    [SerializeField] public Rigidbody _rb;
    private RaycastHit _rayHit;
    private Vector3 relativeGoalVelocity = Vector3.zero;
    
    // Detect isGrounded Settings
    public float MaxGroundDistance = 0.15f;
    public float CharacterHeightOffset = 1f;
    // Jump Settings
    public float JumpForce = 200f;
    // Gravity Settings
    public float GravityIncrease = 10f;
    public float LatchedGravityIncrease = 2f;
    // Speed Settings
    public float MaxSpeed = 20f;
    private float SpeedFactor = 1f;
    public Vector3 ForceScale = new Vector3(1, 0, 1);
    public float RotationSpeed = 3f;

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
        m_playerInputActions.PlayerMovement.Move.performed += setMoveInput;
        m_playerInputActions.PlayerMovement.Move.canceled += setMoveInput;
        m_playerInputActions.PlayerMovement.Jump.performed += setJumpInput;
        m_playerInputActions.PlayerMovement.Jump.canceled += setJumpInput;
        // m_playerInputActions.PlayerMovement.LassoTest.performed += lasso;
    }

    private void setMoveInput(InputAction.CallbackContext context)
    {
        Vector2 inputDirection = context.ReadValue<Vector2>();
        moveInput.x = inputDirection.x;
        moveInput.z = inputDirection.y;
        bool formerPressedMove = isMovePressed;
        isMovePressed = inputDirection.x != 0 || inputDirection.y != 0;
        if (isMovePressed && !formerPressedMove) onStartMoving();
        if (!isMovePressed && formerPressedMove) onStopMoving();
    }
    
    private void setJumpInput(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            isJumping = false;
            wantsToJump = false;
        }
        else
        {
            wantsToJump = true;
        }
    }

    private void setIsGrounded()
    {
        Vector3 rayOrigin = _rb.position;
        rayOrigin.y = rayOrigin.y + CharacterHeightOffset;
        bool formerGrounded = isGrounded;
        if (Physics.Raycast(rayOrigin, transform.TransformDirection(Vector3.down), out _rayHit,
                50))
         {
             isGrounded = _rayHit.distance < MaxGroundDistance;
         }
         else
         {
             isGrounded = false;
         }
        
        if (formerGrounded != isGrounded && isGrounded) onLand();
    }

    private void move()
    {
        Vector3 neededAccel = getNeededAccel(getGoalVelocity());
        
        _rb.AddForce(Vector3.Scale(_rb.mass * neededAccel, ForceScale));
        if(wantsToJump && isGrounded || wantsToJump && isLatched) jump();
    }

    private void setRotation()
    {
        Vector3 velocityDirection = _rb.velocity;
        Vector3 speedForce = velocityDirection;
        speedForce.y = 0;
        if (speedForce.magnitude > 0.1f)
        {
            Quaternion dirQ = Quaternion.LookRotation(velocityDirection);
            _rb.angularVelocity = new Vector3(0, 0, 0);
            Quaternion slerp = Quaternion.Slerp(transform.rotation, dirQ,
                velocityDirection.magnitude * RotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(Quaternion.Euler(0, slerp.eulerAngles.y, 0));
        }
    }
    
    private Vector3 getGoalVelocity()
    {
        Vector3 goalVelocityVector = MaxSpeed*SpeedFactor*moveInput;
        relativeGoalVelocity = Vector3.MoveTowards(relativeGoalVelocity, goalVelocityVector,
            150 * Time.fixedDeltaTime);;
        return relativeGoalVelocity;
    }

    private Vector3 getNeededAccel(Vector3 goalVelocity)
    {  
        return Vector3.ClampMagnitude(((goalVelocity - _rb.velocity)  / Time.fixedDeltaTime), 150);
    }
    
    private void jump()
    {
        _rb.AddForce(new Vector3(0,JumpForce, 0));
        isJumping = true;
        wantsToJump = false;
        isLatched = false;
        onJump();
    }

    private void applyFallingForces()
    {
        if (!isLatched && (!isJumping && !isGrounded || !isGrounded && _rb.velocity.y < 0.4f)) _rb.AddForce(Vector3.down*GravityIncrease);
        if (isLatched && !isGrounded)  _rb.AddForce(Vector3.down*LatchedGravityIncrease);
    }

    private void FixedUpdate()
    {
        setIsGrounded();
        move();
        setRotation();
        applyFallingForces();
    }


}
