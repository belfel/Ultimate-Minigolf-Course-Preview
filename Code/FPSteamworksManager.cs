using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;
using System.Linq;
using Unity.Netcode;

[RequireComponent(typeof(FacepunchTransport))]
public class FPSteamworksManager : MonoBehaviour
{
    public static FPSteamworksManager instance;
    [SerializeField] private static uint AppID = 480;

    private FacepunchTransport transport;
    public NetworkingEvents events;
    private Lobby? currentLobby;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);

        transport = GetComponent<FacepunchTransport>();

        SetupCallbacks();
    }

    private void Start()
    {
        var status = TryConnectToSteam();
        if (!status)
            events.SteamConnectionAttemptFailed.Raise();
        else Debug.Log("Connected to steam");
    }

    public async void CreateLobby()
    {
        var lobby = await TryCreateLobby();

        if (!lobby.HasValue)
        {
            events.LobbyHostFailed.Raise();
            return;
        }

        lobby.Value.SetPrivate();
        lobby.Value.SetData("game", "UGC");
        currentLobby = lobby.Value;

        events.LobbyHostSuccess.Raise();
    }

    public async void TryJoinLobby(Lobby lobby)
    {
        var status = await lobby.Join();
        if (status == RoomEnter.Success)
        {
            currentLobby = lobby;
            transport.targetSteamId = lobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
            events.JoinedLobby.Raise();
            return;
        }

        else if (status == RoomEnter.DoesntExist)
            events.LobbyJoinFailed_NotFound.Raise();

        else if (status == RoomEnter.Full)
            events.LobbyJoinFailed_Full.Raise();

        else
            events.LobbyJoinFailed_Other.Raise();
    }

    public void LeaveLobby()
    {
        if (!currentLobby.HasValue)
        {
            Debug.Log("Attempting to leave lobby but not currently in any lobby");
            return;
        }

        try
        {
            foreach (var member in currentLobby.Value.Members)
                SteamNetworking.CloseP2PSessionWithUser(member.Id);
            currentLobby.Value.Leave();
        }

        catch
        {
            Debug.Log("Error while attempting to leave current lobby");
        }
    }

    public async Task<Image?> GetAvatar(ulong steamId)
    {
        try
        {
            return await SteamFriends.GetLargeAvatarAsync(steamId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }

    public Lobby? GetCurrentLobby()
    {
        return currentLobby;
    }

    public List<Friend> GetLobbyMembers()
    {
        return currentLobby.Value.Members.ToList();
    }

    public void SetLobbyType(LobbyType type)
    {
        switch (type)
        {
            case LobbyType.Private:
                currentLobby.Value.SetPrivate();
                currentLobby.Value.SetJoinable(false);
                break;
            case LobbyType.Public:
                currentLobby.Value.SetPublic();
                break;
            case LobbyType.FriendsOnly:
                currentLobby.Value.SetFriendsOnly();
                currentLobby.Value.SetJoinable(true);
                break;
        }
    }

    private async void OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamId)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to join lobby: " + joinedLobby.ToString()) ;
        }
        else
        {
            currentLobby = _lobby;
            transport.targetSteamId = _steamId;
            NetworkManager.Singleton.StartClient();
            Debug.Log("Joined Lobby");
        }
    }

    private bool TryConnectToSteam()
    {
        SteamClient.Init(AppID);
        if (!SteamClient.IsValid)
            return false;
        else return true;
    }

    private async Task<Lobby?> TryCreateLobby()
    {
        try
        {
            var lobby = await SteamMatchmaking.CreateLobbyAsync(4);
            if (!lobby.HasValue)
            {
                Debug.Log("Lobby created but not correctly instantiated");
                throw new Exception();
            }

            return lobby;
        }

        catch (Exception exception)
        {
            Debug.Log(exception.ToString());
            return null;
        }
    }

    private void SetupCallbacks()
    {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    #region callbacks

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        
    }

    #endregion

    public enum LobbyType
    {
        Private, FriendsOnly, Public
    }

}

[System.Serializable]
public class NetworkingEvents
{
    public GameEvent SteamConnectionAttemptFailed;
    public GameEvent LobbyJoinFailed_Other;
    public GameEvent LobbyJoinFailed_NotFound;
    public GameEvent LobbyJoinFailed_Full;
    public GameEvent LobbyHostSuccess;
    public GameEvent LobbyHostFailed;
    public GameEvent JoiningLobby;
    public GameEvent JoinedLobby;
    public GameEvent LeftLobby;
}