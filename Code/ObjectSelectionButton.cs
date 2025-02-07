using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelectionButton : MonoBehaviour
{
    public StringVariable title;
    public StringVariable description;

    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private GameObject icon;
    [SerializeField] private Image outline;
    [SerializeField] private Image background;
    [SerializeField] private float selectedOutlineOpacity = 0.4f;
    [SerializeField] private float selectedBackgroundepacity = 0.9f;
    [SerializeField] private string objectTitle;
    [SerializeField] private string objectDescription;

    private void Start()
    {
        icon.transform.rotation = Quaternion.identity;
    }

    public void ReplaceText()
    {
        title.SetValue(objectTitle);
        description.SetValue(objectDescription);
    }

    public void SetColor(Color color)
    {
        outline.color = new Color(color.r, color.g, color.b, selectedOutlineOpacity);
        background.color = new Color(color.r, color.g, color.b, selectedBackgroundepacity);
    }

    public GameObject GetObjectPrefab()
    {
        return objectPrefab;
    }    
}
