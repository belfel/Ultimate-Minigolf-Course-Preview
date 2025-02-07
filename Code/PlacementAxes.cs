using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlacementAxes : MonoBehaviour
{
    public Keybinds keybinds;

    [SerializeField] private List<TMP_Text> rotateBindTexts = new List<TMP_Text>();

    private void Awake()
    {
        foreach (var tmp_text in rotateBindTexts)
            tmp_text.text = $"[{keybinds.rotateObject.key.ToString()}]";
    }
}
