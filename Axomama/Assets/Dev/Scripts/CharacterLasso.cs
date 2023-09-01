using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class CharacterLasso : MonoBehaviour
{

    public GameObject obiRope;
    // Start is called before the first frame update
    public Rigidbody _rb;

    public ObiParticleAttachment characterAttach;
    public ObiParticleAttachment latchAttach;

    public V3Movement movementClass;

    private void Awake()
    {
        ObiParticleAttachment[] components = obiRope.GetComponents<ObiParticleAttachment>();
        latchAttach = components[0];
        characterAttach = components[1];
    }

    public void setAttachment(Transform charactTransform)
    {
        characterAttach.target = charactTransform;
    }
    
    public void setLassoer(V3Movement lassoerMovementClass)
    {
        movementClass = lassoerMovementClass;
    }
}
