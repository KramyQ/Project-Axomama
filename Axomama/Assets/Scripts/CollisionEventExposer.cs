using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionEventExposer : MonoBehaviour
{
    public UnityEvent<Collision> onCollisionEnter, onCollisionStay, onCollisionExit;
    
    private void OnCollisionEnter(Collision other)
    {
        onCollisionEnter.Invoke(other);
    }

    private void OnCollisionStay(Collision other)
    {
        onCollisionStay.Invoke(other);
    }

    private void OnCollisionExit(Collision other)
    {
        onCollisionExit.Invoke(other);
    }
}
