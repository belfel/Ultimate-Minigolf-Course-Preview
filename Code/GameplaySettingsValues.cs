using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameplaySettingsValues : ScriptableObject
{
    public bool hideNamesAndAvatars = false;
    public bool hideChat = false;
    public bool hideControls = false;
    public float mouseSensitivity = 1f;
}
