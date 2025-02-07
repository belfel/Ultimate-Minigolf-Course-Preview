using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplaySettings : MonoBehaviour
{
    public GameplaySettingsValues values;

    public GameEvent OnShowNamesAndAvatars;
    public GameEvent OnHideNamesAndAvatars;

    public GameEvent OnShowChat;
    public GameEvent OnHideChat;

    public GameEvent OnShowControls;
    public GameEvent OnHideControls;

    [SerializeField] private Toggle hideNamesAndAvatars;
    [SerializeField] private Toggle hideChat;
    [SerializeField] private Toggle hideControls;

    private void Start()
    {
        hideNamesAndAvatars.isOn = values.hideNamesAndAvatars;
        hideChat.isOn = values.hideChat;
        hideControls.isOn = values.hideControls;
    }

    public void OnToggleHideNamesAndAvatars()
    {
        bool isOn = hideNamesAndAvatars.isOn;
        values.hideNamesAndAvatars = isOn;

        if (isOn)
            OnHideNamesAndAvatars.Raise();
        else OnShowNamesAndAvatars.Raise();
    }

    public void OnToggleChat()
    {
        bool isOn = hideChat.isOn;
        values.hideChat = isOn;

        if (isOn)
            OnHideChat.Raise();
        else OnShowChat.Raise();
    }

    public void OnToggleControls()
    {
        bool isOn = hideControls.isOn;
        values.hideControls = isOn;

        if (isOn)
            OnHideControls.Raise();
        else OnShowControls.Raise();
    }
}
