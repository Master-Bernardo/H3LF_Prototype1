using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothingSpeed;

    Vector3 offset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position,target.position + offset, Time.deltaTime*smoothingSpeed);
    }
}
