using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceMeter : MonoBehaviour
{
    public FloatVariable ballHitForce;
    public FloatVariable ballMaxHitForce;

    [SerializeField] private float maxBarHeight = 490f;
    [SerializeField] private RectTransform bar;

    public void UpdateBar()
    {
        bar.sizeDelta = new Vector2(bar.rect.width, maxBarHeight * ballHitForce.value / ballMaxHitForce.value);
    }

    public void ResetMeter()
    {
        bar.sizeDelta = new Vector2(bar.rect.width, 0f);
    }
}
