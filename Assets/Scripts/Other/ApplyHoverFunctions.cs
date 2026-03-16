// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.Interaction.Toolkit;

// [RequireComponent(typeof(Collider))]
// public class ApplyHoverFunctions : MonoBehaviour
// {
//     [Header("Outline Settings")]
//     public Color outlineColor = Color.yellow;
//     public float outlineWidth = 5f;

//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;

//     private Outline outline; // 用于边缘高亮
//     private bool waitingForRelease = false;

//     void Awake()
//     {
//         outline = gameObject.AddComponent<Outline>();
//         outline.enabled = false;
//         outline.OutlineMode = Outline.Mode.OutlineAll;
//         outline.OutlineColor = outlineColor;
//         outline.OutlineWidth = outlineWidth;
//     }

//     void Update()
//     {
//         if (!waitingForRelease)
//             return;

//         float pinchValue = pinchAction.action.ReadValue<float>();

//         if (pinchValue < 0.2f)
//         {
//             Debug.Log($"[ApplyHoverFunctions] Pinch released → Apply on {gameObject.name}");
//             waitingForRelease = false;
//             Apply();
//         }
//     }


//     //hover触发
//     public void OnFirstHoverEntered(HoverEnterEventArgs args)
//     {
//         Debug.Log($"[ApplyHoverFunctions] Hover Enter: {gameObject.name}");

//         var applier = FindObjectOfType<GazePinchPromptApplierOnDevice>();
//         if (applier == null)
//         {
//             Debug.Log("[ApplyHoverFunctions] No Applier found → ignore hover");
//             return;
//         }

//         if (!applier.IsApplyMode())
//         {
//             Debug.Log("[ApplyHoverFunctions] Not in ApplyMode → ignore hover");
//             return;
//         }

//         //边缘高亮
//         SetOutline(true);

//         applier.SetCurrentTarget(gameObject);
//         Debug.Log($"[ApplyHoverFunctions] Set as current target in Applier: {gameObject.name}");

//         waitingForRelease = true;
//         Debug.Log($"[ApplyHoverFunctions] Waiting for pinch release on {gameObject.name}");
//     }



//     public void OnHoverExited(HoverExitEventArgs args)
//     {
//         Debug.Log($"[ApplyHoverFunctions] Hover Exit: {gameObject.name}");

//         SetOutline(false);

//         var applier = FindObjectOfType<GazePinchPromptApplierOnDevice>();
//         if (applier != null && applier.GetCurrentTarget() == gameObject)
//         {
//             applier.SetCurrentTarget(null);
//             Debug.Log($"[ApplyHoverFunctions] Cleared current target in Applier: {gameObject.name}");
//         }

//         waitingForRelease = false;
//     }



//     void SetOutline(bool state)
//     {
//         if (outline != null)
//         {
//             outline.enabled = state;
//             Debug.Log($"[ApplyHoverFunctions] Outline {(state ? "ON" : "OFF")} for {gameObject.name}");
//         }
//     }



//     void Apply()
//     {
//         Debug.Log($"[ApplyHoverFunctions] APPLY triggered on {gameObject.name}");

//         var applier = FindObjectOfType<GazePinchPromptApplierOnDevice>();
//         if (applier != null)
//         {
//             applier.ApplyPromptToTarget(gameObject);
//         }
//         else
//         {
//             Debug.Log("[ApplyHoverFunctions] Applier is null, cannot apply");
//         }
//     }
// }


using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
public class ApplyHoverFunctions : MonoBehaviour
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
        var applier = FindObjectOfType<GazePinchPromptApplierOnDevice>();

        // 如果没有 Applier → 说明没有 TextObject → ignore
        if (applier == null)
        {
            Debug.Log("[ApplyHoverFunctions] No Applier found → ignore hover");
            return;
        }

        // 如果不在 ApplyMode → ignore
        if (!applier.IsApplyMode())
        {
            Debug.Log("[ApplyHoverFunctions] Not in ApplyMode → ignore hover");
            return;
        }

        // 开始高亮
        SetOutline(true);

        // 设置当前 target
        applier.SetCurrentTarget(gameObject);

        Debug.Log($"[ApplyHoverFunctions] Hover Enter: {gameObject.name}");
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        var applier = FindObjectOfType<GazePinchPromptApplierOnDevice>();

        // 没有 applier 就直接关高亮
        SetOutline(false);

        if (applier == null)
            return;

        // 如果当前 target 是自己 → 清空
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