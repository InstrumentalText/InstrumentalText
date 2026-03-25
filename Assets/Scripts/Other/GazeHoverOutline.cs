using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GazeHoverOutline : MonoBehaviour
{
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 5f;

    private Outline outline;

    void Awake()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.enabled = false;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;
    }

    public void ApplyHighlight()
    {
        if (outline != null)
            outline.enabled = true;
    }

    public void RemoveHighlight()
    {
        if (outline != null)
            outline.enabled = false;
    }
}