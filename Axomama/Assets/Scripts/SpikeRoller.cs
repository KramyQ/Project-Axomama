using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpikeRoller : MonoBehaviour
{
    public float duration;
    public float distance;
    public Ease ease = Ease.InOutCubic;
    public float rotationAmplification = 1.5f;
    public float delayDuration = 0f;

    public Transform spikeRollerMeshTransform;
    
    private Vector3 _currentPosition;
    private Vector3 _targetPosition;

    private void Start()
    {
        _currentPosition = transform.localPosition;
        _targetPosition = transform.position + (transform.forward * distance);

        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(delayDuration);
        
        transform.DOMove(_targetPosition, duration).SetLoops(-1, LoopType.Yoyo).SetEase(ease).OnUpdate(() =>
        {
            Vector3 delta = transform.localPosition - _currentPosition;
            _currentPosition = transform.localPosition;

            float rotationAngle = Mathf.Rad2Deg * (delta.x / (2 * Mathf.PI * (transform.localScale.x / 2)));

            spikeRollerMeshTransform.Rotate(Vector3.right, rotationAngle * rotationAmplification);
        });
    }
}
