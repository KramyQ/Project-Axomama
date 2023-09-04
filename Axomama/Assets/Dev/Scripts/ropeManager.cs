using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;

public class ropeManager : MonoBehaviour
{

    public Transform handTransform;
    public GameObject lassoPrefab;
    public float ropeThrowForce = 300f;
    public float paraboleForce = 300f;
    
    
    public void throwRope(Vector3 direction)
    {
        Vector3 spawnPoint = handTransform.position + direction;
        GameObject newLasso = Instantiate(lassoPrefab, spawnPoint, transform.rotation);
        CharacterLasso lassoClass = newLasso.GetComponent<CharacterLasso>();
        lassoClass.latchAttach.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
        lassoClass.characterAttach.target = handTransform;
        lassoClass.characterAttach.attachmentType = ObiParticleAttachment.AttachmentType.Static;
        Vector3 ForceToApply = ropeThrowForce * direction;
        ForceToApply.y = ForceToApply.y + paraboleForce;
        lassoClass._rb.AddForce(ForceToApply);
        // currentLasso = lassoClass.gameObject;
        // Destroy(newLasso, 1f);
    }
}
