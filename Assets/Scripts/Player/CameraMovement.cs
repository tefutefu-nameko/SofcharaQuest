using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private Vector3 shakeOffset;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    void Update()
    {
        if (target != null)
        {
            if (shakeDuration > 0)
            {
                shakeOffset = (Vector3)Random.insideUnitCircle * shakeMagnitude;
                shakeDuration -= Time.deltaTime;
            }
            else
            {
                shakeDuration = 0f;
                shakeOffset = Vector3.zero;
            }

            transform.position = target.position + offset + shakeOffset;
        }
    }

    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}
