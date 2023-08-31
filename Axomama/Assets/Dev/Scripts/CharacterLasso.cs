using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class CharacterLasso : MonoBehaviour
{

    public GameObject obiRope;
    // Start is called before the first frame update
    public Rigidbody _rb;

    public void setAttachment(Transform charactTransform)
    {
        ObiParticleAttachment[] components = obiRope.GetComponents<ObiParticleAttachment>();
        components[1].target = charactTransform;
    }
}
