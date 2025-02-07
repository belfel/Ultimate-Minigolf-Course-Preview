using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAvatar : MonoBehaviour
{
    [SerializeField] private Image avatar;
    [SerializeField] private Image outline;

    private Sprite defaultAvatar;

    public void SetAvatar(Sprite sprite)
    {
        if (defaultAvatar == null)
            defaultAvatar = avatar.sprite;
        avatar.sprite = sprite;
    }

    public void SetAvatarToDefault()
    {
        avatar.sprite = defaultAvatar;
    }

    public void SetOutlineColor(Color color)
    {
        outline.color = color;
    }
}
