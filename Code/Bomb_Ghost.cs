using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb_Ghost : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        DestructibleObject otherObject = other.gameObject.GetComponent<DestructibleObject>();

        if (otherObject != null )
        {
            otherObject.Highlight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DestructibleObject otherObject = other.gameObject.GetComponent<DestructibleObject>();

        if (otherObject != null)
        {
            otherObject.Dehighlight();
        }
    }
}
