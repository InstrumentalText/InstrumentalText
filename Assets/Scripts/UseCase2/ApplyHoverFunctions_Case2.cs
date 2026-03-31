using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
public class ApplyHoverFunctions_Case2 : MonoBehaviour
{
    [Header("Outline Settings")]
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

    public void OnFirstHoverEntered(HoverEnterEventArgs args)
    {
        var applier = GazePinchPromptApplierOnDevice_Case2.ActiveInstance;

        if (applier == null)
        {
            Debug.Log("[ApplyHoverFunctions] No active Applier → ignore hover");
            return;
        }

        if (!applier.IsApplyMode())
        {
            Debug.Log("[ApplyHoverFunctions] Not in ApplyMode → ignore hover");
            return;
        }

        SetOutline(true);

        applier.SetCurrentTarget(gameObject);

        Debug.Log($"[ApplyHoverFunctions] Hover Enter: {gameObject.name}");
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        var applier = GazePinchPromptApplierOnDevice_Case2.ActiveInstance;

        SetOutline(false);

        if (applier == null)
            return;

        if (applier.GetCurrentTarget() == gameObject)
        {
            applier.SetCurrentTarget(null);
            Debug.Log($"[ApplyHoverFunctions] Cleared current target: {gameObject.name}");
        }

        Debug.Log($"[ApplyHoverFunctions] Hover Exit: {gameObject.name}");
    }

    void SetOutline(bool state)
    {
        if (outline != null)
        {
            outline.enabled = state;
            Debug.Log($"[ApplyHoverFunctions] Outline {(state ? "ON" : "OFF")} for {gameObject.name}");
        }
    }
}