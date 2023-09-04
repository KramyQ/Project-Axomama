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
        Debug.Log(latchAttach.isBound);
        characterAttach = components[1];
        Debug.Log(characterAttach.isBound);
    }
}
