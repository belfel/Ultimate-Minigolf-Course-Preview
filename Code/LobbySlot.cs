using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySlot : MonoBehaviour
{
    public GameplaySettingsValues GameplaySettingsValues;

    [SerializeField] private GameObject confirmationBoxPrefab;

    [SerializeField] private int minPingForRed = 200;
    [SerializeField] private int minPingForYellow = 100;
    [SerializeField] private int pingUpdateRate = 2;

    [SerializeField] private Image displayedAvatar;
    [SerializeField] private Sprite defaultAvatar;
    [SerializeField] private Image avatarBorder;
    [SerializeField] private Image pingKnob;
    [SerializeField] private TMP_Text displayedName;
    [SerializeField] private string defaultName;
    [SerializeField] private TMP_Text playerPing;
    [SerializeField] private Button kick;
    [SerializeField] private Button ban;

    private Player player;
    private ulong clientId;
    private Action kickAction;
    private Action banAction;
    private GameObject confirmationBox;

    private void Awake()
    {
        displayedAvatar.sprite = defaultAvatar;
        if (displayedName)
            displayedName.text = defaultName;
    }

    private void Start()
    {
        InvokeRepeating("UpdatePing", 0f, pingUpdateRate);
    }

    private void UpdatePing()
    {
        if (player != null)
            SetPing(player.GetAveragePing());
    }

    public void SetAvatar(Sprite sprite)
    {
        displayedAvatar.sprite = sprite;
    }

    public void SetAvatar(Texture2D texture)
    {
        Sprite newSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        player.steamAvatar = newSprite;

        if (GameplaySettingsValues.hideNamesAndAvatars)
        {
            displayedAvatar.sprite = defaultAvatar;
        }

        else
        {
            displayedAvatar.sprite = newSprite;
        }
    }

    public void SetPing(int pingMs)
    {
        if (pingMs >= minPingForRed)
            pingKnob.color = Color.red;
        else if (pingMs >= minPingForYellow)
            pingKnob.color = Color.yellow;
        else pingKnob.color = Color.green;

        playerPing.text = pingMs.ToString() + " ms";
    }

    public void SetPlayerName(string name)
    {
        displayedName.text = name;
    }

    public void SetBorderColor(Color color)
    {
        avatarBorder.color = color;
    }

    public void SetOnKickAction(Action action)
    {
        kickAction = action;
        kick.onClick.AddListener(OnKick);
    }

    public void SetOnBanAction(Action action)
    {
        banAction = action;
        ban.onClick.AddListener(OnBan);
    }

    public void SetClientId(ulong id)
    {
        clientId = id;
    }

    public ulong GetClientId()
    {
        return clientId;
    }

    public void SetPlayer(Player _player)
    {
        player = _player;
    }

    private void OnKick()
    {
        if (confirmationBox != null)
            return;
        
        confirmationBox = Instantiate(confirmationBoxPrefab);
        confirmationBox.GetComponent<ConfirmationBox>().onConfirm = kickAction;
    }

    private void OnBan()
    {
        if (confirmationBox != null)
            return;

        confirmationBox = Instantiate(confirmationBoxPrefab);
        confirmationBox.GetComponent<ConfirmationBox>().onConfirm = banAction;
    }

    public Sprite GetDefaultAvatar()
    {
        return defaultAvatar;
    }

    public void SetDefaultAvatar(Sprite sprite)
    {
        defaultAvatar = sprite;
    }

    public void SetDefaultName(string name)
    {
        defaultName = name;
    }
}
