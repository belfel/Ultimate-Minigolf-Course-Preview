using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventRotation : MonoBehaviour
{
    [SerializeField] private bool freezeX;
    [SerializeField] private bool freezeY;
    [SerializeField] private bool freezeZ;
    private Quaternion initialRotation;

    private void Start()
    {
        initialRotation = transform.localRotation;    
    }

    private void LateUpdate()
    {
        Vector3 rotation = transform.rotation.eulerAngles;
        if (freezeX)
            rotation.x = initialRotation.eulerAngles.x;
        if (freezeY)
            rotation.y = initialRotation.eulerAngles.y;
        if (freezeZ)
            rotation.z = initialRotation.eulerAngles.z;

        transform.rotation = Quaternion.Euler(rotation);
    }
}
