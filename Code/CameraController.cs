using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Keybinds keybinds;

    public GameEvent enteredFreelook;
    public GameEvent exitedFreelook;
    public GameEvent introSkipped;

    public FloatVariable lookSensitivity;

    [SerializeField] private Transform target;
    [SerializeField] private Transform resetTransform;
    [SerializeField] private Transform podiumTransform;
    [SerializeField] private Transform buildingPhaseTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private float freelookMovementSpeed = 12f;
    [SerializeField] private float freelookSprintMovementSpeed = 20f;
    [SerializeField] private float freelookRotationSpeed = 2f;
    [SerializeField] private float YMin = 0f;
    [SerializeField] private float YMax = 80f;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float distance = 3f;

    private bool isPanning;
    private bool freelook;
    private bool allowInputs = true;

    private float currentX = 0f;
    private float currentY = 20f;
    private float currentYFreelook = 0f;

    private void Start()
    {
        StartPan();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!allowInputs)
        {
            return;
        }

        if (isPanning)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndPan();
            }

            return;
        }

        if (target != null && Input.GetKeyDown(keybinds.toggleFreelook.key))
        {
            ToggleFreelook();
        }

        if (target != null && !freelook)
        {
            FocusedControls();
        }

        else
        {
            FreelookControls();
        }
    }

    public void ToggleFreelook()
    {
        freelook = !freelook;
        if (freelook)
        {
            enteredFreelook.Raise();
            currentYFreelook = transform.rotation.eulerAngles.x;
        }

        else exitedFreelook.Raise();
    }

    public void ToggleFreelook(bool enable)
    {
        freelook = enable;
        if (freelook)
        {
            enteredFreelook.Raise();
            currentYFreelook = transform.rotation.eulerAngles.x;
        }

        else exitedFreelook.Raise();
    }

    public void StartPan()
    {
        isPanning = true;
    }

    public void EndPan()
    {
        animator.enabled = false;
        isPanning = false;
        freelook = false;

        GameManager.instance.ClientEndedIntroRpc(new RpcParams());
        introSkipped.Raise();
    }

    public void SetFollowTarget(Transform _target)
    {
        target = _target;
    }

    public void IgnoreInputs()
    {
        allowInputs = false;
    }

    public void AllowInputs()
    {
        allowInputs = true;
    }

    private void FocusedControls()
    {
        currentX += Input.GetAxis("Mouse X") * lookSensitivity.value;
        currentY -= Input.GetAxis("Mouse Y") * lookSensitivity.value;

        currentY = Mathf.Clamp(currentY, YMin, YMax);

        Vector3 Direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = target.position + rotation * Direction;

        transform.LookAt(target.position + new Vector3(0f, Mathf.Sqrt(distance) - 1f, 0f));

        distance -= Input.mouseScrollDelta.y * 0.2f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    private void FreelookControls()
    {
        float speed = freelookMovementSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed = freelookSprintMovementSpeed;

        Vector3 movement = Vector3.zero;

        if (keybinds.usingController)
        {
            movement += transform.forward * Input.GetAxisRaw("Vertical");
            movement += transform.right * Input.GetAxisRaw("Horizontal");
        }

        else
        {
            movement += transform.forward * keybinds.GetVerticalAxisRaw();
            movement += transform.right * keybinds.GetHorizontalAxisRaw();
        }

        movement.Normalize();
        movement *= speed * Time.deltaTime;
        transform.position += movement;


        currentYFreelook -= Input.GetAxis("Mouse Y") * freelookRotationSpeed * lookSensitivity.value;
        currentYFreelook = Mathf.Clamp(currentYFreelook, -85f, 85f);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * freelookRotationSpeed * lookSensitivity.value, Space.World);
        transform.rotation = Quaternion.Euler(currentYFreelook, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    public void OnPostmatchStarted()
    {
        gameObject.transform.position = podiumTransform.position;
        gameObject.transform.rotation = podiumTransform.rotation;
    }

    public void OnBuildingPhaseStarted()
    {
        gameObject.transform.position = buildingPhaseTransform.position;
        gameObject.transform.rotation = buildingPhaseTransform.rotation;
    }

    public void ResetCamera()
    {
        transform.position = resetTransform.position;
        transform.rotation = resetTransform.rotation;
    }
}
