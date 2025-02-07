using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager instance;
    public LobbyData lobbyData;
    public PlayerColors playerColors;
    public GameplaySettingsValues gameplaySettingsValues;

    [SerializeField] private int columnsToFlipAnchor = 21;
    [SerializeField] private GameObject roundColumnPrefab;
    [SerializeField] private GameObject playerAvatarPrefab;
    [SerializeField] private RectTransform scrollViewContent;
    [SerializeField] private GameObject ui;

    [SerializeField] private List<ScoreboardRoundColumn> columns = new List<ScoreboardRoundColumn>();
    [SerializeField] private List<PlayerAvatar> avatars = new List<PlayerAvatar>();
    [SerializeField] private List<TMP_Text> totalScoreTexts = new List<TMP_Text>();
    [SerializeField] private List<int> totalScores = new List<int>();
    [SerializeField] private List<TMP_Text> placeTexts = new List<TMP_Text>();

    private bool controlsDisabled = false;
    private int playerCount;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (controlsDisabled)
            return;

        if (Input.GetKeyDown(KeyCode.Tab))
            ui.SetActive(true);
        if (Input.GetKeyUp(KeyCode.Tab))
            ui.SetActive(false);
    }

    private void FlipContentAnchor()
    {
        scrollViewContent.anchorMin = new Vector2(1f, 0.5f);
        scrollViewContent.anchorMax = new Vector2(1f, 0.5f);
        scrollViewContent.pivot = new Vector2(1f, 0.5f);
    }

    private void SortPlaceTexts()
    {
        int[] scores = totalScores.ToArray();

        for (int i = 0; i < playerCount; i++)
        {
            int max = scores.Max();
            int maxIdx = Array.IndexOf(scores, max);
            placeTexts[maxIdx].text = "#" + (i+1).ToString();
            scores[maxIdx] = -1;
        }

    }

    public void AddColumn(int round, int[] scores)
    {
        var newColumnGO = Instantiate(roundColumnPrefab, parent:transform);

        RectTransform rTransform = newColumnGO.GetComponent<RectTransform>();
        rTransform.parent = scrollViewContent.transform;

        if (columns.Count == columnsToFlipAnchor)
            FlipContentAnchor();

        var newColumn = newColumnGO.GetComponent<ScoreboardRoundColumn>();
        newColumn.SetTexts(round, scores);
        columns.Add(newColumn);

        int i = 0;
        foreach (var scoreText in totalScoreTexts)
        {
            if (scores[i] == -1)
                totalScores[i] += 0;
            else
                totalScores[i] += scores[i];
            scoreText.text = totalScores[i].ToString();
            i++;
        }

        SortPlaceTexts();
    }

    public void InitializeScoreboard()
    {
        playerCount = lobbyData.players.Count;

        for (int i = 0; i < 4 - playerCount; i++)
        {
            Destroy(avatars[playerCount].gameObject);
            avatars.RemoveAt(playerCount);

            Destroy(placeTexts[playerCount].gameObject);
            placeTexts.RemoveAt(playerCount);

            Destroy(totalScoreTexts[playerCount].gameObject);
            totalScoreTexts.RemoveAt(playerCount);
        }

        for (int i = 0; i < playerCount; i++)
        {
            avatars[i].SetOutlineColor(lobbyData.players[i].color);
            if (!gameplaySettingsValues.hideNamesAndAvatars)
                avatars[i].SetAvatar(lobbyData.players[i].steamAvatar);
            totalScores.Add(0);
        }
    }

    public void EnableControls()
    {
        controlsDisabled = false;
    }

    public void DisableControls()
    {
        controlsDisabled = true;
    }

    public void OnShowAvatars()
    {
        playerCount = lobbyData.players.Count;
        for (int i = 0; i < playerCount; i++)
            avatars[i].SetAvatar(lobbyData.players[i].steamAvatar);
    }

    public void OnHideAvatars()
    {
        playerCount = lobbyData.players.Count;
        for (int i = 0; i < playerCount; i++)
        {
            avatars[i].SetAvatarToDefault();
        }
    }
}
