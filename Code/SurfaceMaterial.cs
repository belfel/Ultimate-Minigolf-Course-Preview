using UnityEngine;

public class SurfaceMaterial : MonoBehaviour
{
    [SerializeField] private SurfaceMaterialType surfaceMaterialType;

    public SurfaceMaterialType GetMaterialType()
    {
        return surfaceMaterialType;
    }

    public enum SurfaceMaterialType
    {
        Grass, Wall, Wood, Metal
    }
}
