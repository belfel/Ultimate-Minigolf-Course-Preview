using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Chat : NetworkBehaviour
{
    public LobbyData lobbyData;
    public ChatMessages chatMessages;
    public GameplaySettingsValues gameplaySettings;

    public GameEvent chatNewMessage;

    [SerializeField] private Transform messageParentObject;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject scrollView;

    [SerializeField] private TMP_InputField inputField;

    [SerializeField] int characterLimit = 256;

    private ChatToggle chatToggle;
    private Dictionary<GameObject, ChatMessage> messageObjectPairs = new Dictionary<GameObject, ChatMessage>();

    private void Awake()
    {
        inputField.characterLimit = characterLimit;

        if (gameplaySettings.hideChat)
            scrollView.SetActive(false);

        chatToggle = gameObject.GetComponent<ChatToggle>();
    }

    public override void OnNetworkSpawn()
    {
        foreach (ChatMessage msg in chatMessages.messages)
            ShowMessage(msg);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SendChatMessage();
    }

    public void SendChatMessage()
    {
        string msg = inputField.text;
        if (msg == "")
        {
            if (chatToggle)
                chatToggle.CloseChat();
            else return;
        }
        msg = RemoveRichText(msg);

        Player sender = lobbyData.GetPlayerByClientId(NetworkManager.LocalClientId);

        ChatMessage chatMessage = new ChatMessage();
        chatMessage.senderName = sender.name;
        chatMessage.senderSanitizedName = sender.sanitizedName;
        chatMessage.senderColor = sender.color;
        chatMessage.message = msg;

        inputField.text = "";

        if (chatToggle)
            chatToggle.CloseChat();

        SendChatMessageRpc(chatMessage);
    }

    [Rpc(SendTo.Everyone)]
    public void SendChatMessageRpc(ChatMessage chatMessage)
    {
        chatMessages.messages.Add(chatMessage);
        ShowMessage(chatMessage);

        if (chatNewMessage != null)
            chatNewMessage.Raise();
    }

    private void ShowMessage(ChatMessage msg)
    {
        var msgGO = Instantiate(messagePrefab, messageParentObject);
        messageObjectPairs.Add(msgGO, msg);

        string senderName = msg.senderName;
        if (gameplaySettings.hideNamesAndAvatars)
            senderName = msg.senderSanitizedName;

        msgGO.GetComponent<TMP_Text>().text = $"<color={Chat.ColorToHex(msg.senderColor)}>{senderName}:</color> {msg.message}";
    }

    public void SanitizePlayerNames()
    {
        foreach (var msg in messageObjectPairs)
        {
            TMP_Text tmp_text = msg.Key.GetComponent<TMP_Text>();
            tmp_text.text = $"<color={Chat.ColorToHex(msg.Value.senderColor)}>{msg.Value.senderSanitizedName}:</color> {msg.Value.message}";
        }
    }

    public void DesanitizePlayerNames()
    {
        foreach (var msg in messageObjectPairs)
        {
            TMP_Text tmp_text = msg.Key.GetComponent<TMP_Text>();
            tmp_text.text = $"<color={Chat.ColorToHex(msg.Value.senderColor)}>{msg.Value.senderName}:</color> {msg.Value.message}";
        }
    }

    public static string ColorToHex(Color color)
    {
        int r = Mathf.Clamp(Mathf.RoundToInt(color.r * 255), 0, 255);
        int g = Mathf.Clamp(Mathf.RoundToInt(color.g * 255), 0, 255);
        int b = Mathf.Clamp(Mathf.RoundToInt(color.b * 255), 0, 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static string RemoveRichText(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
