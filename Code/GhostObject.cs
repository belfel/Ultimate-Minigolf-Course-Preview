using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GhostObject : NetworkBehaviour
{
    public GameEvent ghostObjectCloseToBorder;
    public GameEvent phase1Started;
    public GameEvent phase2Started;
    public GameEvent phase2Ended;
    public GameEvent ghostOverlapEnter;
    public GameEvent ghostOverlapExit;
    public GameEvent ghostRotation;

    public Keybinds keybinds;

    [SerializeField] private GameObject objectRoot;
    [SerializeField] private GameObject invalidPlacementIndicator;
    [SerializeField] private NetworkObject spawnedObjectPrefab;
    [SerializeField] private NetworkObject spawnedGhostPrefab;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private float distanceForward = 20f;
    [SerializeField] private float minDistanceFromCamera = 5f;
    [SerializeField] private float maxDistanceFromCamera = 25f;
    [SerializeField] private float gridIntervals = 0.5f;
    [SerializeField] private float closeToBorderMaxDistance = 2f;
    [SerializeField] private float rotateDuration = 0.2f;
    [SerializeField] private float placementDelay = 0.5f;

    private bool hold = false;
    private bool hasControl = false;
    private bool overlapping = false;
    private bool boxEnabled = false;
    private bool ignoreInpus = false;
    private bool isPlacementDelayOver = false;
    private int delayInvokeCount = 0;
    private int activeRotations = 0;

    private Vector2 bordersX;
    private Vector2 bordersY;
    private Vector2 bordersZ;

    private int axisIndex;
    private Vector3[] moveAxis = {Vector3.right, Vector3.up, Vector3.forward};
    [SerializeField] private List<GameObject> axisLines = new List<GameObject>();

    private void Awake()
    {
        if (objectRoot == null)
            objectRoot = gameObject;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            hasControl = true;
            InitBorderProximityCheck();
        }
    }

    void Update()
    {
        if (!hasControl || ignoreInpus)
            return;

        ProcessInputs();

        if (!hold)
            UpdatePosition();

        if (overlapping && !boxEnabled)
            if (invalidPlacementIndicator != null)
                invalidPlacementIndicator.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsOwner)
            ghostOverlapEnter.Raise();
    }

    private void OnTriggerStay(Collider other)
    {
        overlapping = true;
    }

    private void OnTriggerExit(Collider other)
    {
        overlapping = false;
        if (invalidPlacementIndicator != null)
            invalidPlacementIndicator.SetActive(false);

        if (IsOwner)
            ghostOverlapExit.Raise();
    }

    private void InitBorderProximityCheck()
    {
        bordersX = GameManager.instance.mapSettings.buildRangeX;
        bordersY = GameManager.instance.mapSettings.buildRangeY;
        bordersZ = GameManager.instance.mapSettings.buildRangeZ;

        InvokeRepeating("CheckIfCloseToBorder", 0.2f, 0.2f);
    }

    private void CheckIfCloseToBorder()
    {
        Vector2[] borders = { bordersX, bordersY, bordersZ };
        float[] positions = { objectRoot.transform.position.x, objectRoot.transform.position.y, objectRoot.transform.position.z };

        for (int i = 0; i<3; i++)
        {
            if (positions[i] <= borders[i].x + closeToBorderMaxDistance || positions[i] >= borders[i].y - closeToBorderMaxDistance)
            {
                ghostObjectCloseToBorder.Raise();
                return;
            }
        }
    }

    private void ProcessInputs()
    {
        if (hold)
        {
            if (Input.GetKeyDown(keybinds.rotateObject.key))
            {
                if (axisIndex == 0)
                    StartCoroutine(RotateAroundWorldAxisRoutine(Vector3.right, 90f, rotateDuration));
                else if (axisIndex == 1)
                    StartCoroutine(RotateAroundWorldAxisRoutine(Vector3.up, 90f, rotateDuration));
                else
                    StartCoroutine(RotateAroundWorldAxisRoutine(Vector3.forward, 90f, rotateDuration));

                ghostRotation.Raise();
            }

            if (isPlacementDelayOver && Input.GetMouseButtonDown(0))
                TryPlace();

            if (Input.GetMouseButtonDown(1))
            {
                hold = false;
                phase1Started.Raise();
                phase2Ended.Raise();
                axisLines[axisIndex].gameObject.SetActive(false);
            }
        }

        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Invoke("AllowPlacement", placementDelay);
                delayInvokeCount++;

                hold = true;
                phase2Started.Raise();
                axisLines[axisIndex].gameObject.SetActive(true);
            }

            distanceForward += Input.mouseScrollDelta.y * 1f;
            distanceForward = Mathf.Clamp(distanceForward, minDistanceFromCamera, maxDistanceFromCamera);
            return;
        }

        if (Input.GetKeyDown(keybinds.previousAxis.key))
            PrevAxisIndex();

        if (Input.GetKeyDown(keybinds.nextAxis.key))
            NextAxisIndex();

        Vector3 newPosition = objectRoot.transform.position;

        newPosition += Input.mouseScrollDelta.y * gridIntervals * moveAxis[axisIndex];

        rb.MovePosition(SnapPositionToGrid(newPosition));
    }

    private IEnumerator RotateAroundWorldAxisRoutine(Vector3 axis, float angle, float duration)
    {
        float timer = duration;
        float currentAngleTotal = 0f;
        activeRotations++;

        while (timer > 0f)
        {
            float angleFraction = angle * Time.deltaTime / duration;
            currentAngleTotal += angleFraction;
            objectRoot.transform.Rotate(axis, angleFraction, Space.World);

            foreach (GameObject axisLine in axisLines)
                axisLine.transform.rotation = Quaternion.identity;

            timer -= Time.deltaTime;
            yield return null;
        }

        // Correct the final rotation
        objectRoot.transform.Rotate(axis, angle - currentAngleTotal, Space.World);

        foreach (GameObject axisLine in axisLines)
            axisLine.transform.rotation = Quaternion.identity;

        activeRotations--;
    }

    private void AllowPlacement()
    {
        delayInvokeCount--;

        if (delayInvokeCount > 0)
        {
            return;
        }

        isPlacementDelayOver = true;
    }
    

    private void NextAxisIndex()
    {
        axisLines[axisIndex].gameObject.SetActive(false);

        if (axisIndex == moveAxis.Length - 1)
            axisIndex = 0;
        else axisIndex++;

        axisLines[axisIndex].gameObject.SetActive(true);
    }

    private void PrevAxisIndex()
    {
        axisLines[axisIndex].gameObject.SetActive(false);

        if (axisIndex == 0)
            axisIndex = moveAxis.Length - 1;
        else axisIndex--;

        axisLines[axisIndex].gameObject.SetActive(true);
    }

    private bool TryPlace()
    {
        if (overlapping || activeRotations > 0)
        {
            return false;
        }

        hasControl = false;
        isPlacementDelayOver = false;
        phase2Ended.Raise();

        if (spawnedGhostPrefab == null)
            GameManager.instance.LocalPlayerFinishedBuildingRpc(new RpcParams());
        else
            RequestSpawnGhostRpc(new RpcParams());

        RequestPlaceObjectRpc(objectRoot.transform.position, objectRoot.transform.rotation, new RpcParams());
        RequestDespawnGhostRpc(new RpcParams());

        return true;
    }

    private void UpdatePosition()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 newPosition = cameraTransform.position + cameraTransform.forward * distanceForward;
        rb.MovePosition(SnapPositionToGrid(newPosition));
    }

    private Vector3 SnapPositionToGrid(Vector3 position)
    {
        Vector3 snappedPosition = position;

        snappedPosition.x = Mathf.Clamp(gridIntervals * (int)Mathf.Round(snappedPosition.x / gridIntervals), bordersX.x, bordersX.y);
        snappedPosition.y = Mathf.Clamp(gridIntervals * (int)Mathf.Round(snappedPosition.y / gridIntervals), bordersY.x, bordersY.y);
        snappedPosition.z = Mathf.Clamp(gridIntervals * (int)Mathf.Round(snappedPosition.z / gridIntervals), bordersZ.x, bordersZ.y);

        return snappedPosition;
    }

    [Rpc(SendTo.Server)]
    private void RequestPlaceObjectRpc(Vector3 position, Quaternion rotation, RpcParams rpcParams)
    {
        NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(spawnedObjectPrefab, position: position, rotation: rotation, destroyWithScene: true);
        //InstantiateObjectRpc(position, rotation, new RpcParams());
    }

    [Rpc(SendTo.Server)]
    private void RequestSpawnGhostRpc(RpcParams rpcParams)
    {
        if (spawnedGhostPrefab != null)
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(spawnedGhostPrefab, rpcParams.Receive.SenderClientId);
    }

    [Rpc(SendTo.Server)]
    private void RequestDespawnGhostRpc(RpcParams rpcParams)
    {
        objectRoot.GetComponent<NetworkObject>().Despawn();
    }

    public void OnTimerRanOut()
    {
        if (!IsOwner)
            return;

        bool placed = TryPlace();
        if (placed)
            return;

        GameManager.instance.LocalPlayerFinishedBuildingRpc(new RpcParams());
        RequestDespawnGhostRpc(new RpcParams());
    }

    public void IgnoreInputs()
    {
        ignoreInpus = true;
    }

    public void UnignoreInputs()
    {
        ignoreInpus = false;
    }
}