using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private Color highlightTint = new Color(1.2f, 1.2f, 1.2f, 1f);

    private RawImage rawImage;
    private TextMeshProUGUI[] tmps;
    private Color originalImageColor;
    private Color[] originalTmpColors;

    void Awake()
    {
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            rawImage = canvas.GetComponentInChildren<RawImage>();
            tmps = canvas.GetComponentsInChildren<TextMeshProUGUI>();
        }
    }

    public void Select()
    {
        if (rawImage != null)
        {
            originalImageColor = rawImage.color;
            rawImage.color = originalImageColor * highlightTint;
        }

        if (tmps != null)
        {
            originalTmpColors = new Color[tmps.Length];
            for (int i = 0; i < tmps.Length; i++)
            {
                originalTmpColors[i] = tmps[i].color;
                tmps[i].color = originalTmpColors[i] * highlightTint;
            }
        }
    }

    public void Deselect()
    {
        if (rawImage != null)
            rawImage.color = originalImageColor;

        if (tmps != null && originalTmpColors != null)
        {
            for (int i = 0; i < tmps.Length && i < originalTmpColors.Length; i++)
                tmps[i].color = originalTmpColors[i];
        }
    }
}
