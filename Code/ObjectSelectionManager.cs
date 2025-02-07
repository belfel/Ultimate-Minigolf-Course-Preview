using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelectionManager : NetworkBehaviour
{
    // TODO add 2 and 3 player variants (3 and 4 pieces)
    private const int numberOfButtons = 5;

    public LobbyData lobbyData;
    public PlayerColors playerColors;
    public GameEvent stopCursorSendingPosition;
    public GameEvent selectionFinished;

    [SerializeField] private NetworkObject cursorPrefab;
    [SerializeField] private List<GameObject> buttonPrefabs = new List<GameObject>();
    [SerializeField] private List<Vector2> iconPositions = new List<Vector2>();
    [SerializeField] private float spamProtectionTime = 0.2f;

    private List<NetworkObject> cursors = new List<NetworkObject>();
    private List<Button> buttons = new List<Button>();
    private Dictionary<int, ulong> selectedButtons = new Dictionary<int, ulong>();
    private ButtonData buttonData;
    private int readyPlayers = 0;
    private bool spamProtection = false;
    private bool objectSelected = false;

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
            return;

        RollObjectIds();
    }

    public void RollObjectIds()
    {
        if (!IsHost)
            return;

        int[] rolls = new int[5];
        for (int i = 0; i < 5; i++)
        {
            rolls[i] = Random.Range(0, buttonPrefabs.Count);
        }

        ButtonData newData = new ButtonData(rolls[0], rolls[1], rolls[2], rolls[3], rolls[4]);

        SetButtonData(newData);
        SendButtonDataRpc(newData, new RpcParams());
    }

    public void InstantiateButtons()
    {
        buttons.Clear();
        selectedButtons.Clear();

        int i = 0;
        foreach (var field in typeof(ButtonData).GetFields())
        {
            GameObject newButton = Instantiate(buttonPrefabs[(int)field.GetValue(buttonData)], Vector3.zero, Quaternion.Euler(0f, 0f, 72f * i), transform);
            RectTransform buttonTransform = newButton.GetComponent<RectTransform>();
            buttonTransform.anchoredPosition = iconPositions[i];
            Button buttonComponent = newButton.GetComponent<Button>();
            int iCopy = i;
            buttonComponent.onClick.AddListener(() => ButtonPressed(iCopy));
            buttons.Add(buttonComponent);
            i++;
        }
    }

    private void DespawnCursors()
    {
        for (int i = cursors.Count - 1; i >= 0; i--)
            if (cursors[i] != null)
                Destroy(cursors[i].gameObject);
        cursors.Clear();
    }

    private void SpawnSelectedObjects()
    {
        foreach (var pair in selectedButtons)
        {
            bool playerFound = false;
            foreach (var player in lobbyData.players)
                if (player.id == pair.Value)
                    playerFound = true;

            if (!playerFound)
                continue;

            GameObject objectPrefab = buttons[pair.Key].GetComponent<ObjectSelectionButton>().GetObjectPrefab();
            NetworkManager.SpawnManager.InstantiateAndSpawn(objectPrefab.GetComponent<NetworkObject>(), pair.Value);
        }
    }

    private void ObjectSelected(int buttonId, ulong clientId)
    {
        LockButtonRpc(clientId, buttonId, new RpcParams());
        selectedButtons.Add(buttonId, clientId);
        PieceSuccessfulyTakenRpc(buttonId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
        CheckIfEveryoneSelected();
    }

    public void CheckIfEveryoneSelected()
    {
        if (selectedButtons.Count >= lobbyData.PlayerCount())
        {
            ReadyCheckRpc(new RpcParams());
        }
    }

    #region RPCs

    [Rpc(SendTo.NotServer)]
    private void SendButtonDataRpc(ButtonData newButtonData, RpcParams rpcParams)
    {
        SetButtonData(newButtonData);
    }

    [Rpc(SendTo.Everyone)]
    private void LockButtonRpc(ulong clientId, int buttonId, RpcParams rpcParams)
    {
        // TODO use playerSlot to display piece next to the player who got it
        buttons[buttonId].GetComponent<ObjectSelectionButton>().SetColor(lobbyData.GetClientColorById(clientId));
        LockButton(buttonId);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void PieceAlreadyTakenRpc(int buttonId, RpcParams rpcParams)
    {
        if (objectSelected)
            return;

        //TODO Add some visual info
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void PieceSuccessfulyTakenRpc(int buttonId, RpcParams rpcParams)
    {
        objectSelected = true;

        foreach (Button b in buttons)
        {
            b.interactable = false;
        }
    }

    [Rpc(SendTo.Server)]
    private void ButtonPressedRpc(int buttonId, RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!selectedButtons.ContainsKey(buttonId))
        {
            ObjectSelected(buttonId, senderId);
        }

        else
            PieceAlreadyTakenRpc(buttonId, RpcTarget.Single(senderId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.Everyone)]
    private void ReadyCheckRpc(RpcParams rpcParams)
    {
        readyPlayers = 0;
        stopCursorSendingPosition.Raise();
        ReadyCheckConfirmationRpc(new RpcParams());
    }

    [Rpc(SendTo.Server)]
    private void ReadyCheckConfirmationRpc(RpcParams rpcParams)
    {
        readyPlayers++;
        if (readyPlayers >= lobbyData.PlayerCount())
            selectionFinished.Raise();
    }
    #endregion

    #region RPC Handlers

    private void LockButton(int buttonId)
    {
        buttons[buttonId].interactable = false;
    }

    private void SetButtonData(ButtonData newButtonData)
    {
        buttonData = newButtonData;
    }

    #endregion

    public void ButtonPressed(int buttonId)
    {
        if (spamProtection)
            return;

        ButtonPressedRpc(buttonId, new RpcParams());
        StartCoroutine(SpamClickProtection());
    }

    private IEnumerator SpamClickProtection()
    {
        spamProtection = true;
        yield return new WaitForSeconds(spamProtectionTime);
        spamProtection = false;
    }

    public void SpawnCursors()
    {
        if (!IsHost)
            return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var cursor = NetworkManager.SpawnManager.InstantiateAndSpawn(cursorPrefab, clientId);
            cursors.Add(cursor);
            Color cursorColor = lobbyData.GetClientColorById(clientId);
            cursor.GetComponent<NetworkCursor>().color.Value = cursorColor;
        }
    }

    public void OnTimerRanOut()
    {
        if (!IsHost)
            return;

        List<int> availableButtons = new List<int>{ 0, 1, 2, 3, 4 };
        List<ulong> remainingClientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();

        foreach (var pair in selectedButtons)
        {
            availableButtons.Remove(pair.Key);
            remainingClientIds.Remove(pair.Value);
        }

        foreach (ulong clientId in remainingClientIds)
        {
            int randomInt = Random.Range(0, availableButtons.Count);
            int randomButtonId = availableButtons[randomInt];
            ObjectSelected(randomButtonId, clientId);
            availableButtons.Remove(randomButtonId);
        }

        ReadyCheckRpc(new RpcParams());
    }

    public void OnObjectSelectionEnded()
    {
        for (int i = buttons.Count - 1; i >= 0; i--)
            Destroy(buttons[i].gameObject);

        if (!IsHost)
            return;
        
        DespawnCursors();
        SpawnSelectedObjects();
    }

    public struct ButtonData : INetworkSerializable
    {
        public int button1PrefabId;
        public int button2PrefabId;
        public int button3PrefabId;
        public int button4PrefabId;
        public int button5PrefabId;

        public ButtonData(int b1PrefabId, int b2PrefabId, int b3PrefabId, int b4PrefabId, int b5PrefabId)
        {
            button1PrefabId = b1PrefabId;
            button2PrefabId = b2PrefabId;
            button3PrefabId = b3PrefabId;
            button4PrefabId = b4PrefabId;
            button5PrefabId = b5PrefabId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref button1PrefabId);
            serializer.SerializeValue(ref button2PrefabId);
            serializer.SerializeValue(ref button3PrefabId);
            serializer.SerializeValue(ref button4PrefabId);
            serializer.SerializeValue(ref button5PrefabId);
        }
    }
}
