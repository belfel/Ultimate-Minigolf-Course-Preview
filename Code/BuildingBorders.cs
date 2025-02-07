using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBorders : MonoBehaviour
{
    [SerializeField] private GameObject borders;
    [SerializeField] private float showDuration = 0.5f;
    private float hideTimer = 0f;

    private void Update()
    {
        if (!borders.activeInHierarchy)
            return;

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
            borders.SetActive(false);

    }

    public void ShowBorders()
    {
        hideTimer = showDuration;
        borders.SetActive(true);
    }
}
