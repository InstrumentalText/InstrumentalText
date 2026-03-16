// using UnityEngine;
// using UnityEngine.InputSystem;
// using System.Collections;

// [RequireComponent(typeof(Collider))]
// public class GazePinchPromptApplierOnDevice : MonoBehaviour
// {
//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;
//     public float pinchHoldTime = 1.0f;
//     public float pinchDownThreshold = 0.8f;
//     public float pinchUpThreshold = 0.2f;

//     [Header("Text Object")]
//     public GameObject textRoot;   // TextObject 根对象，用于显示/隐藏视觉，但这里保持不隐藏

//     private bool applyMode = false;   // 是否进入 ApplyMode
//     private bool pinchActive = false; // 第一次进入 ApplyMode 的 pinch 是否保持
//     private float pinchTimer = 0f;

//     private LLMProcessorOnDevice llmProcessor;
//     private Camera mainCamera;

//     // 当前 hover 高亮目标
//     private GameObject currentTarget = null;

//     void Start()
//     {
//         // 查找 LLMProcessor
//         llmProcessor = FindObjectOfType<LLMProcessorOnDevice>();
//         mainCamera = Camera.main;

//         if (textRoot == null)
//             textRoot = transform.root.gameObject;

//         Debug.Log($"[GazePinchPromptApplierOnDevice] Start: TextRoot = {textRoot.name}");
//         Debug.Log($"[GazePinchPromptApplierOnDevice] llmProcessor found: {(llmProcessor != null ? llmProcessor.name : "null")}");
//     }

//     void Update()
//     {
//         float pinchValue = pinchAction.action.ReadValue<float>();
//         //Debug.Log($"[GazePinchPromptApplierOnDevice] Update: applyMode={applyMode}, pinchActive={pinchActive}, pinchValue={pinchValue}");

//         // Phase 1: 进入 ApplyMode
//         if (!applyMode)
//         {
//             if (IsGazingThisObject() && pinchValue > pinchDownThreshold)
//             {
//                 pinchTimer += Time.deltaTime;
//                 //Debug.Log($"[GazePinchPromptApplierOnDevice] Gaze detected, pinch hold timer = {pinchTimer:F2}s");

//                 if (pinchTimer >= pinchHoldTime)
//                 {
//                     EnterApplyMode();
//                     pinchActive = true; // 标记第一次 pinch 保持
//                 }
//             }
//             else
//             {
//                 if (pinchTimer > 0f)
//                     //Debug.Log($"[GazePinchPromptApplierOnDevice] Pinch interrupted, reset timer");

//                 pinchTimer = 0f;
//             }
//             return;
//         }

//         // Phase 2: ApplyMode 中，等待 Pinch Release
//         if (pinchActive)
//         {
//             if (currentTarget != null)
//             {
//                 //Debug.Log($"[GazePinchPromptApplierOnDevice] Waiting for Pinch Release on target: {currentTarget.name}");
//             }
//             else
//             {
//                 //Debug.Log("[GazePinchPromptApplierOnDevice] Waiting for Pinch Release, no target yet");
//             }

//             if (pinchValue < pinchUpThreshold)
//             {
//                 if (currentTarget != null)
//                 {
//                     Debug.Log($"[GazePinchPromptApplierOnDevice] Pinch released → apply to target: {currentTarget.name}");
//                     ApplyPromptToTarget(currentTarget);
//                 }
//                 else
//                 {
//                     Debug.Log("[GazePinchPromptApplierOnDevice] Pinch released, but no target → exit apply mode");
//                     ExitApplyModeWithoutApply();
//                 }
//             }
//         }
//     }

//     bool IsGazingThisObject()
//     {
//         Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
//         bool hit = GetComponent<Collider>().Raycast(ray, out RaycastHit hitInfo, 100f);
//         if (hit)
//             Debug.Log($"[GazePinchPromptApplierOnDevice] Raycast hit {gameObject.name}");
//         return hit;
//     }

//     void EnterApplyMode()
//     {
//         applyMode = true;
//         pinchTimer = 0f;

//         // 保留 textRoot 显示，不 SetActive(false)

//         ApplyTargetHighlighter.Instance?.SetApplyMode(true);

//         Debug.Log("[GazePinchPromptApplierOnDevice] Apply Mode Entered");
//     }

//     void ExitApplyModeWithoutApply()
//     {
//         applyMode = false;
//         pinchActive = false;

//         // 保留 textRoot 显示，不 SetActive(true)

//         ApplyTargetHighlighter.Instance?.SetApplyMode(false);

//         if (currentTarget != null)
//             Debug.Log($"[GazePinchPromptApplierOnDevice] Clearing current target: {currentTarget.name}");

//         currentTarget = null;

//         Debug.Log("[GazePinchPromptApplierOnDevice] Apply Mode Exited Without Apply");
//     }

//     // 外部 Hover 脚本调用，应用到目标
//     public void ApplyPromptToTarget(GameObject target)
//     {
//         if (target == null || textRoot == null)
//         {
//             Debug.LogWarning("[GazePinchPromptApplierOnDevice] ApplyPromptToTarget: target or textRoot is null");
//             return;
//         }

//         if (llmProcessor == null)
//         {
//             Debug.LogWarning("[GazePinchPromptApplierOnDevice] llmProcessor is null → cannot call ProcessPrompt");
//             return;
//         }

//         CurrentTextStore textStore = textRoot.GetComponent<CurrentTextStore>();
//         if (textStore == null)
//         {
//             Debug.LogWarning("[GazePinchPromptApplierOnDevice] ApplyPromptToTarget: CurrentTextStore not found");
//             return;
//         }

//         string prompt = textStore.CurrentText;
//         if (string.IsNullOrEmpty(prompt))
//         {
//             Debug.LogWarning("[GazePinchPromptApplierOnDevice] ApplyPromptToTarget: CurrentText is empty");
//             return;
//         }

//         Debug.Log($"[GazePinchPromptApplierOnDevice] Applying prompt '{prompt}' to {target.name}");

//         llmProcessor.ProcessPrompt(target, prompt);

//         applyMode = false;
//         pinchActive = false;
//         currentTarget = null;

//         ApplyTargetHighlighter.Instance?.SetApplyMode(false);

//         Debug.Log("[GazePinchPromptApplierOnDevice] Apply completed, ApplyMode exited");
//     }

//     public void SetCurrentTarget(GameObject target)
//     {
//         currentTarget = target;
//         Debug.Log($"[GazePinchPromptApplierOnDevice] SetCurrentTarget: {(target != null ? target.name : "null")}");
//     }

//     public GameObject GetCurrentTarget()
//     {
//         return currentTarget;
//     }

//     public bool IsApplyMode()
//     {
//         return applyMode;
//     }
// }


using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class GazePinchPromptApplierOnDevice : MonoBehaviour
{
    [Header("Pinch Input")]
    public InputActionProperty pinchAction;
    public float pinchHoldTime = 1.0f;
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;

    [Header("Text Object")]
    public GameObject textRoot;

    private bool applyMode = false;
    private bool pinchActive = false;
    private float pinchTimer = 0f;

    private LLMProcessorOnDevice llmProcessor;
    private Camera mainCamera;

    private GameObject currentTarget = null;

    void Start()
    {
        llmProcessor = FindObjectOfType<LLMProcessorOnDevice>();
        mainCamera = Camera.main;

        if (textRoot == null)
            textRoot = transform.root.gameObject;

        Debug.Log($"[GazePinchPromptApplierOnDevice] Start: TextRoot = {textRoot.name}");
    }

    void Update()
    {
        float pinchValue = pinchAction.action.ReadValue<float>();

        // Phase 1: Enter ApplyMode
        if (!applyMode)
        {
            if (IsGazingThisObject() && pinchValue > pinchDownThreshold)
            {
                pinchTimer += Time.deltaTime;

                if (pinchTimer >= pinchHoldTime)
                {
                    EnterApplyMode();
                    pinchActive = true;
                }
            }
            else
            {
                pinchTimer = 0f;
            }

            return;
        }

        // Phase 2: Wait for pinch release
        if (pinchActive)
        {
            if (pinchValue < pinchUpThreshold)
            {
                if (currentTarget != null)
                {
                    Debug.Log($"[Applier] Pinch released → apply to {currentTarget.name}");
                    ApplyPromptToTarget(currentTarget);
                }
                else
                {
                    Debug.Log("[Applier] Pinch released but no target");
                    ExitApplyModeWithoutApply();
                }
            }
        }
    }

    bool IsGazingThisObject()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        return GetComponent<Collider>().Raycast(ray, out RaycastHit hitInfo, 100f);
    }

    void EnterApplyMode()
    {
        applyMode = true;
        pinchTimer = 0f;

        ApplyTargetHighlighter.Instance?.SetApplyMode(true);

        Debug.Log("[Applier] Apply Mode Entered");
    }

    void ExitApplyModeWithoutApply()
    {
        applyMode = false;
        pinchActive = false;

        ApplyTargetHighlighter.Instance?.SetApplyMode(false);

        currentTarget = null;

        Debug.Log("[Applier] Apply Mode Exited Without Apply");
    }

    public void ApplyPromptToTarget(GameObject target)
    {
        if (target == null || textRoot == null)
        {
            Debug.LogWarning("[Applier] target or textRoot null");
            return;
        }

        if (llmProcessor == null)
        {
            Debug.LogWarning("[Applier] LLMProcessor not found");
            return;
        }

        CurrentTextStore textStore = textRoot.GetComponent<CurrentTextStore>();

        if (textStore == null || string.IsNullOrEmpty(textStore.CurrentText))
        {
            Debug.LogWarning("[Applier] CurrentText empty");
            return;
        }

        string prompt = textStore.CurrentText;

        Debug.Log($"[Applier] Applying '{prompt}' to {target.name}");

        llmProcessor.ProcessPrompt(target, prompt);

        applyMode = false;
        pinchActive = false;
        currentTarget = null;

        ApplyTargetHighlighter.Instance?.SetApplyMode(false);

        Debug.Log("[Applier] Apply Completed");
    }

    public void SetCurrentTarget(GameObject target)
    {
        currentTarget = target;

        Debug.Log($"[Applier] CurrentTarget = {(target != null ? target.name : "null")}");
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    public bool IsApplyMode()
    {
        return applyMode;
    }
}