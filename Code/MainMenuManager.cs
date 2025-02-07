using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuCanvasPrefab;

    public void InstantiateMainMenuCanvas()
    {
        Instantiate(mainMenuCanvasPrefab);
    }
}
