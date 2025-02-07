using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BallInitializer : NetworkBehaviour
{
    public NetworkVariable<Color> color = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private List<GameObject> deleteIfNotLocal = new List<GameObject>();
    [SerializeField] private Transform ballAnchor;
    [SerializeField] private LayerMask localBallLayer;
    [SerializeField] private SpriteRenderer marker;
    [SerializeField] private MeshRenderer outline;


    public override void OnNetworkSpawn()
    {
        UpdateColor();
        color.OnValueChanged += OnColorChanged;

        if (IsOwner)
        {
            marker.enabled = false;
            SetAsCameraTarget();
            gameObject.tag = "LocalPlayer";
            gameObject.layer++;

            return;
        }

        foreach (GameObject go in deleteIfNotLocal)
        {
            Destroy(go);
        }

        var ballController = gameObject.GetComponent<BallController>();
        if (ballController)
            Destroy(ballController);       
    }

    private void SetAsCameraTarget()
    {
        Camera.main.gameObject.GetComponent<CameraController>().SetFollowTarget(ballAnchor);
    }

    private void OnColorChanged(Color previousColor, Color newColor)
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (marker != null)
            marker.color = color.Value;

        if (outline != null)
            outline.material.SetColor("_Color", color.Value);
    }
}
