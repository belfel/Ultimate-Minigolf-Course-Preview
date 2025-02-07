using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LatencyTester : NetworkBehaviour
{
    public LobbyData lobbyData;
    public GameEvent networkInitialized;

    private float pingTime = 0f;
    private int pingCount = 0;

    private void Update()
    {
        pingTime += Time.deltaTime;
    }

    public override void OnNetworkSpawn()
    {
        networkInitialized.Raise();
        InvokeRepeating("UpdatePings", 0f, 1f);
    }

    private void UpdatePings()
    {
        pingTime = 0f;
        PingRequestRpc(pingCount, new RpcParams());
        pingCount++;
    }

    [Rpc(SendTo.NotMe, Delivery = RpcDelivery.Unreliable)]
    private void PingRequestRpc(int id, RpcParams rpcParams)
    {
        PingReplyRpc(id, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams, Delivery = RpcDelivery.Unreliable)]
    private void PingReplyRpc(int id, RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        foreach (var player in lobbyData.players)
            if (player.id == senderId)
                player.AddPingValue((int)((pingTime * 1000f) + 1000f * (pingCount - id - 1)));

    }
}
