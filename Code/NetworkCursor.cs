using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCursor : NetworkBehaviour
{
    public Vector2 movementTarget;
    public NetworkVariable<Color> color = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private float positionSendRate = 0.02f;
    [SerializeField] private float lerpMultiplier = 1f;
    [SerializeField] private Image cursorImage;
    private bool sendPosition = true;


    private void Update()
    {
        if (IsOwner)
            return;

        transform.position = Vector2.Lerp(transform.position, movementTarget, lerpMultiplier * Time.deltaTime);
    }

    public override void OnNetworkSpawn()
    {
        GameObject canvas = GameObject.FindWithTag("Canvas");
        if (canvas)
            transform.parent = canvas.transform;

        if (IsOwner)
        {
            gameObject.GetComponent<FollowCursor>().enabled = true;
            Invoke("SendPositionToOthers", 0f);
        }

        UpdateColor();
        color.OnValueChanged += OnColorChanged;
    }

    private void OnColorChanged(Color previousColor, Color newColor)
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (cursorImage != null)
            cursorImage.color = color.Value;
    }

    public void SendPositionToOthers()
    {
        Vector2 normalizedPosition = new Vector2(transform.position.x / Screen.width, transform.position.y / Screen.height);
        UpdateCursorPositionRpc(normalizedPosition, new RpcParams());
        if (sendPosition)
            Invoke("SendPositionToOthers", positionSendRate);
    }

    public void StopSendingPosition()
    {
        sendPosition = false;
    }

    [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Unreliable)]
    private void UpdateCursorPositionRpc(Vector2 normalizedPosition, RpcParams rpcParams)
    {
        // TODO Find out how to make this work with different aspect ratios
        Vector2 newPosition = new Vector2(normalizedPosition.x * Screen.width, normalizedPosition.y * Screen.height);
        movementTarget = newPosition;
    }
}
