using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DisconnectionMonitor : NetworkBehaviour
{
    [SerializeField] private float repeatRate = 0.3f;
    [SerializeField] private float maxResponseTime = 5f;

    private Dictionary<int, float> timers = new Dictionary<int, float>();
    private Dictionary<int, ulong> idClientPairs = new Dictionary<int, ulong>();
    private List<int> idsToBeRemoved = new List<int>();
    private int repeat = 0;

    private void Update()
    {
        foreach (var timer in timers)
        {
            timers[timer.Key] += Time.deltaTime;
            if (timer.Value > maxResponseTime)
            {
                ResponseTimeout(idClientPairs[timer.Key]);
                idsToBeRemoved.Add(timer.Key);
            }
        }

        foreach (int id in idsToBeRemoved)
        {
            timers.Remove(id);
            idClientPairs.Remove(id);
        }
        idsToBeRemoved.Clear();
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            return;
        }

        InvokeRepeating("Query", 0f, repeatRate);
    }

    private void ResponseTimeout(ulong clientId)
    {
        Debug.Log(clientId.ToString() + " timed out");
    }

    private void Query()
    {
        InitialQueryRpc(new RpcParams());
    }

    [Rpc(SendTo.Server)]
    private void InitialQueryRpc(RpcParams rpcParams)
    {
        InitialResponseRpc(new RpcParams());
    }

    [Rpc(SendTo.Server)]
    private void InitialResponseRpc(RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        timers.Add(repeat, 0f);
        idClientPairs.Add(repeat, senderId);
        RepeatingQueryRpc(repeat, RpcTarget.Single(senderId, RpcTargetUse.Temp));
        repeat++;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void RepeatingQueryRpc(int id, RpcParams rpcParams)
    {
        RepeatingResponseRpc(id, new RpcParams());
    }

    [Rpc(SendTo.Server)]
    private void RepeatingResponseRpc(int id, RpcParams rpcParams)
    {
        timers.Remove(id);
        idClientPairs.Remove(id);
    }
}
