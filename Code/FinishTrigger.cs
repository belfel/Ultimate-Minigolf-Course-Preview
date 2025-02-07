using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public GameEvent finishEvent;

    [SerializeField] private bool instantFinish;
    [SerializeField] private float nonInstantFinishDelay = 0.5f;

    private bool localPlayerInsideZone;

    private void OnTriggerEnter(Collider other)
    {
        BallController controller = other.GetComponent<BallController>();
        
        if (controller)
        {
            localPlayerInsideZone = true;
            InvokeRepeating("PlayerLeftZone", 0f, 0.1f);
            Invoke("CancelInsideZoneChecks", nonInstantFinishDelay);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BallController controller = other.GetComponent<BallController>();

        if (controller)
        {
            localPlayerInsideZone = false;
        }
    }

    private bool PlayerLeftZone()
    {
        if (localPlayerInsideZone)
            return false;
        else return true;
    }

    private void CancelInsideZoneChecks()
    {
        CancelInvoke();

        if (PlayerLeftZone())
            return;

        finishEvent.Raise();
    }
}
