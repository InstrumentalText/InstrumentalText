using UnityEngine;
using System.Collections.Generic;

public class ObjectController : MonoBehaviour
{
    private readonly List<Material> modifiedMats = new();

    public void Select(Color emissionColor)
    {
        modifiedMats.Clear();
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.IsKeywordEnabled("_EMISSION")) continue;

                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
                modifiedMats.Add(mat);
            }
        }
    }

    public void Deselect()
    {
        foreach (var mat in modifiedMats)
        {
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
        }
        modifiedMats.Clear();
    }
}
