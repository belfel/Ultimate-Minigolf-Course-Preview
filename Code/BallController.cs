using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public Keybinds keybinds;

    public FloatVariable ballHitForce;
    public FloatVariable ballMaxHitForce;
    public GameEvent ballStationary;
    public GameEvent ballMoving;
    public GameEvent outOfBounds;
    public GameEvent ballPositionReset;

    [SerializeField] private State currState;
    [SerializeField] private float forceBuildupMulti = 1f;
    [SerializeField] private float maximumStationaryVelocity = 0.1f;
    [SerializeField] private float maxOutOfBoundTime = 2f;
    [SerializeField] private GameObject anchor;
    [SerializeField] private GameObject pointer;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>();

    private Rigidbody rb;
    private Transform resetTransform;
    private Vector3 spawnPosition;
    private Quaternion lastStationaryRotation;
    private float outOfBoundsTimer = 0;
    private bool inputsAllowed;
    private bool isOutOfBounds;

    private MapSettings mapSettings;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        spawnPosition = transform.position;

        GameObject resetGO = new GameObject();
        resetGO.transform.position = spawnPosition;
        resetTransform = resetGO.transform;

        GetMapSettings();
    }

    private void Update()
    {
        UpdateOutOfBoundsTimer();

        if (inputsAllowed)
            return;

        switch (currState)
        {
            default:
            case State.Stationary:
                if (!IsStationaryCheck())
                {
                    currState = State.Moving;
                    ballMoving.Raise();
                    pointer.SetActive(false);
                    break;
                }

                ProcessMouseInputs();
                break;

            case State.Moving:
                if (IsStationaryCheck())
                {
                    currState = State.Stationary;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    pointer.SetActive(true);
                    ballHitForce.SetValue(0f);
                    UpdateResetPosition();
                    ballStationary.Raise();
                    break;
                }
                break;
        }

        ProcessKeyboardInputs();
        Rotate();
    }

    private void UpdateResetPosition()
    {
        RaycastHit hit;
        int layerMask = ~(1 << gameObject.layer);
        if (!Physics.Raycast(transform.position, Vector3.down, out hit, 1f, layerMask))
            return;

        if (resetTransform)
            Destroy(resetTransform.gameObject);

        GameObject resetGO = new GameObject();
        resetGO.transform.position = transform.position;
        resetGO.transform.rotation = transform.rotation;
        resetGO.transform.parent = hit.collider.gameObject.transform;
        resetTransform = resetGO.transform;
    }

    private void UpdateOutOfBoundsTimer()
    {
        if (!isOutOfBounds)
            return;
        outOfBoundsTimer += Time.deltaTime;
        if (outOfBoundsTimer > maxOutOfBoundTime)
            ResetBall();
    }

    private void GetMapSettings()
    {
        mapSettings = GameManager.instance.mapSettings;
        if (mapSettings == null)
        {
            Invoke("GetMapSettings", 1f);
            return;
        }

        InvokeRepeating("CheckIfOutOfBounds", 0f, 0.2f);
    }

    private void CheckIfOutOfBounds()
    {
        Vector2[] borders = { mapSettings.buildRangeX, mapSettings.buildRangeY, mapSettings.buildRangeZ };
        float[] positions = { transform.position.x, transform.position.y, transform.position.z };

        for (int i = 0; i < 3; i++)
        {
            if (positions[i] < borders[i].x || positions[i] > borders[i].y)
            {
                if (!isOutOfBounds)
                {
                    outOfBounds.Raise();
                    isOutOfBounds = true;
                }

                return;
            }
        }

        isOutOfBounds = false;
        outOfBoundsTimer = 0f;
    }

    private void Rotate()
    {
        if (anchor)
            anchor.transform.rotation = Camera.main.transform.rotation;
    }

    private bool IsStationaryCheck()
    {
        if (rb.linearVelocity.magnitude > maximumStationaryVelocity)
            return false;
        else return true;
    }

    private void ProcessMouseInputs()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            ballHitForce.SetValue(Mathf.Min(ballHitForce.value + forceBuildupMulti * Time.deltaTime, ballMaxHitForce.value));
        }

        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            rb.AddForce(anchor.transform.forward * ballHitForce.value, ForceMode.Impulse);

            PlayHitAudio(ballHitForce.value / ballMaxHitForce.value);

            ballHitForce.SetValue(0f);
        }
    }

    private void PlayHitAudio(float forceIndex)
    {
        int index = (int)(forceIndex / (1.0f / audioClips.Count));

        if (index == 0)
            audioSource.gameObject.GetComponent<AudioVolumeChanger>().OverrideInitialVolume(1f);
        else audioSource.gameObject.GetComponent<AudioVolumeChanger>().OverrideInitialVolume(forceIndex);

        audioSource.PlayOneShot(audioClips[Math.Min(index, audioClips.Count -1)]);
    }

    private void ProcessKeyboardInputs()
    {
        if (Input.GetKeyDown(keybinds.quickReset.key))
            ResetBall();

        if (Input.GetKeyDown(keybinds.fullReset.key))
            Respawn();
    }

    private void Respawn()
    {
        trail.emitting = false;
        Invoke("ReEnableTrail", 0.2f);
        Invoke("ResetBallPositionToSpawn", 0.1f);
    }

    public void ResetBall()
    {
        trail.emitting = false;
        Invoke("ReEnableTrail", 0.2f);
        Invoke("ResetBallPosition", 0.1f);
    }
    
    private void ResetBallPosition()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = resetTransform.position;
        transform.rotation = lastStationaryRotation;
        ballPositionReset.Raise();
    }

    private void ResetBallPositionToSpawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPosition;
        ballPositionReset.Raise();
    }

    private void ReEnableTrail()
    {
        trail.emitting = true;
    }

    public void IgnoreInputs()
    {
        inputsAllowed = true;
    }

    public void AllowInputs()
    {
        inputsAllowed = false;
    }

    private IEnumerator DelayAllowInputsRoutine()
    {
        yield return new WaitForEndOfFrame();
        inputsAllowed = true;
    }

    private enum State
    {
        Stationary, Moving, 
    }
}
