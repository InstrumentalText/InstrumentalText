using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Collider))]
public class ApplyHoverFunctions : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 5f;

    private XRRayInteractor gazeInteractor;
    private Outline outline;

    void Awake()
    {
        outline = gameObject.AddComponent<Outline>();
        outline.enabled = false;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;

        var gazeObj = GameObject.FindWithTag("GazeInteractor");
        if (gazeObj != null)
            gazeInteractor = gazeObj.GetComponent<XRRayInteractor>();
    }

    void Start()
    {
        var simple = GetComponent<XRSimpleInteractable>();
        if (simple != null)
            simple.hoverEntered.AddListener(OnHoverEntered);
    }

    void OnDestroy()
    {
        var simple = GetComponent<XRSimpleInteractable>();
        if (simple != null)
            simple.hoverEntered.RemoveListener(OnHoverEntered);
    }

    bool IsGazeInteractor(IXRInteractor interactor)
    {
        if (gazeInteractor == null) return true;
        return ReferenceEquals(interactor, gazeInteractor);
    }

    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (!IsGazeInteractor(args.interactorObject))
            return;

        var applier = FindObjectOfType<GazePinchPromptApplierOnDevice>();

        if (applier == null || !applier.IsApplyMode())
            return;

        SetOutline(true);
        applier.SetCurrentTarget(gameObject);

        Debug.Log($"[ApplyHoverFunctions] Hover Enter: {gameObject.name}");
    }

    // 保留供 Inspector 中已连接的 First Hover Entered 事件使用
    public void OnFirstHoverEntered(HoverEnterEventArgs args)
    {
        OnHoverEntered(args);
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        if (!IsGazeInteractor(args.interactorObject))
            return;

        var applier = FindObjectOfType<GazePinchPromptApplierOnDevice>();

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