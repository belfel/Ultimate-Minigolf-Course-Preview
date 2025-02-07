using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaterialSound", menuName = "Scriptable Objects/MaterialSound")]
public class MaterialSound : ScriptableObject
{
    public List<AudioClip> sounds = new List<AudioClip>();

    public AudioClip GetRandomSound()
    {
        if (sounds.Count == 0)
            return null;

        int i = Random.Range(0, sounds.Count);
        return sounds[i];
    }
}
