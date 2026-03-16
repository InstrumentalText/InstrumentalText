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

    // 被 gaze 命中 → 高亮
    public void ApplyHighlight()
    {
        if (outline != null)
            outline.enabled = true;
    }

    // gaze 离开 → 取消高亮
    public void RemoveHighlight()
    {
        if (outline != null)
            outline.enabled = false;
    }
}