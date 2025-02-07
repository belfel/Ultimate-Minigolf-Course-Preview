using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class AudioVolumeChanger : MonoBehaviour
{
    public GameEvent volumeChanged;

    public FloatVariable masterVolume;
    public FloatVariable volume;

    private UnityAction action;
    private AudioSource audioSource;
    private float initialVolume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("AudioVolumeChanger script added but no AudioSource found");
            return;
        }

        initialVolume = audioSource.volume;

        SetupListener();
    }

    private void Start()
    {
        UpdateVolume();
    }

    private void SetupListener()
    {
        GameEventListener listener = gameObject.AddComponent<GameEventListener>();
        volumeChanged.RegisterListener(listener);
        action += UpdateVolume;
        listener.Event = volumeChanged;
        listener.Response = new UnityEvent();
        listener.Response.AddListener(action);
    }

    public void UpdateVolume()
    {
        float newVolume = masterVolume.value * volume.value * initialVolume;

        audioSource.volume = newVolume;
    }

    public void OverrideInitialVolume(float newVolume)
    {
        initialVolume = newVolume;
        UpdateVolume();
    }
}
