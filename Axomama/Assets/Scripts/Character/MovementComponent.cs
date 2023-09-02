using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class MovementComponent : MonoBehaviour
{
    
    private TemporaryInputs m_playerInputActions;
    
    public new Rigidbody rigidbody;
    public Transform cameraHolderTransform;
    public Transform cameraTransform;
    public Transform shadowTransform;
    public LayerMask characterLayerMask;
    public CapsuleCollider capsuleCollider;
    public PhysicMaterial highFrictionPhysicMaterial;
    public PhysicMaterial noFrictionPhysicMaterial;

    public float maxWalkSpeed = 6f;
    public float maxAcceleration = 1f;
    public AnimationCurve accelerationFactorFromVelocityDotCurve;
    public float groundCheckDistance = 1f;
    public float sphereGroundCheckRadius = 0.5f;

    public float maxAirSpeed = 6f;
    public float maxAirAcceleration = 0.3f;
    public float airControlFactor = 0.5f;
    
    public float hangAirSpeed = 6f;
    public ForceMode hangAirForceMode = ForceMode.Acceleration;
 
    public float rotationSpeed = 8f;
    public float rotationTargetSpeed = 80f;
    public Transform rotationTargetTransform;

    public float jumpHeight = 2f;
    public int maxJumpAirCount = 1;
    public float maxJumpTime = 0.6f;
    public float coyoteTime = 0.4f;
    public Vector3 jumpGroundNormalFactor = new Vector3(0.4f, 1f, 0.4f);
 
    public float slopeDownForce = 4.5f;
    public float slopeLimitAngle = 45f;
    public float slideSpeed = 10f;
    public float slideControlFactor = 0.5f;
    public ForceMode slideForceMode = ForceMode.Force;
    public String slipperyTag = "Slippery";

    public float fallingGravityForce = 30f;
    public ForceMode fallingGravityForceMode = ForceMode.Acceleration;

    public Animator animator;
    public string forwardAnimatorFloat;
    public string lateralAnimatorFloat;
    public string jumpAnimatorTrigger;
    public string isGroundedAnimatorBool;
    public string isSlidingAnimatorBool;
    
    public float maxShadowIndicatorScale = 0.1f;
    public float minShadowIndicatorScale = 0.025f;
    public float offsetShadowIndicatorScale = 0.025f;
    
    private Vector3 _planarInput;
    private Vector3 _velocity, _desiredVelocity;
    private RaycastHit _groundHitInfo;
    private bool _isJumping;
    private bool _isGroundJumping;
    private bool _isLookingAtTarget;
    private int _availableAirJumpCount;
    private int _stepsSinceLastGrounded;
    private float _jumpTime;
    private float _lastTimeOnGround;
    public enum EMovementMode { Grounding = 0, Sliding = 1, Flying = 2, Hanging = 3 }
    private EMovementMode _movementMode;
    public enum ERotationMode { Velocity = 0, Target = 1 }
    private ERotationMode _rotationMode;

    private void Awake()
    {
        m_playerInputActions = new TemporaryInputs();
    }

    private void Start()
    {
        m_playerInputActions.PlayerMovement.Move.performed += Move;
        m_playerInputActions.PlayerMovement.Move.canceled += Move;
        m_playerInputActions.PlayerMovement.Jump.performed += StartJump;
        m_playerInputActions.PlayerMovement.Jump.canceled += StopJump;
        
        _movementMode = EMovementMode.Grounding;
        _rotationMode = ERotationMode.Velocity;
        
        _availableAirJumpCount = maxJumpAirCount;
        shadowTransform.parent = null;
        cameraHolderTransform.parent = null;
    }
    
    private void OnEnable()
    {
        m_playerInputActions.Enable();
    }

    private void OnDisable()
    {
        m_playerInputActions.Disable();
    }

    private void Update()
    {
        if (_movementMode != EMovementMode.Grounding)
        {
            _lastTimeOnGround += Time.deltaTime;
        }
        
        animator.SetFloat(forwardAnimatorFloat, _planarInput.x);
        animator.SetFloat(lateralAnimatorFloat, _planarInput.z);
        animator.SetBool(isGroundedAnimatorBool, _movementMode == EMovementMode.Grounding);
        animator.SetBool(isSlidingAnimatorBool, _movementMode == EMovementMode.Sliding);
    }

    void FixedUpdate()
    {
        // Get velocity
        _velocity = rigidbody.velocity;
        
        GroundCheck();

        // Modify velocity
        HandleMovement();
        HandleJump();
        HandleRotation();
            
        // Apply velocity
        rigidbody.velocity = _velocity;
    }

    private void GroundCheck()
    {
        // Get ground info
        if (Physics.Raycast(transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down, out RaycastHit hit, Mathf.Infinity, ~characterLayerMask))
        {
            _groundHitInfo = hit;
            
            // shadow indicator position
            shadowTransform.position = hit.point + new Vector3(0f, 0.05f, 0f);
            
            // align shadow to ground
            Vector3 shadowForward = Vector3.Cross(Vector3.up, hit.normal); // Get a forward direction parallel to the ground
            Vector3 shadowUp = hit.normal; // Use the hit normal as the up direction
            if (shadowForward != Vector3.zero)
            {
                Quaternion shadowRotation = Quaternion.LookRotation(shadowForward, shadowUp); // Calculate the rotation
                shadowTransform.rotation = shadowRotation; // Apply the rotation to the shadow
            }
            else
            {
                shadowTransform.eulerAngles = Vector3.zero; // Apply the rotation to the shadow
            }
            
            // shadow indicator scale
            float shadowTransformScale = (maxShadowIndicatorScale + offsetShadowIndicatorScale) - Mathf.Clamp(hit.distance, minShadowIndicatorScale, maxShadowIndicatorScale);
            shadowTransform.localScale = Vector3.one * shadowTransformScale;
        }
    }

    public void HandleMovement()
    {
        switch (_movementMode)
        {
            case EMovementMode.Grounding:
                GroundMovement();
                break;
            case EMovementMode.Sliding:
                SlidingMovement();
                break;
            case EMovementMode.Flying: 
                AirMovement();
                break;
            case EMovementMode.Hanging:
                HangMovement();
                break;
        }
    }

    private void HandleRotation()
    {
        switch (_rotationMode)
        {
            case ERotationMode.Velocity:
                if (_planarInput != Vector3.zero)
                {
                    rigidbody.rotation = Quaternion.Slerp(
                        rigidbody.rotation, 
                        Quaternion.LookRotation(_planarInput), 
                        rotationSpeed * Time.fixedDeltaTime);
                }
                break;
            case ERotationMode.Target:
                if (rotationTargetTransform != null)
                {
                    Vector3 directionToTarget = rotationTargetTransform.position - transform.position;
                    directionToTarget.y = 0f;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    rigidbody.rotation = Quaternion.RotateTowards(
                        rigidbody.rotation, 
                        targetRotation, 
                        rotationTargetSpeed * Time.fixedDeltaTime);
                }
                break;
        }
    }

    private void HandleJump()
    {
        if (!_isJumping) return;
        
        if (_isGroundJumping)
        {
            _isGroundJumping = false;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            _velocity.y += jumpSpeed;
        }
        else
        {
            _velocity = new Vector3(_velocity.x, jumpHeight, _velocity.z);
        }

        _jumpTime += Time.deltaTime;
        if (_jumpTime > maxJumpTime)
        {
            _isJumping = false;
        }
    }


    private void GroundMovement()
    {
        _desiredVelocity = _planarInput * maxWalkSpeed;
        
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right);
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward);
        
        float currentX = Vector3.Dot(_velocity, xAxis);
        float currentZ = Vector3.Dot(_velocity, zAxis);
        
        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedChange);
        
        _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        
        // prevent sliding
        if (_planarInput != Vector3.zero) capsuleCollider.material = noFrictionPhysicMaterial;
        else capsuleCollider.material = highFrictionPhysicMaterial;
    }

    private void SlidingMovement()
    {
        // Calculate the slide direction using the plane's normal
        Vector3 slideDirection = Vector3.ProjectOnPlane(_groundHitInfo.normal, Vector3.up).normalized;

        // Apply planar input to control the slide direction
        slideDirection = Vector3.Lerp(slideDirection, _planarInput, slideControlFactor).normalized;

        // Apply the sliding force
        Vector3 slideForce = slideDirection * slideSpeed;
        slideForce.y = -_groundHitInfo.point.y;
        
        rigidbody.AddForce(slideForce, slideForceMode);
    }

    private void HangMovement()
    {
        _desiredVelocity = _planarInput * hangAirSpeed;
        rigidbody.AddForce(_desiredVelocity, hangAirForceMode);
    }

    private void AirMovement()
    {
        _desiredVelocity = _planarInput * maxAirSpeed;

        float desiredVelocityDot = Vector3.Dot(_desiredVelocity, _velocity);
        float acceleration = maxAirAcceleration * accelerationFactorFromVelocityDotCurve.Evaluate(desiredVelocityDot);

        Vector3 newVelocity = Vector3.MoveTowards(
            _velocity,
            _desiredVelocity,
            acceleration * Time.fixedDeltaTime);

        Vector3 desiredAcceleration = (newVelocity - _velocity) / Time.fixedDeltaTime;
        
        Vector3 jumpVelocityLerp = Vector3.Lerp(Vector3.zero, desiredAcceleration, airControlFactor);
        
        // Custom falling gravity
        if (_velocity.y < 0f)
            _velocity.y -= fallingGravityForce * Time.fixedDeltaTime;
        
        _velocity += new Vector3(jumpVelocityLerp.x, 0f, jumpVelocityLerp.z);
    }

    private void SnapToGround()
    {
        float dot = Vector3.Dot(_velocity, _groundHitInfo.normal);
        if (dot > 0f)
        {
            _velocity = (_velocity - _groundHitInfo.normal * dot).normalized * _velocity.magnitude;
        }
    }
    
    Vector3 ProjectOnContactPlane (Vector3 vector) {
        return vector - _groundHitInfo.normal * Vector3.Dot(vector, _groundHitInfo.normal);
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        Vector3 cameraForwardPlanar = cameraTransform.forward;
        cameraForwardPlanar.y = 0f;
        cameraForwardPlanar.Normalize();
        
        Vector3 cameraRightPlanar = cameraTransform.right;
        cameraRightPlanar.y = 0f;
        cameraRightPlanar.Normalize();

        _planarInput = (input.x * cameraRightPlanar + input.y * cameraForwardPlanar);
    }

    public void StartJump(InputAction.CallbackContext context)
    {
        if (_movementMode == EMovementMode.Grounding || _lastTimeOnGround <= coyoteTime)
        {
            _movementMode = EMovementMode.Flying;
            _isGroundJumping = true;
            _jumpTime = 0f;
            _isJumping = true;
            
            animator.SetTrigger(jumpAnimatorTrigger);
        }
        else if (_movementMode == EMovementMode.Flying && _availableAirJumpCount > 0)
        {
            _movementMode = EMovementMode.Flying;
            _availableAirJumpCount--;
            
            _isGroundJumping = false;
            _jumpTime = 0f;
            _isJumping = true;
            
            animator.SetTrigger(jumpAnimatorTrigger);
        }
    }

    public void StopJump(InputAction.CallbackContext context)
    {
        _isJumping = false;
    }

    public void SetMovementMode(int movementModeInt)
    {
        _movementMode = (EMovementMode)movementModeInt;
    }
    
    public void SetRotationMode(int rotationModeInt)
    {
        _rotationMode = (ERotationMode)rotationModeInt;
    }
    
    public void OnCollisionEnter(Collision other)
    {
        _availableAirJumpCount = maxJumpAirCount;
        _lastTimeOnGround = 0f;
        
        if (Vector3.Angle(_groundHitInfo.normal, Vector3.up) > slopeLimitAngle /*|| 
            _groundHitInfo.transform.CompareTag(slipperyTag)*/)
        {
            _movementMode = EMovementMode.Sliding;
            return;
        }

        _movementMode = EMovementMode.Grounding;
    }

    public void OnCollisionStay(Collision other)
    {
        _availableAirJumpCount = maxJumpAirCount;
        _lastTimeOnGround = 0f;
        
        if (_movementMode == EMovementMode.Hanging)
            return;
        
        if (Vector3.Angle(_groundHitInfo.normal, Vector3.up) > slopeLimitAngle /*|| 
            _groundHitInfo.transform.CompareTag(slipperyTag)*/)
        {
            _movementMode = EMovementMode.Sliding;
            return;
        }

        _movementMode = EMovementMode.Grounding;
    }

    public void OnCollisionExit(Collision other)
    {
        if (_movementMode == EMovementMode.Hanging)
            return;
        
        _movementMode = EMovementMode.Flying;
    }
}
