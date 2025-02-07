using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfirmationBox_TimeLimit : MonoBehaviour
{
    public Action OnTimeRanOut;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int limitInSeconds;

    private void Awake()
    {
        countdownText.text = limitInSeconds.ToString();
    }

    private void Start()
    {
        StartCoroutine(CountdownRoutine());   
    }

    private IEnumerator CountdownRoutine()
    {
        int timeRemaining = limitInSeconds;

        while (timeRemaining > 0)
        {
            countdownText.text = timeRemaining.ToString();
            yield return new WaitForSeconds(1f);
            timeRemaining -= 1;
        }

        if ( OnTimeRanOut != null )
            OnTimeRanOut.Invoke();
    }
}
