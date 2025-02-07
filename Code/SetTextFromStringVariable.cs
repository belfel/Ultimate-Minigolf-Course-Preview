using UnityEngine;
using TMPro;

public class SetTextFromStringVariable : MonoBehaviour
{
    public StringVariable variable;

    private TMP_Text tmpText;

    private void Awake()
    {
        tmpText = gameObject.GetComponent<TMP_Text>();
    }

    public void UpdateText()
    {
        if (tmpText != null)
            tmpText.text = variable.value;
    }
}
