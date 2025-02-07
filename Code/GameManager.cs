using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;

    public GameManagerEvents events;
    public GameManagerTimeVariables timeVars;
    public GameManagerScoring scoring;

    public LobbyData lobbyData;
    public MapSettings mapSettings;
    public PlayerColors playerColors;

    [SerializeField] private NetworkObject ballPrefab;

    [SerializeField] private int round = 1;
    
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> podiumSpawnPoints = new List<Transform>();

    private GameState gameState;
    private List<NetworkObject> playerBalls = new List<NetworkObject>();
    private List<ulong> playersEndedIntro = new List<ulong>();
    private List<ulong> playersFinished = new List<ulong>();
    private List<ulong> playersFinishedBuilding = new List<ulong>();

    private NetworkVariable<float> networkTimer = new NetworkVariable<float>(60f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject); 
    }

    private void Start()
    {
        gameState = GameState.Intro;
        events.introStarted.Raise();
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            InitializeLists();
        }

        scoring.pointsGoal = lobbyData.pointsGoal;
        timeVars.gameplayTime = lobbyData.gameplayTime;
        timeVars.buildingTime = lobbyData.buildingTime;

        timeVars.timeLeft.SetValue(timeVars.gameplayTime);
        networkTimer.OnValueChanged += (float previousValue, float newValue) =>
        {
            timeVars.timeLeft.SetValue(newValue);
        };
    }

    private void Update()
    {
        
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback += PlayerConnected;
    }

    private void OnDisable()
    {
        if (!NetworkManager.Singleton)
            return;

        NetworkManager.Singleton.OnClientDisconnectCallback -= PlayerDisconnected;
        NetworkManager.Singleton.OnClientConnectedCallback -= PlayerConnected;
    }

    private void PlayerConnected(ulong clientId)
    {

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

    [Rpc(SendTo.Everyone)]
    private void PlayerDisconnectedRpc(ulong clientId, RpcParams rpcParams)
    {
        foreach (var player in lobbyData.players)
            if (player.id == clientId)
            {
                // TODO add reconnect support - mark as disconnected rather than remove
                lobbyData.players.Remove(player);
                break;
            }

        if (!IsHost)
            return;

        RemovePlayerFromLists(clientId);
        RecheckConditions();
    }

    private void RemovePlayerFromLists(ulong clientId)
    {
        foreach (var player in playersEndedIntro)
            if (player == clientId)
            {
                playersEndedIntro.Remove(player);
                break;
            }

        foreach (var ball in playerBalls)
            if (ball.OwnerClientId == clientId)
            {
                playerBalls.Remove(ball);
                break;
            }

        foreach (var player in playersFinished)
            if (player == clientId)
            {
                playersFinished.Remove(player);
                break;
            }

        foreach (var player in playersFinishedBuilding)
            if (player == clientId)
            {
                playersFinishedBuilding.Remove(player);
                break;
            }
    }
    
    private void RecheckConditions()
    {
        events.recheckConditions.Raise();
        if (gameState == GameState.BallPhase)
            EveryoneFinishedCheck();
        else if (gameState == GameState.Intro)
            CheckEveryoneSkippedIntro();
        else if (gameState == GameState.BuildingPhase)
            CheckEveryoneFinishedBuilding();

    }

    private void InitializeLists()
    {
        playersEndedIntro.Clear();
        playersFinishedBuilding.Clear();
        playersFinished.Clear();
    }

    private IEnumerator TimerRoutine()
    {
        while (networkTimer.Value > 0f)
        {
            networkTimer.Value -= Time.deltaTime;

            yield return null;
        }
        timeVars.onTimerElapsed();
    }

    private void StartTimer(float time)
    {
        networkTimer.Value = time;
        timeVars.currentPhaseDuration.SetValue(time);

        if (timeVars.timerCoroutine != null)
            StopCoroutine(timeVars.timerCoroutine);
        timeVars.timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private void SpawnBalls(List<Transform> spawns)
    {
        if (!IsHost)
            return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;

        playerBalls.Clear();
        int i = 0;
        foreach (var client in clients)
        {
            ulong clientId = client.ClientId;
            Vector3 spawnPosition = new Vector3(spawns[i].position.x, spawns[i].position.y, spawns[i].position.z);

            var ball = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(ballPrefab, clientId, false, position: spawnPosition);
            var ballInit = ball.GetComponent<BallInitializer>();

            ballInit.color.Value = lobbyData.GetClientColorById(clientId);

            playerBalls.Add(ball);
            i++;
        }
    }

    private void DespawnBalls()
    {
        if (!IsHost)
            return;

        foreach(var ball in playerBalls)
        {
            if (ball != null && ball.IsSpawned)
                ball.Despawn();
        }
    }

    private void OnCountdownEnd()
    {
        if (!IsHost)
            return;

        GameStateChangedRpc((int)GameState.BallPhase, new RpcParams());

        timeVars.onTimerElapsed = () =>
        {
            InvokeBallPhaseTimeRanOutEventRpc(new RpcParams());
            DespawnBalls();
        };

        StartTimer(timeVars.gameplayTime);
    }

    public void OnBallPhaseFinished()
    {
        if (!IsHost || gameState != GameState.BallPhase)
            return;

        GameStateChangedRpc((int)GameState.ScoreboardPhase, new RpcParams());
    }

    private void OnCountdownStarted()
    {
        if (!IsHost)
            return;

        playersFinished.Clear();
        playersFinishedBuilding.Clear();

        SpawnBalls(spawnPoints);
        Invoke("OnCountdownEnd", timeVars.countdownDuration.value);
    }

    public void OnScoreboardStarted()
    {
        if (!IsHost) 
            return;

        timeVars.onTimerElapsed = () =>
        {
            if (scoring.goalReached)
                GameStateChangedRpc((int)GameState.Postmatch, new RpcParams());
            else
                GameStateChangedRpc((int)GameState.ObjectSelection, new RpcParams());
        };

        StartTimer(timeVars.scoreboardTime);
    }

    public void OnLocalPlayerFinished()
    {
        PlayerFinishedRpc(new RpcParams());
    }

    public void OnObjectSelectionStarted()
    {
        if (!IsHost)
            return;

        timeVars.onTimerElapsed = () =>
        {
            events.objectSelectionTimeRanOut.Raise();
        };

        StartTimer(timeVars.objectSelectionTime);
    }

    public void OnObjectSelectionFinished()
    {
        if (!IsHost)
            return;

        timeVars.onTimerElapsed = () =>
        {
            GameStateChangedRpc((int)GameState.BuildingPhase, new RpcParams());
        };

        StartTimer(3f);
    }

    public void OnBuildingPhaseStarted()
    {
        if (!IsHost)
            return;

        timeVars.onTimerElapsed = () =>
        {
            events.buildingPhaseTimeRanOut.Raise();
        };

        StartTimer(timeVars.buildingTime);
    }

    public void OnPostmatchStarted()
    {
        if (!IsHost)
            return;

        SpawnBalls(podiumSpawnPoints);

        timeVars.onTimerElapsed = () =>
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Failed to load {"MainMenu"} " +
                        $"with a {nameof(SceneEventProgressStatus)}: {status}");
            }
        };

        StartTimer(timeVars.postmatchTime);
    }

    [Rpc(SendTo.Server)]
    public void ClientEndedIntroRpc(RpcParams rpcParams)
    {
        playersEndedIntro.Add(rpcParams.Receive.SenderClientId);
        CheckEveryoneSkippedIntro();
    }

    private bool CheckEveryoneSkippedIntro()
    {
        if (!IsHost)
            return false;

        if (playersEndedIntro.Count == lobbyData.PlayerCount())
        {
            GameStateChangedRpc((int)GameState.Countdown, new RpcParams());
            return true;
        }

        else 
            return false;
    }

    [Rpc(SendTo.Server)]
    public void PlayerFinishedRpc(RpcParams rpcParams)
    {
        var id = rpcParams.Receive.SenderClientId;
        foreach (var ball in playerBalls)
        {
            if (ball.OwnerClientId != id)
                continue;

            playersFinished.Add(id);
        }

        if (!EveryoneFinishedCheck())
        {
            InvokePlayerFinishedRpc(id, new RpcParams());
        }         
    }

    private bool EveryoneFinishedCheck()
    {
        if (playersFinished.Count < lobbyData.PlayerCount())
        {
            return false;
        }
        else
        {
            OnEveryoneFinished();
            return true;
        }
    }

    private void OnEveryoneFinished()
    {
        if (gameState != GameState.BallPhase || timeVars.timerCoroutine == null)
            return;

        AllPlayersFinishedRpc(new RpcParams());
        StopCoroutine(timeVars.timerCoroutine);
        DespawnBalls();
    }

    [Rpc(SendTo.Everyone)]
    public void AllPlayersFinishedRpc(RpcParams rpcParams)
    {
        events.allPlayersFinished.Raise();
    }

    [Rpc(SendTo.Server)]
    public void LocalPlayerFinishedBuildingRpc(RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        if (playersFinishedBuilding.Contains(senderId))
            return;

        playersFinishedBuilding.Add(senderId);
        CheckEveryoneFinishedBuilding();
    }

    private bool CheckEveryoneFinishedBuilding()
    {
        if (playersFinishedBuilding.Count == NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            GameStateChangedRpc((int)GameState.Countdown, new RpcParams());
            return true;
        }
        else return false;
    }

    [Rpc(SendTo.Everyone)]
    public void AddScoreboardColumnRpc(int round, int score1, int score2, int score3, int score4, RpcParams rpcParams)
    {
        int[] columnScores = { score1, score2, score3, score4 };
        ScoreboardManager.instance.AddColumn(round, columnScores);
    }

    [Rpc(SendTo.Everyone)]
    public void GameStateChangedRpc(int newState, RpcParams rpcParams)
    {
        RaiseEventPhaseEnded(gameState);
        gameState = (GameState)newState;
        RaiseEventPhaseStarted(gameState);
    }

    [Rpc(SendTo.Everyone)]
    public void InvokeBallPhaseTimeRanOutEventRpc(RpcParams rpcParams)
    {
        events.ballPhaseTimeRanOut.Raise();
    }

    [Rpc(SendTo.Everyone)]
    public void InvokePlayerFinishedRpc(ulong finishingPlayerId, RpcParams rpcParams)
    {
        if (finishingPlayerId != NetworkManager.Singleton.LocalClientId)
            events.playerFinished.Raise();
    }

    public void OnHostStopped()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }

    #region Phase transition events
    private void RaiseEventPhaseStarted(GameState newState)
    {
        switch (newState)
        {
            case GameState.Intro:
                events.introStarted.Raise();
                break;
            case GameState.Countdown:
                events.countdownStarted.Raise();
                OnCountdownStarted();
                break;
            case GameState.BallPhase:
                events.ballPhaseStarted.Raise();
                break;
            case GameState.ScoreboardPhase:
                events.scoreboardStarted.Raise();
                OnScoreboardStarted();
                break;
            case GameState.ObjectSelection:
                events.objectSelectionStarted.Raise();
                OnObjectSelectionStarted();
                break;
            case GameState.BuildingPhase:
                events.buildingPhaseStarted.Raise();
                OnBuildingPhaseStarted();
                break;
            case GameState.Postmatch:
                events.postmatchStarted.Raise();
                OnPostmatchStarted();
                break;
        }

        events.anyPhaseStarted.Raise();
    }

    private void RaiseEventPhaseEnded(GameState endingState)
    {
        timeVars.onTimerElapsed = null;

        switch (endingState)
        {
            case GameState.Intro:
                events.introEnded.Raise();
                break;
            case GameState.Countdown:
                events.countdownEnded.Raise();
                break;
            case GameState.BallPhase:
                events.ballPhaseEnded.Raise();
                if (IsHost)
                    scoring.UpdateScores(playersFinished, round, lobbyData.PlayerCount(), lobbyData);
                round++;
                break;
            case GameState.ScoreboardPhase:
                events.scoreboardEnded.Raise();
                break;
            case GameState.ObjectSelection:
                events.objectSelectionEnded.Raise();
                break;
            case GameState.BuildingPhase:
                events.buildingPhaseEnded.Raise();
                break;
            case GameState.Postmatch:
                events.postmatchEnded.Raise();
                break;
        }

        events.anyPhaseEnded.Raise();
    }
    #endregion

    private enum GameState
    {
        Intro, Countdown, BallPhase, ScoreboardPhase, ObjectSelection, BuildingPhase, Postmatch
    }
}

[System.Serializable]
public class GameManagerEvents
{
    public GameEvent introStarted;
    public GameEvent introEnded;
    public GameEvent countdownStarted;
    public GameEvent countdownEnded;
    public GameEvent ballPhaseStarted;
    public GameEvent ballPhaseEnded;
    public GameEvent ballPhaseTimeRanOut;
    public GameEvent scoreboardStarted;
    public GameEvent scoreboardEnded;
    public GameEvent objectSelectionStarted;
    public GameEvent objectSelectionTimeRanOut;
    public GameEvent objectSelectionEnded;
    public GameEvent buildingPhaseStarted;
    public GameEvent buildingPhaseEnded;
    public GameEvent buildingPhaseTimeRanOut;
    public GameEvent postmatchStarted;
    public GameEvent postmatchEnded;
    public GameEvent anyPhaseStarted;
    public GameEvent anyPhaseEnded;
    public GameEvent playerFinished;
    public GameEvent allPlayersFinished;
    public GameEvent recheckConditions;
}

[System.Serializable]
public class GameManagerTimeVariables
{
    public Action onTimerElapsed;
    public Coroutine timerCoroutine;

    public FloatVariable timeLeft;
    public FloatVariable currentPhaseDuration;
    public FloatVariable countdownDuration;
    public float gameplayTime = 60f;
    public float scoreboardTime = 5f;
    public float objectSelectionTime = 7f;
    public float buildingTime = 30f;
    public float postmatchTime = 15f;
}

[System.Serializable]
public class GameManagerScoring
{
    public int pointsForFinishing = 3;
    public int pointsForFirst = 5;
    public int pointsGoal = 30;

    public bool goalReached;
    public int[] totalScores = { 0, 0, 0, 0 };

    public void UpdateScores(List<ulong> playersWhoFinished, int round, int playerCount, LobbyData lobbyData)
    {
        int[] scores = { -1, -1, -1, -1 };
        for (int i = 0; i < playerCount; i++)
            scores[i] = 0;

        // no points if either everyone finished or no one finished
        if (playersWhoFinished.Count == playerCount || playersWhoFinished.Count == 0)
            for (int i = 0; i < playerCount; i++)
                scores[i] = 0;

        else
        {
            int slotId = lobbyData.GetSlotIdByClientId(playersWhoFinished[0]);
            scores[slotId] = pointsForFirst;

            for (int i = 1; i < playersWhoFinished.Count; i++)
            {
                slotId = lobbyData.GetSlotIdByClientId(playersWhoFinished[i]);
                scores[slotId] = pointsForFinishing;
            }
        }

        for (int i = 0; i < playerCount; i++)
        {
            totalScores[i] += scores[i];
            if (totalScores[i] >= pointsGoal)
                goalReached = true;
        }

        GameManager.instance.AddScoreboardColumnRpc(round, scores[0], scores[1], scores[2], scores[3], new RpcParams());
    }
}
