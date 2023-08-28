using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private float cameraHeight = 5f;
    [SerializeField] private float offset = 5f;
    
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
            Vector3 targetPosition = player.position + Vector3.up * cameraHeight + Vector3.back * offset;
            cameraHolder.transform.position = Vector3.SmoothDamp( cameraHolder.transform.position, targetPosition, ref Velocity, SmoothTime);
        }
    }
}
