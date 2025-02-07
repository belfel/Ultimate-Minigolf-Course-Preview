using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationBox : MonoBehaviour
{
    public Action onConfirm;
    public Action onCancel;

    public GameEvent OnInstantiateEvent;
    public GameEvent OnDestroyEvent;

    [SerializeField] private Button cancel;
    [SerializeField] private Button confirm;

    private void Awake()
    {
        if (cancel != null)
            cancel.onClick.AddListener(OnCancel);

        if (confirm != null)
            confirm.onClick.AddListener(OnConfirm);

        var timeLimit = gameObject.GetComponent<ConfirmationBox_TimeLimit>();
        if (timeLimit != null)
            timeLimit.OnTimeRanOut = () => OnCancel();
    }

    private void Start()
    {
        if (OnInstantiateEvent != null)
            OnInstantiateEvent.Raise();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            OnCancel();

        if (Input.GetKeyDown(KeyCode.Return))
            OnConfirm();
    }

    private void OnCancel()
    {
        if (onCancel != null)
            onCancel.Invoke();

        Destroy(gameObject);
    }

    private void OnConfirm()
    {
        if (onConfirm != null)
            onConfirm.Invoke();

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnDestroyEvent.Raise();
    }
}
