using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private float cameraHeight = 5f;
    [SerializeField] private float offset = 5f;
    
    public float movementOffsetForce = 1;
    public float movementHeightForce = 1;
    public float predictionDistance = 1;
    public Vector3 Velocity = Vector3.zero;
    public float SmoothTime = 1f;
    private void Start()
    {
        if (IsOwner)
        {
            cameraHolder.SetActive(true);
            cameraHolder.transform.SetParent(null);
        }
    }

    private void LateUpdate()
    {
        if (player)
        {
            float projectedVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z).magnitude;
            float movementOffset = (projectedVelocity / 4.19f) * movementOffsetForce;
            float movementHeight = (projectedVelocity / 4.19f) * movementHeightForce;
            Vector3 predictedPlayerPosition = _rb.position + _rb.transform.forward.normalized * predictionDistance;
            Vector3 targetPosition = player.position + Vector3.up * (cameraHeight+movementHeight) + Vector3.back * (offset+movementOffset);
            cameraHolder.transform.position = Vector3.SmoothDamp( cameraHolder.transform.position, targetPosition, ref Velocity, SmoothTime);
        }
    }
}
