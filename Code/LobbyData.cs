using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class LobbyData : ScriptableObject
{
    public List<Player> players = new List<Player>();

    public int pointsGoal = 30;
    [SerializeField] private int pointsGoalDefault = 30;
    public int gameplayTime = 60;
    [SerializeField] private int gameplayTimeDefault = 60;
    public int buildingTime = 30;
    [SerializeField] private int buildingTimeDefault = 30;

    public void ClearData()
    {
        players.Clear();

        pointsGoal = pointsGoalDefault;
        gameplayTime = gameplayTimeDefault;
        buildingTime = buildingTimeDefault;
    }

    public Color GetClientColorById(ulong clientId)
    {
        foreach (var player in players)
        {
            if (player.id == clientId)
                return player.color;
        }

        return Color.white;
    }

    public int GetSlotIdByClientId(ulong clientId)
    {
        foreach (var player in players)
        {
            if (player.id == clientId)
                return player.slotId;
        }

        return -1;
    }

    public Player GetPlayerByClientId(ulong clientId)
    {
        foreach (var player in players)
            if (player.id == clientId)
                return player;

        return null;
    }

    public Player AddPlayer(ulong clientId, int slotId, Color color)
    {
        Player newPlayer = new Player();
        newPlayer.name = $"Player {slotId + 1}";
        newPlayer.sanitizedName = $"Player {slotId + 1}";
        newPlayer.id = clientId;
        newPlayer.slotId = slotId;
        newPlayer.color = color;
        players.Add(newPlayer);
        return newPlayer;
    }

    public int PlayerCount()
    {
        return players.Count;
    }
}

[System.Serializable]
public class Player
{
    public ulong id;
    public ulong steamId;
    public int slotId;
    public Queue<int> pings = new Queue<int>(5);
    public string name;
    public string sanitizedName;
    public Sprite steamAvatar;
    public Color color;

    public void AddPingValue(int pingMs)
    {
        if (pings.Count == 5)
            pings.Dequeue();
        pings.Enqueue(pingMs);
    }

    public int GetAveragePing()
    {
        if (pings.Count == 0)
            return 0;
        return (int)pings.Average();
    }
}
