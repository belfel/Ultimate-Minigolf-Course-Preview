using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsTrigger : MonoBehaviour
{
    public GameEvent localPlayerOutOfBounds;

    private void OnTriggerEnter(Collider other)
    {
        BallController controller = other.GetComponent<BallController>();

        if (controller)
        {
            localPlayerOutOfBounds.Raise();
        }
    }
}
