using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorWindowManager : MonoBehaviour
{
    public static ErrorWindowManager Instance;

    [SerializeField] private GameObject errorWindowPrefab;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);

         DontDestroyOnLoad(gameObject);
    }

    private void InstantiateErrorWindow()
    {
        if (errorWindowPrefab == null)
        {
            Debug.LogError("Prefab not set");
            return;
        }

        var errorWindow = Instantiate(errorWindowPrefab);
    }

    private void InstantiateErrorWindow(string title, string description)
    {
        if (errorWindowPrefab == null)
        {
            Debug.LogError("Prefab not set");
            return;
        }

        var errorWindow = Instantiate(errorWindowPrefab);
        ErrorMessage errorMessage = errorWindow.GetComponent<ErrorMessage>();
        if (errorMessage == null)
            Debug.LogError("Prefab missing ErrorMessage component");
        else
        {
            errorMessage.SetTitle(title);
            errorMessage.SetDescription(description);
        }    
    }

    public void OnHostFailed()
    {
        InstantiateErrorWindow("failed to host a lobby", "error occured when trying to start a new lobby.");
    }

    public void OnJoinFailedFull()
    {
        InstantiateErrorWindow("failed to join the lobby", "the lobby you are trying to join is full.");
    }

    public void OnJoinFailedNotFound()
    {
        InstantiateErrorWindow("failed to join the lobby", "the lobby you are trying to join doesn't exist or is no longer joinable.");
    }

    public void OnJoinFailedGeneric()
    {
        InstantiateErrorWindow("failed to join the lobby", "an unknown error occured.");
    }

    public void OnLocalClientLostConnection()
    {
        InstantiateErrorWindow("lost connection", "check your internet connection and steam status.");
    }

    public void OnServerStopped()
    {
        InstantiateErrorWindow("host closed connection", "lobby host timed out or closed the game.");
    }

    public void OnKicked()
    {
        InstantiateErrorWindow("you've been kicked", "the host kicked you from the lobby.");
    }

    public void OnBanned()
    {
        InstantiateErrorWindow("you've been banned", "the host banned you from the lobby.");
    }

    public void OnPreviouslyBanned()
    {
        InstantiateErrorWindow("banned from lobby", "you are banned from this lobby.");
    }
}
