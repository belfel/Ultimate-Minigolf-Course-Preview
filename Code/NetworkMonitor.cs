using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkMonitor : NetworkBehaviour
{
    public static NetworkMonitor Instance;
    public GameEvent hostDisconnected;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    [Rpc(SendTo.NotMe)]
    public void HostQuitInformOthersRpc(RpcParams rpcParams)
    {
        hostDisconnected.Raise();
    }
}
