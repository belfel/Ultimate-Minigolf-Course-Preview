using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFlavourTextController : MonoBehaviour
{
    public GameEvent OnHide;

    [SerializeField] private GameObject text;
    [SerializeField] private float displayDuration = 1f;
    [SerializeField] private bool startVisible = false;

    private void Awake()
    {
        if (startVisible)
            text.SetActive(true);
        else text.SetActive(false);
    }

    public void DisplayText()
    {
        text.SetActive(true);
        Invoke("HideText", displayDuration);
    }

    public void HideText()
    {
        if (OnHide != null)
            OnHide.Raise();
        text.SetActive(false);
    }
}
