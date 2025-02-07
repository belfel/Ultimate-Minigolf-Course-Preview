using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BallCollisionSounds", menuName = "Scriptable Objects/BallCollisionSounds")]
public class BallCollisionSounds : ScriptableObject
{
    public AudioClip defaultSound;
    public List<MaterialSoundPairs> pairs = new List<MaterialSoundPairs>();

    public AudioClip GetCollisionSound(SurfaceMaterial.SurfaceMaterialType type)
    {
        foreach (var pair in pairs)
            if (pair.surfaceMaterialType == type)
            {
                var sound = pair.materialSound.GetRandomSound();
                return sound == null ? defaultSound : sound;
            }

        return defaultSound;
    }

    [System.Serializable]
    public struct MaterialSoundPairs
    {
        public SurfaceMaterial.SurfaceMaterialType surfaceMaterialType;
        public MaterialSound materialSound;
    }
}
