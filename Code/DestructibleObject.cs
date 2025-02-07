using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DestructibleObject: MonoBehaviour
{
    [SerializeField] private GameObject objectRoot;

    [SerializeField] private Material highlightMaterial;

    [SerializeField] private bool getRenderersAutomatically = true;
    [SerializeField] private List<MeshRenderer> objectRenderers = new List<MeshRenderer>();
    private List<List<Material>> initialMaterials = new List<List<Material>>();

    private void Awake()
    {
        if (objectRoot == null)
            objectRoot = gameObject;

        if (getRenderersAutomatically)
            FetchRenderers();

        FetchMaterials();
    }

    private void FetchRenderers()
    {
        var renderer = objectRoot.GetComponent<MeshRenderer>();
        if (renderer != null)
            objectRenderers.Add(renderer);

        var childrenRenderers = objectRoot.GetComponentsInChildren<MeshRenderer>();
        foreach (var childRenderer in childrenRenderers)
            objectRenderers.Add(childRenderer);
    }

    private void FetchMaterials()
    {
        foreach (var renderer in objectRenderers)
        {
            List<Material> tempMaterials = new List<Material>();
            renderer.GetMaterials(tempMaterials);
            initialMaterials.Add(tempMaterials);
        }
    }

    public void Highlight()
    {
        foreach(var renderer in objectRenderers)
        {
            int rendererMatCount = renderer.materials.Length;
            List<Material> replacedMaterials = new List<Material>();
            for (int i = 0; i < rendererMatCount; i++)
                replacedMaterials.Add(highlightMaterial);

            renderer.SetMaterials(replacedMaterials);
        }
    }

    public void Dehighlight()
    {
        int i = 0;
        foreach (var renderer in objectRenderers)
        {
            renderer.SetMaterials(initialMaterials[i]);
            i++;
        }
    }

    public NetworkObject GetRootNetworkObject()
    {
        var networkObject = objectRoot.GetComponent<NetworkObject>();
        if (!networkObject)
        {
            Debug.LogError("Object missing a network object component in it's root");
            return null;
        }

        return networkObject;
    }
}
