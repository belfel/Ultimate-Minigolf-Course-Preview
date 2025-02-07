using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MainMenuBall : MonoBehaviour
{
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] private float minForce;
    [SerializeField] private float maxForce;
    [SerializeField] private float resetTime;
    [SerializeField] private float launchDelay;

    [SerializeField] private bool logForce = false;

    private Vector3 resetPosition;
    private Quaternion resetRotation;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        resetPosition = transform.position;
        resetRotation = transform.rotation;
    }

    private void Start()
    {
        StartCoroutine(BallRoutine());
    }

    private void LaunchBall()
    {
        float force = Random.Range(minForce, maxForce);
        if (logForce)
            Debug.Log(force);

        rb.AddForce(transform.forward * force);
    }

    private void ResetBall()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(rb.position, rb.rotation);
        transform.position = resetPosition;
        transform.rotation = resetRotation;
    }

    private IEnumerator BallRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(launchDelay);
            trailRenderer.enabled = true;
            LaunchBall();

            yield return new WaitForSeconds(resetTime - 1f);
            trailRenderer.enabled = false;

            yield return new WaitForSeconds(1f);
            ResetBall();
        }
    }
}
