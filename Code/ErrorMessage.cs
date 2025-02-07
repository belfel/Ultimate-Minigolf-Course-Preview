using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void SetTitle(string newTitle)
    {
        title.text = newTitle.ToLower();
    }

    public void SetDescription(string newDescription)
    {
        description.text = newDescription.ToLower();
    }

    public void OnContinue()
    {
        Destroy(gameObject);
    }
}
