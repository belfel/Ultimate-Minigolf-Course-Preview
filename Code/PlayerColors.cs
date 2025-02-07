using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerColors : ScriptableObject
{
    public List<Color> colors = new List<Color>();

    public int GetColorId(Color color)
    {
        int i = 0;
        foreach (Color c in colors)
        {
            if (c == color)
                return i;
            else i++;
        }

        return -1;
    }
}
