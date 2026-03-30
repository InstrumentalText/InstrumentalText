// using UnityEngine;

// public class ColorSourceTag : MonoBehaviour
// {
//     // Marker component: attach to objects that can provide color
// }


using UnityEngine;

/// <summary>
/// Marker component for objects that provide a predefined color.
/// </summary>
public class ColorSourceTag : MonoBehaviour
{
    [Header("Predefined color for TMP mirroring")]
    public Color color = Color.white;
}