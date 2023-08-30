using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCube : MonoBehaviour
{
    public float rotateSpeed = 1;
    


    void Update()
    {
        transform.Rotate(0,rotateSpeed,0);
    }
}
