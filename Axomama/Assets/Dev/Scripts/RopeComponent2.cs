using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Obi;
using Unity.VisualScripting;
using UnityEngine.Events;

public class RopeComponent2 : MonoBehaviour
{
    public Camera camera;
    public ObiRope obiRope;
    public ObiRopeCursor obiRopeCursor;
    public Animator animator;
    public Transform handAttachTransform;
    public Transform aimTargetTransform;
    public ObiSolver obiSolver;
    public LayerMask characterLayer;

    public float aimRopeLength = 0.5f;
    public float throwDistance = 5f;
    public float ropeUnravellingSpeed = 2f;
    public float ropeRavellingSpeed = 2f;
    public float ropeExtendingSpeed = 5f;
    public float ropeWeight = 50f;
    public float ropeThrowWeight = 1f;

    public UnityEvent onStartAiming;
    public UnityEvent onStopAiming;
    public UnityEvent<Vector3> onAttach;
    public UnityEvent onDetach;

    private ObiPinConstraintsBatch _handBatch;
    private ObiPinConstraintsBatch _batch;
    private ObiConstraints<ObiPinConstraintsBatch> _pinConstraints;
    
    private bool _isAiming;
    private bool _isAttached;
    private float _currentRopeLength = 0f;
    private RaycastHit _hit;

    private void Start()
    {
        obiSolver.transform.parent = null;
        aimTargetTransform.parent = null;

        obiRopeCursor.ChangeLength(aimRopeLength);
        
        // set particle masses at zero to move points
        for (int i = 0; i < obiRope.activeParticleCount; ++i)
            obiSolver.invMasses[obiRope.solverIndices[i]] = ropeThrowWeight;
        
        // Clear pin constraints:
        _pinConstraints = obiRope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        
        // Pin both ends of the rope (this enables two-way interaction between character and rope):
        _handBatch = new ObiPinConstraintsBatch();
        
        // attach to hand
        _handBatch.AddConstraint(obiRope.elements[0].particle1, handAttachTransform.GetComponent<ObiCollider>(), transform.localPosition, Quaternion.identity, 0, 0,10f);
        _handBatch.activeConstraintCount = 1;
        _pinConstraints.AddBatch(_handBatch);
        obiRope.SetConstraintsDirty(Oni.ConstraintType.Pin);
        
        // set particle masses at zero to move points
        for (int i = 0; i < obiRope.activeParticleCount; ++i)
            obiSolver.invMasses[obiRope.solverIndices[i]] = ropeWeight;
    }

    void Update()
    {
        // print(String.Format("restlength:{0} vs distance:{1}", rope.obiRope.restLength, Vector3.Distance(handAttachTransform.position, _hit.point)));
        //print(obiSolver.invMasses[rope.obiRope.solverIndices[0]]);
        
        // Aim ------------------------------------------------
        if (Input.GetMouseButtonDown(1))
        {
            obiRopeCursor.ChangeLength(aimRopeLength);
            _currentRopeLength = aimRopeLength;
            
            _isAiming = true;
            
            onStartAiming.Invoke();
        }

        if (Input.GetMouseButtonUp(1))
        {

            _isAiming = false;

            if (_isAttached)
                StartCoroutine(DetachRope());

            _isAttached = false;

            obiRopeCursor.ChangeLength(aimRopeLength);
            
            // Retract rope
            // float currentRopeLength = rope.obiRope.restLength;
            // DOTween.To(() => currentRopeLength, value => currentRopeLength = value, aimRopeLength, ropeRavellingSpeed)
            //     .OnUpdate(() =>
            //     {
            //         rope.obiRopeCursor.ChangeLength(currentRopeLength);
            //     })
            //     .OnStart(() =>
            //     {
            //         // set particle masses at zero to move points
            //         //for (int i = 0; i < rope.obiRope.activeParticleCount; ++i)
            //         //    obiSolver.invMasses[rope.obiRope.solverIndices[i]] = ropeWeight;
            //     })
            //     .OnComplete(() =>
            //     {
            //         //for (int i = 0; i < rope.obiRope.activeParticleCount; ++i)
            //         //    obiSolver.invMasses[rope.obiRope.solverIndices[i]] = ropeWeight;
            //
            //         _currentRopeLength = currentRopeLength;
            //     });
            
            onStopAiming.Invoke();
        }

        if (_isAiming)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            // Check if the ray hits something.
            if (Physics.Raycast(ray, out hitInfo))
            {
                // Update the position of the aim target to the hit point.
                aimTargetTransform.position = hitInfo.point;
                
                // Calculate the direction from the character's position to the hit point.
                Vector3 directionToAim = hitInfo.point - transform.position;

                // Rotate the character in the direction of the aim.
                if (directionToAim != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToAim);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
        
        // Throw ----------------------------------------------
        if (Input.GetMouseButtonUp(0))
        {
            if (!_isAiming) return;

            StartCoroutine(ThrowRope());
            
        }
        
        // Rope extending --------------------------------------
        if (Input.GetKey(KeyCode.Q))
        {
            _currentRopeLength -= ropeExtendingSpeed * Time.deltaTime;
            
            if (_currentRopeLength < aimRopeLength)
                _currentRopeLength = aimRopeLength;
            
            obiRopeCursor.ChangeLength(_currentRopeLength);
        }
        
        if (Input.GetKey(KeyCode.E))
        {
            _currentRopeLength += ropeExtendingSpeed * Time.deltaTime;
            
            if (_currentRopeLength > throwDistance)
                _currentRopeLength = throwDistance;
            
            obiRopeCursor.ChangeLength(_currentRopeLength);
        }
    }
    
    
    IEnumerator ThrowRope()
    {
        // set particle masses at zero to move points
        for (int i = 0; i < obiRope.activeParticleCount; ++i)
            obiSolver.invMasses[obiRope.solverIndices[i]] = 0f;

        _currentRopeLength = obiRope.restLength;
        
        // set target point before unravelling
        Vector3 aimTargetPosition = obiSolver.transform.InverseTransformPoint(aimTargetTransform.position);

        // update end point position
        while (true)
        {
            // calculate rope origin in solver space:
            Vector3 origin = obiSolver.transform.InverseTransformPoint(handAttachTransform.position);

            // update direction and distance to hook point:
            Vector3 direction = aimTargetPosition - origin;
            float distance = direction.magnitude;
            direction.Normalize();
            
            // increase length:
            _currentRopeLength += ropeUnravellingSpeed * Time.deltaTime;
            
            // if we have reached the desired length, break the loop:
            if (_currentRopeLength >= distance || _currentRopeLength >= throwDistance)
            {
                obiRopeCursor.ChangeLength(_currentRopeLength);
                print("reached");
                break;
            }
            
            // change rope length (clamp to distance between rope origin and hook to avoid overshoot)
            obiRopeCursor.ChangeLength(Mathf.Min(distance, _currentRopeLength));

            // iterate over all particles in sequential order, placing them in a straight line while
            // respecting element length:
            float length = 0;
            for (int i = 0; i < obiRope.elements.Count; ++i)
            {
                obiSolver.positions[obiRope.elements[i].particle1] = origin + direction * length;
                obiSolver.positions[obiRope.elements[i].particle2] = origin + direction * (length + obiRope.elements[i].restLength);
                length += obiRope.elements[i].restLength;
            }
            
            // check for collision
            var element = obiRope.GetElementAt(1,out float elementMu);
            var index1 = element.particle1; // first particle in the rope
            if (Physics.Raycast(obiRope.GetParticlePosition(index1), direction, out _hit, 0.5f))
            {
                ObiColliderBase hitObiColliderBase = _hit.collider.GetComponent<ObiColliderBase>();
                if (hitObiColliderBase != null)
                {
                    
                    _batch = new ObiPinConstraintsBatch();
                    _batch.AddConstraint(obiRope.elements[obiRope.elements.Count-1].particle2, _hit.collider.GetComponent<ObiColliderBase>(),
                        _hit.collider.transform.InverseTransformPoint(_hit.point), Quaternion.identity, 0, 0, float.PositiveInfinity);
                    
                    _batch.activeConstraintCount = 1;
                    _pinConstraints.AddBatch(_batch);

                    obiRope.SetConstraintsDirty(Oni.ConstraintType.Pin);
                    
                    _isAttached = true;

                    obiRopeCursor.ChangeLength(Vector3.Distance(handAttachTransform.position, _hit.point));
                    
                    onAttach.Invoke(_hit.point);
                    
                    break;
                }
            }

            // wait one frame:
            yield return null;
        }
        
        // set particle masses back to default after attaching
        for (int i = 0; i < obiRope.activeParticleCount; ++i)
            obiSolver.invMasses[obiRope.solverIndices[i]] = ropeWeight;

        // wait one frame:
        yield return null;
    }

    IEnumerator DetachRope()
    {
        _pinConstraints.RemoveBatch(_batch);

        obiRope.SetConstraintsDirty(Oni.ConstraintType.Pin);

        yield return null;
        
        onDetach.Invoke();
    }
}
