using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreboardRoundColumn : MonoBehaviour
{
    [SerializeField] private List<TMP_Text> texts = new List<TMP_Text>();

    public void SetTexts(int round, int[] scoresTopDown)
    {
        texts[0].text = round.ToString();
        
        for (int i = 1; i < texts.Count; i++)
        {
            if (scoresTopDown[i - 1] == -1)
                texts[i].text = "";
            else
                texts[i].text = scoresTopDown[i - 1].ToString();
        }
    }
}
