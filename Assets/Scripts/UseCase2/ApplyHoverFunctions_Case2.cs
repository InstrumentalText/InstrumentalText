// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// [RequireComponent(typeof(Collider))]
// public class ApplyHoverFunctions_Case2 : MonoBehaviour
// {
//     [Header("Outline Settings")]
//     public Color outlineColor = Color.yellow;
//     public float outlineWidth = 5f;

//     private Outline outline;

//     void Awake()
//     {
//         outline = gameObject.AddComponent<Outline>();
//         outline.enabled = false;
//         outline.OutlineMode = Outline.Mode.OutlineAll;
//         outline.OutlineColor = outlineColor;
//         outline.OutlineWidth = outlineWidth;
//     }

//     public void OnFirstHoverEntered(HoverEnterEventArgs args)
//     {   
//         //这里的逻辑需要修改，因为场景中可能存在多个Text object，每一个text object上都会有“GazePinchPromptApplierOnDevice_Case2”
//         //按照现在逻辑，如果场景中有多个text object的话就会找第一个？然后场景中始终只有第一个text object可以成功apply,其他就不行了。这个是有问题的
//         //我的场景中会有多个text object挂<GazePinchPromptApplierOnDevice_Case2的脚本，但是关键是同一时刻只会有一个text object进入apply mode
//         //进入apply mode的text object可以通过gaze去触发interactive object，满足我们之前这里写的逻辑的话就可以apply->触发llm
//         //所以只要有一个text object进入apply mode的时候就可以触发gaze interactive object的高亮，所以对interactive object来说没有特别大的改变？
//         //你看看我们现在哪里需要修改。其实就是注意不同的text object可以进入ally mode,现在的逻辑没办法实现。
//         var applier = FindObjectOfType<GazePinchPromptApplierOnDevice_Case2>();

//         // 如果没有 Applier → ignore
//         if (applier == null)
//         {
//             Debug.Log("[ApplyHoverFunctions] No Applier found → ignore hover");
//             return;
//         }

//         // 如果不在 ApplyMode → ignore
//         if (!applier.IsApplyMode())
//         {
//             Debug.Log("[ApplyHoverFunctions] Not in ApplyMode → ignore hover");
//             return;
//         }

//         SetOutline(true);

//         applier.SetCurrentTarget(gameObject);

//         Debug.Log($"[ApplyHoverFunctions] Hover Enter: {gameObject.name}");
//     }

//     public void OnHoverExited(HoverExitEventArgs args)
//     {
//         var applier = FindObjectOfType<GazePinchPromptApplierOnDevice_Case2>();

//         SetOutline(false);

//         if (applier == null)
//             return;

//         if (applier.GetCurrentTarget() == gameObject)
//         {
//             applier.SetCurrentTarget(null);
//             Debug.Log($"[ApplyHoverFunctions] Cleared current target: {gameObject.name}");
//         }

//         Debug.Log($"[ApplyHoverFunctions] Hover Exit: {gameObject.name}");
//     }

//     void SetOutline(bool state)
//     {
//         if (outline != null)
//         {
//             outline.enabled = state;
//             Debug.Log($"[ApplyHoverFunctions] Outline {(state ? "ON" : "OFF")} for {gameObject.name}");
//         }
//     }
// }



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
        // ✅ 改为使用当前激活的 Applier
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