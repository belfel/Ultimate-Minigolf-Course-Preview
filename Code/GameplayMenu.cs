using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMenu : MonoBehaviour
{
    public GameEvent gameplayMenuOpened;
    public GameEvent gameplayMenuClosed;

    [SerializeField] private GameObject confirmationBoxPrefab;
    [SerializeField] private GameObject settingsPrefab;
    [SerializeField] private GameObject ui;

    private GameObject confirmationBox;
    private bool blockKeybind = false;

    private void Awake()
    {
        if (ui.activeInHierarchy)
            HideMenu();
    }

    private void Update()
    {
        if (!blockKeybind && Input.GetKeyDown(KeyCode.Escape))
        {
            if (ui.activeInHierarchy)
                HideMenu();
            else ShowMenu();
        }
    }

    private void HideMenu()
    {
        gameplayMenuClosed.Raise();
        ui.SetActive(false);
    }

    private void ShowMenu()
    {
        gameplayMenuOpened.Raise();
        ui.SetActive(true);
    }

    public void OnClose()
    {
        HideMenu();
    }

    public void OnSettings()
    {
        Instantiate(settingsPrefab);
        ui.SetActive(false);
        blockKeybind = true;
    }

    public void OnExit()
    {
        blockKeybind = true;
        confirmationBox = Instantiate(confirmationBoxPrefab);
        ConfirmationBox cb = confirmationBox.GetComponent<ConfirmationBox>();
        cb.onConfirm = () => OnConfirmExit();
        cb.onCancel = () => blockKeybind = false;
    }

    private void OnConfirmExit()
    {
        if (NetworkManager.Singleton.IsHost)
            NetworkMonitor.Instance.HostQuitInformOthersRpc(new RpcParams());
        NetworkManager.Singleton.Shutdown(); 
        blockKeybind = false; 
        SceneManager.LoadScene("MainMenu");
    }

    public void LockKeybind()
    {
        blockKeybind = true;
    }

    public void UnlockKeybind()
    {
        blockKeybind = false;
    }
}
