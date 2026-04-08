using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(Collider))]
public class ApplyHoverFunctions_Case2 : MonoBehaviour
{
    [Header("Outline Settings")]
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 5f;

    private XRRayInteractor gazeInteractor;
    private Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
        if (outline == null) return;
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
        // 订阅 hoverEntered（每次 hover 都触发，不只是 first）
        // 这样无论手部射线是否先 hover，gaze 进入时都能正确设置 target
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

    // 每次 hover 进入都调用（代码订阅），替代只在 first hover 触发的 Inspector 事件
    private void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (!IsGazeInteractor(args.interactorObject))
            return;

        var applier = GazePinchPromptApplierOnDevice_Case2.ActiveInstance;

        if (applier == null || !applier.IsApplyMode())
            return;

        SetOutline(true);
        applier.SetCurrentTarget(gameObject);
    }

    // 保留此方法供 Inspector 中已连接的 First Hover Entered 事件使用（无副作用）
    public void OnFirstHoverEntered(HoverEnterEventArgs args)
    {
        OnHoverEntered(args);
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        if (!IsGazeInteractor(args.interactorObject))
            return;

        var applier = GazePinchPromptApplierOnDevice_Case2.ActiveInstance;

        SetOutline(false);

        if (applier == null)
            return;

        if (applier.GetCurrentTarget() == gameObject)
        {
            applier.SetCurrentTarget(null);
            // Debug.Log($"[ApplyHoverFunctions] Cleared current target: {gameObject.name}");
        }

        // Debug.Log($"[ApplyHoverFunctions] Hover Exit: {gameObject.name}");
    }

    void SetOutline(bool state)
    {
        if (outline != null)
        {
            outline.enabled = state;
            // Debug.Log($"[ApplyHoverFunctions] Outline {(state ? "ON" : "OFF")} for {gameObject.name}");
        }
    }
}