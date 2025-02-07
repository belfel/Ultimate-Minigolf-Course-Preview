using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMarker : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 180f);
    }
}
