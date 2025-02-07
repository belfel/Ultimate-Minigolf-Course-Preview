using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatLite : MonoBehaviour
{
    public ChatMessages chatMessages;
    public GameplaySettingsValues gameplaySettings;

    [SerializeField] private Transform messageParentObject;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private GameObject scrollView;

    [SerializeField] private int messagesToShow = 5;
    [SerializeField] private float fadeDelay = 4f;

    [SerializeField] private float fadeDuration = 1f;

    private Dictionary<GameObject, ChatMessage> messageObjectPairs = new Dictionary<GameObject, ChatMessage>();

    private Coroutine fadeRoutine;

    private bool isChatOpen = false;
    private bool isLiteChatActive = false;
    private float activeTime = 0f;
    private float baseBackgroundAlpha;

    private void Awake()
    {
        if (gameplaySettings.hideChat)
            scrollView.SetActive(false);

        baseBackgroundAlpha = scrollView.GetComponent<Image>().color.a;
    }

    private void Update()
    {
        if (isLiteChatActive)
        {
            activeTime += Time.deltaTime;
            if (activeTime > fadeDelay)
            {
                isLiteChatActive = false;
                StopCoroutine(FadeRoutine());
                fadeRoutine = StartCoroutine(FadeRoutine());
            }
        }
    }

    private IEnumerator FadeRoutine()
    {
        Image background = scrollView.GetComponent<Image>();
        Color bgColor = background.color;
        float fadeTime = fadeDuration;

        while (fadeTime > 0f)
        {
            background.color = new Color(bgColor.r, bgColor.g, bgColor.b, fadeTime * baseBackgroundAlpha / fadeDuration);
            fadeTime -= Time.deltaTime;
            yield return null;
        }

        background.color = new Color(bgColor.r, bgColor.g, bgColor.b, baseBackgroundAlpha);
        Hide();
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

    private void ShowRecentMessages()
    {
        int numOfMessages = Mathf.Min(chatMessages.messages.Count, messagesToShow);
        int idx = chatMessages.messages.Count - numOfMessages;

        for (int i = 0; i < numOfMessages; i++)
        {
            ShowMessage(chatMessages.messages[idx]);
            idx++;
        }
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

    private void Hide()
    {
        scrollView.SetActive(false);
    }

    private void Show()
    {
        scrollView.SetActive(true);
    }

    public void OnNewMessage()
    {
        activeTime = 0f;
        isLiteChatActive = true;

        foreach (var message in messageObjectPairs)
            Destroy(message.Key);
        messageObjectPairs.Clear();

        if (!isChatOpen)
            Show();

        ShowRecentMessages();
    }

    public void OnChatClosed()
    {
        isChatOpen = false;
        if (isLiteChatActive)
            Show();

    }
    public void OnChatOpened()
    {
        isChatOpen = true;
        Hide();
    }
}
