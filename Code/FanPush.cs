using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanPush : MonoBehaviour
{
    [SerializeField] private LayerMask localPlayerLayer;
    [SerializeField] private float pushForce = 1f;
    [SerializeField] private float distanceFalloffMultiplier = 1f;

    private void OnTriggerStay(Collider other)
    {
        if ((localPlayerLayer.value & (1 << other.gameObject.layer)) <= 0)
            return;

        var rb = other.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        ApplyForce(rb);
    }

    private void ApplyForce(Rigidbody rb)
    {
        float dist = Vector3.Distance(transform.position, rb.position);
        Vector3 force = (gameObject.transform.up * pushForce * Time.deltaTime) / (dist * dist * distanceFalloffMultiplier);
        rb.AddForce(force, ForceMode.Force);
    }

}
