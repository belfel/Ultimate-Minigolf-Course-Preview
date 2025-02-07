using TMPro;
using UnityEngine;

public class ChatToggle : MonoBehaviour
{
    public Keybinds keybinds;

    public GameEvent chatOpened;
    public GameEvent chatClosed;

    [SerializeField] private GameObject chatUI;
    [SerializeField] private TMP_InputField inputField;

    private bool keybindBlocked = false;

    private void Update()
    {
        if (chatUI.activeInHierarchy && Input.GetKeyDown(KeyCode.Escape))
        {
            // close chat with tiny delay to avoid opening gameplay menu on the same frame
            Invoke("CloseChat", 0.1f);
            return;
        }

        if (keybindBlocked)
            return;

        if (Input.GetKeyDown(keybinds.openChat.key))
        {
            if (!chatUI.activeInHierarchy)
                OpenChat();
        }    
    }

    public void BlockKeybind()
    {
        keybindBlocked = true;
    }

    public void UnlockKeybind()
    {
        keybindBlocked = false;
    }

    public void OpenChat()
    {
        chatUI.SetActive(true);
        inputField.ActivateInputField();
        chatOpened.Raise();
    }

    public void CloseChat()
    {
        chatUI.SetActive(false);
        chatClosed.Raise();
    }
}
