using System.Collections.Generic;
using UnityEngine;

public class AudioClipRandomizer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> clips = new List<AudioClip>();

    int lastPlayedClipIndex = -1;

    public void PlayRandom()
    {
        if (clips.Count == 0)
            return;

        int randomIndex = Random.Range(0, clips.Count);
        audioSource.PlayOneShot(clips[randomIndex]);
        lastPlayedClipIndex = randomIndex;
    }

    public void PlayRandomExceptSameOne()
    {
        if (clips.Count < 2)
        { 
            PlayRandom();
            return;
        }

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, clips.Count);
        } while (randomIndex == lastPlayedClipIndex);

        audioSource.PlayOneShot(clips[randomIndex]);
        lastPlayedClipIndex = randomIndex;
    }
}
