using Steamworks;
using Steamworks.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityColor = UnityEngine.Color;

[RequireComponent(typeof(UILobby))]
public class LobbyManager : NetworkBehaviour
{
    public PlayerColors colors;
    public LobbyData data;
    public MapPreviews mapPreviews;

    public GameEvent joinedLobby;
    public GameEvent hostDisconnected;
    public GameEvent kickedEvent;
    public GameEvent bannedEvent;
    public GameEvent previouslyBannedEvent;

    [SerializeField] private GameObject mainMenuCanvasPrefab;
    [SerializeField] private GameObject awaitWindow;

    [SerializeField] private UnityEngine.UI.Image mapPreview;

    [SerializeField] private UILobbySetting mapSetting;
    [SerializeField] private UILobbySetting modeSetting;
    [SerializeField] private UILobbySetting goalSetting;
    [SerializeField] private UILobbySetting gameplayTimeSetting;
    [SerializeField] private UILobbySetting buildTimeSetting;

    [SerializeField] private TMP_InputField nameInputField;

    [SerializeField] private LobbyPlayerSlots playerSlots;


    private bool usingSteam = false;
    private UILobby ui;
    private Lobby currentLobby;

    private int readyConfirmationCount = 0;

    private List<ulong> bannedSteamIds = new List<ulong>();

    private void Awake()
    {
        data.ClearData();
        ui = GetComponent<UILobby>();

        if (FPSteamworksManager.instance != null)
            usingSteam = true;

        awaitWindow.SetActive(true);

        InitSettingsActions();
    }

    private void Start()
    {
        joinedLobby.Raise();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
        {
            HideAwaitWindow();
            InitSettings();
            return;
        }    

        CreateHostPlayer();

        if (FPSteamworksManager.instance == null)
        {
            HideAwaitWindow();
            return;
        }

        if (FPSteamworksManager.instance.GetCurrentLobby().HasValue)
            OnLobbyHostSuccess();
    }

    private void InitSettingsActions()
    {
        mapSetting.onUpdateAction = () => SetMapData();
        modeSetting.onUpdateAction = () => SetGamemodeData();
        goalSetting.onUpdateAction = () => SetGoalData();
        gameplayTimeSetting.onUpdateAction = () => SetGameplayDurationData();
        buildTimeSetting.onUpdateAction = () => SetBuildTimeData();
    }

    private void InitSettings()
    {
        Lobby lobby = FPSteamworksManager.instance.GetCurrentLobby().Value;

        mapSetting.SetValue(lobby.GetData("map"));
        modeSetting.SetValue(lobby.GetData("gamemode"));
        goalSetting.SetValue(lobby.GetData("goal"));
        gameplayTimeSetting.SetValue(lobby.GetData("gameplayDuration"));
        buildTimeSetting.SetValue(lobby.GetData("buildTime"));
    }

    public void OnLobbyHostFailed()
    {
        OnLeave();
    }

    public void OnLobbyHostSuccess()
    {
        if (awaitWindow.activeInHierarchy == false)
            return;

        HideAwaitWindow();
        InitLobbyData();
        nameInputField.text = SteamClient.Name + "'s lobby";
    }

    private void HideAwaitWindow()
    {
        awaitWindow.SetActive(false);
    }

    private void InitLobbyData()
    {
        currentLobby = FPSteamworksManager.instance.GetCurrentLobby().Value;
        currentLobby.SetData("name", SteamClient.Name + "'s lobby");
        SetMapData();
        SetGamemodeData();
        SetGoalData();
        SetGameplayDurationData();
        SetBuildTimeData();
    }

    #region lobby data parameter setters

    public void SetMapData()
    {
        currentLobby.SetData("map", mapSetting.GetCurrentValue());
        UpdateMapPreview();
    }

    public void SetGamemodeData()
    {
        currentLobby.SetData("gamemode", modeSetting.GetCurrentValue());
    }

    public void SetGoalData()
    {
        currentLobby.SetData("goal", goalSetting.GetCurrentValue());
    }

    public void SetGameplayDurationData()
    {
        currentLobby.SetData("gameplayDuration", gameplayTimeSetting.GetCurrentValue());
    }

    public void SetBuildTimeData()
    {
        currentLobby.SetData("buildTime", buildTimeSetting.GetCurrentValue());
    }

    #endregion

    private void CreateHostPlayer()
    {
        Player newPlayer = new Player();
        newPlayer.id = NetworkManager.Singleton.LocalClientId;
        newPlayer.slotId = 0;
        newPlayer.color = colors.colors[0];

        if (usingSteam)
        {
            newPlayer.steamId = Steamworks.SteamClient.SteamId;
            newPlayer.name = Steamworks.SteamClient.Name;
            newPlayer.sanitizedName = "Player 1";
        }
        else
        {
            newPlayer.name = "Player 1";
            newPlayer.sanitizedName = "Player 1";
        }

        data.players.Add(newPlayer);


        playerSlots.ReplaceSlotWithType(0, LobbyPlayerSlots.PlayerSlotTypes.Local);
        playerSlots.UpdateSlotColor(0, newPlayer.color);
        playerSlots.UpdateSlotPlayer(0, data.players[0]);

        if (usingSteam)
        {
            playerSlots.UpdateSlotAvatar(0, SteamClient.SteamId);
            playerSlots.UpdateSlotPlayerName(0, SteamClient.Name + " (You)");
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendSteamIdRpc(ulong senderSteamId, RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        if (bannedSteamIds.Contains(senderSteamId))
            DisconnectClient(KickReason.previouslyBanned, senderId);

        foreach (var player in data.players)
        {
            if (player.id == senderId)
            {
                player.steamId = senderSteamId;

                List<Friend> members = FPSteamworksManager.instance.GetLobbyMembers();
                foreach (Friend f in members)
                {
                    if (f.Id == player.steamId)
                    {
                        player.name = f.Name;
                        playerSlots.UpdateSlotPlayerName(player.slotId, f.Name);
                        playerSlots.UpdateSlotAvatar(player.slotId, player.steamId);
                    }
                }
                return;
            }
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void RequestSteamIdRpc(RpcParams rpcParams)
    {
        SendSteamIdRpc(SteamClient.SteamId, RpcTarget.Single(rpcParams.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnected;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnected;
    }

    private UnityColor GetFirstAvailableColor()
    {
        List<UnityColor> colorsCopy = new List<UnityColor>(colors.colors);
        foreach (Player p in data.players)
            colorsCopy.Remove(p.color);

        return colorsCopy[0];
    }

    private void PlayerConnected(ulong clientId)
    {
        if (!IsHost)
            return;

        Player newPlayer = data.AddPlayer(clientId, playerSlots.GetFirstOpenSlotId(), GetFirstAvailableColor());

        LobbySlot slot = playerSlots.ReplaceSlotWithType(newPlayer.slotId, LobbyPlayerSlots.PlayerSlotTypes.Other_Hostview);

        slot.SetPlayer(newPlayer);
        slot.SetBorderColor(newPlayer.color);
        slot.SetOnKickAction(() => DisconnectClient(KickReason.kick, clientId));
        slot.SetOnBanAction(() => BanClient(clientId));
        
        if (usingSteam)
            RequestSteamIdRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));

        foreach (var player in data.players)
        {
            int colorId = colors.GetColorId(player.color);
            SlotDataRpc(player.slotId, player.id, colorId, new RpcParams());
        }
    }

    private void DisconnectClient(KickReason reason, ulong clientId)
    {
        DisconnectRpc(reason, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    private void BanClient(ulong clientId)
    {
        ulong clientSteamId = data.GetPlayerByClientId(clientId).steamId;
        if (clientSteamId == 0)
            StartCoroutine(AwaitSteamIdRoutine(clientId));
        bannedSteamIds.Add(clientSteamId);
        DisconnectClient(KickReason.ban, clientId);
    }

    private IEnumerator AwaitSteamIdRoutine(ulong clientId)
    {
        ulong steamId = 0;
        while (steamId == 0)
        {
            steamId = data.GetPlayerByClientId(clientId).steamId;
            yield return new WaitForSeconds(0.1f);
        }
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void DisconnectRpc(KickReason reason, RpcParams rpcParams)
    {
        switch (reason)
        {
            default:
            case KickReason.kick:
                kickedEvent.Raise();
                break;
            case KickReason.ban:
                bannedEvent.Raise();
                break;
            case KickReason.previouslyBanned:
                previouslyBannedEvent.Raise();
                break;
        }

        OnLeave();
    }

    [Rpc(SendTo.NotMe)]
    private void SlotDataRpc(int slotId, ulong clientId, int colorId, RpcParams rpcParams)
    {
        // skip if already have the player added
        foreach (var player in data.players)
            if (player.slotId == slotId)
                return;

        Player newPlayer = data.AddPlayer(clientId, slotId, colors.colors[colorId]);

        ReplaceSlot(slotId, clientId);

        playerSlots.UpdateSlotPlayer(slotId, newPlayer);
        playerSlots.UpdateSlotColor(slotId, colors.colors[colorId]);

        if (!usingSteam)
            return;

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            playerSlots.UpdateSlotPlayerName(slotId, SteamClient.Name + " (You)");
            playerSlots.UpdateSlotAvatar(slotId, SteamClient.SteamId);
        }
        else
            RequestSteamIdRpc(RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }

    private void ReplaceSlot(int slotId, ulong clientId)
    {
        if (slotId == 0)
            playerSlots.ReplaceSlotWithType(slotId, LobbyPlayerSlots.PlayerSlotTypes.Host);

        else if (clientId == NetworkManager.Singleton.LocalClientId)
            playerSlots.ReplaceSlotWithType(slotId, LobbyPlayerSlots.PlayerSlotTypes.Local);

        else
            playerSlots.ReplaceSlotWithType(slotId, LobbyPlayerSlots.PlayerSlotTypes.Other);
    }

    [Rpc(SendTo.Server)]
    private void ChangeColorRpc(bool next, RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;

        List<UnityColor> colorsCopy = new List<UnityColor>(colors.colors);

        UnityColor currColor = UnityColor.white;
        foreach (Player p in data.players)
        {
            if (p.id != senderId)
                colorsCopy.Remove(p.color);
            else currColor = p.color;
        }

        int colorId = colorsCopy.FindIndex(x => x == currColor);

        if (next)
            colorId++;
        else colorId--;

        if (colorId > colorsCopy.Count - 1)
            colorId = 0;
        else if (colorId < 0)
            colorId = colorsCopy.Count - 1;

        int newColorId = colors.GetColorId(colorsCopy[colorId]);
        PlayerColorChangedRpc(senderId, newColorId, new RpcParams());
    }

    [Rpc(SendTo.Everyone)]
    private void PlayerColorChangedRpc(ulong playerId, int colorId, RpcParams rpcParams)
    {
        foreach (Player p in data.players)
            if (p.id == playerId)
            {
                UnityColor newColor = colors.colors[colorId];
                p.color = newColor;
                playerSlots.UpdateSlotColor(p.slotId, newColor);
            }
    }

    [Rpc(SendTo.Everyone)]
    private void PlayerDisconnectedRpc(ulong clientId, RpcParams rpcParams)
    {
        Player p = data.GetPlayerByClientId(clientId);
        playerSlots.ReplaceSlotWithType(data.GetSlotIdByClientId(clientId), LobbyPlayerSlots.PlayerSlotTypes.Empty);
        data.players.Remove(p);
    }

    public void ChangeColorPrevious()
    {
        ChangeColorRpc(false, new RpcParams());
    }

    public void ChangeColorNext()
    {
        ChangeColorRpc(true, new RpcParams());
    }

    public void OnGameStart()
    {
        ParseGameSettings();
        if (!IsHost)
            return;

        readyConfirmationCount = 0;
        ReadyCheckRpc(data.players.Count, new RpcParams());
    }

    private void ParseGameSettings()
    {
        data.pointsGoal = int.Parse(goalSetting.GetCurrentValue());
        data.gameplayTime = int.Parse(gameplayTimeSetting.GetCurrentValue());
        data.buildingTime = int.Parse(buildTimeSetting.GetCurrentValue());
    }

    [Rpc(SendTo.Everyone)]
    private void ReadyCheckRpc(int playerCount, RpcParams rpcParams)
    {
        if (CheckDataPlayerCount(playerCount))
            ReadyConfirmationRpc(new RpcParams());
    }

    [Rpc(SendTo.Server)]
    private void ReadyConfirmationRpc(RpcParams rpcParams)
    {
        readyConfirmationCount++;
        if (readyConfirmationCount == data.players.Count)
            ChangeScene();
    }

    private bool CheckDataPlayerCount(int expected)
    {
        // Request for steamID is made when receiving data about that player

        int playerDataCount = 0;

        if (usingSteam)
        {
            foreach (Player p in data.players)
            {
                if (p.steamId != 0)
                    playerDataCount++;
            }
        }

        else playerDataCount = data.players.Count;

        return playerDataCount == expected ? true : false;
    }

    public void OnLeave()
    {
        if (NetworkManager.Singleton.IsHost)
            HostDisconnectedRpc(new RpcParams());

        NetworkManager.Singleton.Shutdown();

        if (FPSteamworksManager.instance != null)
            FPSteamworksManager.instance.LeaveLobby();
        
        Instantiate(mainMenuCanvasPrefab);
    }

    [Rpc(SendTo.NotMe)]
    private void HostDisconnectedRpc(RpcParams rpcParams)
    {
        hostDisconnected.Raise();
    }

    private void PlayerDisconnected(ulong clientId)
    {
        if (IsHost)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
                return;
            else 
                PlayerDisconnectedRpc(clientId, new RpcParams());
        }
    }

    private void ChangeScene()
    {
        var status = NetworkManager.Singleton.SceneManager.LoadScene("Map_Grassy", LoadSceneMode.Single);
        if (status != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to load {"Map_Grassy"} " +
                    $"with a {nameof(SceneEventProgressStatus)}: {status}");
        }
    }

    public void ApplyLobbyNameChange()
    {
        Lobby lobby = FPSteamworksManager.instance.GetCurrentLobby().Value;
        string prevName = lobby.GetData("name");
        string newName = nameInputField.text;

        if (newName == "")
            newName = SteamClient.Name + "'s lobby";

        if (prevName != newName)
            lobby.SetData("name", newName);
    }

    private void UpdateMapPreview()
    {
        string map = mapSetting.GetCurrentValue();

        if (map == "grassy")
            mapPreview.sprite = mapPreviews.grassyLargePreview;
    }

    private enum KickReason
    {
        kick, ban, previouslyBanned
    }

    [System.Serializable]
    public class LobbyPlayerSlots
    {
        [SerializeField] private List<LobbySlot> slots = new List<LobbySlot>();

        [SerializeField] private GameObject prefab_Empty;
        [SerializeField] private GameObject prefab_Host;
        [SerializeField] private GameObject prefab_Local;
        [SerializeField] private GameObject prefab_Other;
        [SerializeField] private GameObject prefab_Hostview;

        private bool[] slotFree = { true, true, true, true };

        public LobbySlot ReplaceSlotWithType(int slotId, PlayerSlotTypes slotType)
        {
            if (slotType == PlayerSlotTypes.Empty)
                slotFree[slotId] = true;
            else 
                slotFree[slotId] = false;

            GameObject prefab = GetPrefabByType(slotType);

            Transform replacedSlotTransform = slots[slotId].transform;
            Transform parent = slots[slotId].transform.parent;

            Sprite defaultSprite = slots[slotId].GetDefaultAvatar();

            Destroy(slots[slotId].gameObject);
            var go = Instantiate(prefab, parent);
            go.transform.position = replacedSlotTransform.position;
            go.transform.rotation = replacedSlotTransform.rotation;
            go.transform.localScale = Vector3.one;
            slots[slotId] = go.GetComponent<LobbySlot>();

            slots[slotId].SetDefaultAvatar(defaultSprite);
            slots[slotId].SetDefaultName("Player " + (slotId + 1));

            return slots[slotId];
        }

        private GameObject GetPrefabByType(PlayerSlotTypes slotType)
        {
            GameObject prefab = prefab_Empty;

            switch (slotType)
            {
                case PlayerSlotTypes.Empty:
                    break;
                case PlayerSlotTypes.Host:
                    prefab = prefab_Host;
                    break;
                case PlayerSlotTypes.Local:
                    prefab = prefab_Local;
                    break;
                case PlayerSlotTypes.Other:
                    prefab = prefab_Other;
                    break;
                case PlayerSlotTypes.Other_Hostview:
                    prefab = prefab_Hostview;
                    break;
            }

            return prefab;
        }

        public void UpdateSlotColor(int slotId, UnityColor color)
        {
            slots[slotId].SetBorderColor(color);
        }

        public void UpdateSlotPlayer(int slotId, Player player)
        {
            slots[slotId].SetPlayer(player);
        }

        public void UpdateSlotPlayerName(int slotId, string name)
        {
            slots[slotId].SetPlayerName(name);
        }

        public async void UpdateSlotAvatar(int slotId, ulong steamId)
        {
            Image? result = await FPSteamworksManager.instance.GetAvatar(steamId);

            if (!result.HasValue)
            {
                Debug.Log("Error fetching avatar");
                return;
            }

            Image image = result.Value;

            // Create a new Texture2D
            var avatar = new Texture2D((int)image.Width, (int)image.Height, TextureFormat.ARGB32, false);

            // Set filter type, or else its really blury
            avatar.filterMode = FilterMode.Trilinear;

            // Flip image
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var p = image.GetPixel(x, y);
                    avatar.SetPixel(x, (int)image.Height - y, new UnityEngine.Color(p.r / 255.0f, p.g / 255.0f, p.b / 255.0f, p.a / 255.0f));
                }
            }

            avatar.Apply();
            slots[slotId].SetAvatar(avatar);
        }

        public int GetFirstOpenSlotId()
        {
            for (int i = 0; i < 4; i++)
                if (slotFree[i])
                    return i;

            return -1;
        }

        public enum PlayerSlotTypes
        {
            Empty, Host, Local, Other, Other_Hostview
        }
    }
}
