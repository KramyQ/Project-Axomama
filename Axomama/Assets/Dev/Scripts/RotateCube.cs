using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCube : MonoBehaviour
{
    public float rotateSpeed = 1;
    [SerializeField] public Rigidbody platform_rb;

    void FixedUpdate()
    {
        platform_rb.angularVelocity = new Vector3(0, rotateSpeed, 0);
    }
}
