using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Keybinds : ScriptableObject
{
    public bool usingController;

    public Bind moveForward;
    public Bind moveLeft;
    public Bind moveBackwards;
    public Bind moveRight;
    public Bind toggleFreelook;
    public Bind quickReset;
    public Bind fullReset;
    public Bind openChat;
    public Bind nextAxis;
    public Bind previousAxis;
    public Bind rotateObject;

    public float GetVerticalAxisRaw()
    {
        float val = 0f;

        if (Input.GetKey(moveForward.key))
            val += 1f;

        if (Input.GetKey(moveBackwards.key))
            val -= 1f;

        return val;
    }

    public float GetHorizontalAxisRaw()
    {
        float val = 0f;

        if (Input.GetKey(moveRight.key))
            val += 1f;

        if (Input.GetKey(moveLeft.key))
            val -= 1f;

        return val;
    }
}
