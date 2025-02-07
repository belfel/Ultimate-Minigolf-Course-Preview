using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Transform partToSpin;

    private void Update()
    {
        partToSpin.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, relativeTo:Space.Self);
    }
}
