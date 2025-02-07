using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private bool gameplayMenuOpen = false;
    private bool cursorVisible = false;
    private bool cursorLocked = false;

    public void ShowCursor()
    {
        cursorVisible = true;
        UpdateCursor();
    }

    public void HideCursor()
    {
        cursorVisible = false;
        UpdateCursor();
    }

    public void LockCursor()
    {
        cursorLocked = true;
        UpdateCursor();
    }

    public void UnlockCursor()
    {
        cursorLocked = false;
        UpdateCursor();
    }

    public void OnGameplayMenuOpened()
    {
        gameplayMenuOpen = true;
        UpdateCursor();
    }

    public void OnGameplayMenuClosed()
    {
        gameplayMenuOpen = false;
        UpdateCursor();
    }

    public void UpdateCursor()
    {
        if (gameplayMenuOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            return;
        }

        if (cursorVisible)
        {
            Cursor.visible = true;
        }
        else Cursor.visible = false;

        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else Cursor.lockState = CursorLockMode.Confined;
    }
}
