using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using Unity.VisualScripting;
using UnityEngine;

public class LatchOn : MonoBehaviour
{
    public CharacterLasso charLassoClass;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Latchable")
        {
            Transform hookTransform = other.GetComponentInChildren<CapsuleCollider>().transform;
            charLassoClass.characterAttach.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
            Vector3 loopPosition = new Vector3(hookTransform.position.x, hookTransform.position.y, hookTransform.position.z);
            charLassoClass.transform.position = loopPosition;
            charLassoClass._rb.constraints = RigidbodyConstraints.FreezeAll;
            charLassoClass.latchAttach.attachmentType = ObiParticleAttachment.AttachmentType.Static;
            charLassoClass.movementClass.latched = true;

        }
    }
}
