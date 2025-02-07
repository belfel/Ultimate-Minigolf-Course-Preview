using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class StringVariable : ScriptableObject
{
    public GameEvent valueChanged;
    public string value;

    public void SetValue(string _value)
    {
        if (value != _value && valueChanged != null)
            valueChanged.Raise();
        value = _value;
    }
}
