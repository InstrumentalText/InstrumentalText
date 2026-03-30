// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// [RequireComponent(typeof(Collider))]
// public class ColorTriggerZone : MonoBehaviour
// {
//     private readonly List<Renderer> insideObjects = new();

//     private TMP_Text targetText;
//     private Color defaultColor;
//     private bool isActive = false;

//     public void Initialize(TMP_Text text, Color defaultCol)
//     {
//         targetText = text;
//         defaultColor = defaultCol;
//         Debug.Log($"[ColorTriggerZone] Initialized with default color {defaultColor}");
//     }

//     public void SetActive(bool active)
//     {
//         isActive = active;
//         Debug.Log($"[ColorTriggerZone] SetActive({active})");

//         if (!isActive)
//         {
//             ResetColor();
//         }
//     }

//     private void Awake()
//     {
//         var col = GetComponent<Collider>();
//         col.isTrigger = true;
//         Debug.Log("[ColorTriggerZone] Collider set to isTrigger=true");
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         Debug.Log($"[ColorTriggerZone] OnTriggerEnter: {other.name}");

//         if (!isActive)
//         {
//             Debug.Log("[ColorTriggerZone] Ignored: not active");
//             return;
//         }

//         if (other.GetComponent<ColorSourceTag>() == null)
//         {
//             Debug.Log("[ColorTriggerZone] Ignored: no ColorSourceTag");
//             return;
//         }

//         var renderer = other.GetComponent<Renderer>();
//         if (renderer == null)
//         {
//             Debug.Log("[ColorTriggerZone] Ignored: no Renderer");
//             return;
//         }

//         if (!insideObjects.Contains(renderer))
//         {
//             insideObjects.Add(renderer);
//             Debug.Log($"[ColorTriggerZone] Added {other.name} to insideObjects, count={insideObjects.Count}");
//         }

//         ApplyColor();
//     }

//     private void OnTriggerExit(Collider other)
//     {
//         Debug.Log($"[ColorTriggerZone] OnTriggerExit: {other.name}");

//         if (!isActive)
//         {
//             Debug.Log("[ColorTriggerZone] Ignored: not active");
//             return;
//         }

//         var renderer = other.GetComponent<Renderer>();
//         if (renderer == null)
//         {
//             Debug.Log("[ColorTriggerZone] Ignored: no Renderer");
//             return;
//         }

//         if (insideObjects.Contains(renderer))
//         {
//             insideObjects.Remove(renderer);
//             Debug.Log($"[ColorTriggerZone] Removed {other.name} from insideObjects, count={insideObjects.Count}");
//         }

//         ApplyColor(); 
//     }

//     private void ApplyColor()
//     {
//         if (targetText == null)
//         {
//             Debug.LogWarning("[ColorTriggerZone] ApplyColor called but targetText is null");
//             return;
//         }

//         if (insideObjects.Count == 0)
//         {
//             Debug.Log("[ColorTriggerZone] No objects inside, resetting color");
//             ResetColor();
//             return;
//         }

//         var renderer = insideObjects[insideObjects.Count - 1];
//         if (renderer == null)
//         {
//             Debug.Log("[ColorTriggerZone] Renderer null, resetting color");
//             ResetColor();
//             return;
//         }

//         targetText.color = renderer.material.color;
//         Debug.Log($"[ColorTriggerZone] Applied color {targetText.color} from {renderer.gameObject.name}");
//     }

//     private void ResetColor()
//     {
//         if (targetText != null)
//         {
//             targetText.color = defaultColor;
//             Debug.Log($"[ColorTriggerZone] Reset color to default {defaultColor}");
//         }
//     }
// }



using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public class ColorTriggerZone : MonoBehaviour
{
    private readonly List<ColorSourceTag> insideObjects = new();

    private TMP_Text targetText;
    private Color defaultColor;
    private bool isActive = false;

    public void Initialize(TMP_Text text, Color defaultCol)
    {
        targetText = text;
        defaultColor = defaultCol;
        Debug.Log($"[ColorTriggerZone] Initialized with default color {defaultColor}");
    }

    public void SetActive(bool active)
    {
        isActive = active;
        Debug.Log($"[ColorTriggerZone] SetActive({active})");

        if (!isActive)
        {
            ResetColor();
        }
    }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        Debug.Log("[ColorTriggerZone] Collider set to isTrigger=true");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ColorTriggerZone] OnTriggerEnter: {other.name}");

        if (!isActive)
        {
            Debug.Log("[ColorTriggerZone] Ignored: not active");
            return;
        }

        var colorSource = other.GetComponent<ColorSourceTag>();
        if (colorSource == null)
        {
            Debug.Log("[ColorTriggerZone] Ignored: no ColorSourceTag");
            return;
        }

        if (!insideObjects.Contains(colorSource))
        {
            insideObjects.Add(colorSource);
            Debug.Log($"[ColorTriggerZone] Added {other.name} to insideObjects, count={insideObjects.Count}");
        }

        ApplyColor();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[ColorTriggerZone] OnTriggerExit: {other.name}");

        if (!isActive)
        {
            Debug.Log("[ColorTriggerZone] Ignored: not active");
            return;
        }

        var colorSource = other.GetComponent<ColorSourceTag>();
        if (colorSource == null)
        {
            Debug.Log("[ColorTriggerZone] Ignored: no ColorSourceTag");
            return;
        }

        if (insideObjects.Contains(colorSource))
        {
            insideObjects.Remove(colorSource);
            Debug.Log($"[ColorTriggerZone] Removed {other.name} from insideObjects, count={insideObjects.Count}");
        }

        ApplyColor(); 
    }

    private void ApplyColor()
    {
        if (targetText == null)
        {
            Debug.LogWarning("[ColorTriggerZone] ApplyColor called but targetText is null");
            return;
        }

        if (insideObjects.Count == 0)
        {
            Debug.Log("[ColorTriggerZone] No objects inside, resetting color");
            ResetColor();
            return;
        }

        var colorSource = insideObjects[insideObjects.Count - 1];
        if (colorSource == null)
        {
            Debug.Log("[ColorTriggerZone] ColorSourceTag null, resetting color");
            ResetColor();
            return;
        }

        targetText.color = colorSource.color;
        Debug.Log($"[ColorTriggerZone] Applied color {targetText.color} from {colorSource.gameObject.name}");
    }

    private void ResetColor()
    {
        if (targetText != null)
        {
            targetText.color = defaultColor;
            Debug.Log($"[ColorTriggerZone] Reset color to default {defaultColor}");
        }
    }
}