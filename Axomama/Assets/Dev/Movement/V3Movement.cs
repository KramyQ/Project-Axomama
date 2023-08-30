using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class V3Movement : NetworkBehaviour
{
    // SETUP PARAMS
    public float hoverHeight = 10;
    public float characterOffset = 1;
    public float groundDistance = 12;
    public float hoverSpringStrength = 10;
    public float JumpForce = 10;
    public float rotationSpeed = 10;

    public Vector3 ForceScale = new Vector3(1, 0, 1);

    public float maxSpeed = 8;
    public float speedFactor = 10;
    public float acceleration = 200;
    public float maxAccelForce = 150;
    private float accelFactor = 10;
    private Vector3 groundVelocity = new Vector3(0,0,0);

    public float gravityIncrease = 10;
    
    // PHYSICS
    [SerializeField] public Rigidbody _rb;
    private RaycastHit _rayHit;
    private bool _rayDidHit;
    private bool inJump = false;
    public bool wantsToJump = false;
    public bool grounded = true;

    private Vector3 m_goalVelocity = new Vector3(0,0,0);
    
    // INPUTS
    private TemporaryInputs m_playerInputActions;
    private Vector3 currentMovement;
    public bool isMovementPressed;
    
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
        m_playerInputActions.PlayerMovement.Jump.canceled += jumpInputed;
        currentMovement.x = 0;
        currentMovement.y = 0;
        currentMovement.z = 0;
    }

    private void jumpInputed(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            inJump = false;
            wantsToJump = false;
        }
        else
        {
            wantsToJump = true;
        }
    }
    
    private void setMovementInput(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        currentMovement.x = direction.x;
        currentMovement.z = direction.y;
        isMovementPressed = direction.x != 0 || direction.y != 0;
    }

    private void move()
    {
        float accel = acceleration * accelFactor;
        
        Vector3 goalVelocityVector = maxSpeed*speedFactor*currentMovement;
        
        m_goalVelocity = Vector3.MoveTowards(m_goalVelocity, goalVelocityVector + groundVelocity,
            accel * Time.fixedDeltaTime);

        Vector3 neededAcceleration = (m_goalVelocity - _rb.velocity) / Time.fixedDeltaTime;

        float maxAcceleration = maxAccelForce * accelFactor;

        neededAcceleration = Vector3.ClampMagnitude(neededAcceleration, maxAcceleration);
        
        _rb.AddForce(Vector3.Scale(neededAcceleration * _rb.mass, ForceScale));

        if (wantsToJump && grounded)
        {
            _rb.AddForce(new Vector3(0,JumpForce, 0));
            inJump = true;
            wantsToJump = false;
        }
        Vector3 velocityDirection = _rb.velocity;
        Vector3 speedForce = velocityDirection;
        speedForce.y = 0;
        if (speedForce.magnitude > 0.1f)
        {
            Quaternion dirQ = Quaternion.LookRotation(velocityDirection);
            _rb.angularVelocity = new Vector3(0, 0, 0);
            Quaternion slerp = Quaternion.Slerp(transform.rotation, dirQ,
                velocityDirection.magnitude * rotationSpeed * Time.deltaTime);
            _rb.MoveRotation(Quaternion.Euler(0, slerp.eulerAngles.y, 0));
        }

    }

    private void fireHoverRay()
    {
        Vector3 rayOrigin = _rb.position;
        rayOrigin.y = rayOrigin.y + characterOffset;
        _rayDidHit = Physics.Raycast(rayOrigin, transform.TransformDirection(Vector3.down), out _rayHit,
            50);
        if (_rayDidHit )
        {
            grounded = _rayHit.distance < groundDistance;
        }
    }

    private void addSrpingForces()
    {
        if (_rayDidHit)
        {
            Vector3 currentVel = _rb.velocity;
            Vector3 rayDir = transform.TransformDirection(Vector3.down);

            Vector3 otherVel = Vector3.zero;
            Rigidbody hitBody = _rayHit.rigidbody;
            if (hitBody)
            {
                otherVel = hitBody.velocity;
                if (grounded)
                {
                    // Calculate the player's position relative to the platform's center
                    Vector2 projectedRelativePosition = new Vector2(transform.position.x, transform.position.z) - new Vector2(hitBody.position.x, hitBody.position.z);

                    Vector2 projectedAngularVelocity = new Vector2(0, hitBody.angularVelocity.y);
                    
                    // Calculate the tangential velocity
                    Vector2 tangentialVelocity = Vector3.Cross(projectedAngularVelocity * Vector3.forward, projectedRelativePosition);
                    Vector3 projectedtangentialVelocity = new Vector3(tangentialVelocity.x, 0, tangentialVelocity.y);
                    // Apply the tangential velocity to the player's rigidbody
                    _rb.velocity = _rb.velocity+projectedtangentialVelocity;
                }
            }

            float rayDirVel = Vector3.Dot(rayDir, currentVel);
            float otherDirVel = Vector3.Dot(rayDir, otherVel);

            float relVel = rayDirVel - otherDirVel;

            float x = _rayHit.distance - hoverHeight;

            float springForce = (x * hoverSpringStrength) - (relVel * hoverSpringStrength );

            _rb.AddForce(rayDir * springForce);

            if (hitBody != null)
            {
                hitBody.AddForceAtPosition(rayDir * -springForce, _rayHit.point);
            }
        }
        
    }

    private void applyFallingForces()
    {
        if (!inJump && !grounded)
        {
            _rb.AddForce(Vector3.down*gravityIncrease);
        }
    }

    private void FixedUpdate()
    {
        fireHoverRay();
        move();
        applyFallingForces();
        addSrpingForces();
    }
}
