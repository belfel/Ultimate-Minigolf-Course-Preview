using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public PortalLinkUtility linkUtility;

    [SerializeField] private bool isFirstPortal = true;
    [SerializeField] private Portal exit;
    [SerializeField] private Camera portalCamera;
    [SerializeField] private MeshRenderer viewRenderer;

    private RenderTexture renderTexture;
    private bool tpBlocked = false;

    private void Awake()
    {
        renderTexture = new RenderTexture(512, 512, 24);
        portalCamera.targetTexture = renderTexture;

        if (isFirstPortal)
        {
            linkUtility.firstPortal = this;
            return;
        }
    }

    private void Start()
    {
        if (isFirstPortal)
            return;

        linkUtility.firstPortal.AssignExit(this);
        AssignExit(linkUtility.firstPortal);

        linkUtility.firstPortal.AssignRenderTexture(renderTexture);
        AssignRenderTexture(linkUtility.firstPortal.GetRenderTexture());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (tpBlocked || !other.gameObject.GetComponent<BallController>())
            return;

        if (exit)
            exit.BlockTeleport();
        else return;

        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
        Vector3 enterPosition = other.transform.position;
        Vector3 offset = enterPosition - transform.position;
        Vector3 enterVelocity = rb.linearVelocity;

        Quaternion relativeRotation = Quaternion.FromToRotation(transform.up, exit.transform.up);

        Vector3 exitVelocity = relativeRotation * enterVelocity;
        Vector3 exitOffset = relativeRotation * offset;

        StartCoroutine(PortalRoutine(rb, exit.transform.position + exitOffset, exitVelocity));
    }

    private IEnumerator PortalRoutine(Rigidbody rb, Vector3 exitPosition, Vector3 exitVelocity)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();

        rb.gameObject.transform.position = exitPosition;
        rb.linearVelocity = exitVelocity;
    }

    private void OnTriggerExit(Collider other)
    {
        tpBlocked = false;
    }

    public void BlockTeleport()
    {
        tpBlocked = true;
    }

    public void AssignRenderTexture(RenderTexture _renderTexture)
    {
        viewRenderer.material.SetTexture("_Texture2D", _renderTexture);
    }

    public RenderTexture GetRenderTexture()
    {
        return renderTexture;
    }

    public void AssignExit(Portal _exit)
    {
        exit = _exit;
    }
}
