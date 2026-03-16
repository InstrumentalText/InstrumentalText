// using UnityEngine;
// using UnityEngine.InputSystem;

// public class ApplyTargetHighlighter : MonoBehaviour
// {
//     public static ApplyTargetHighlighter Instance;

//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;

//     [Header("Pinch Settings")]
//     public float pinchUpThreshold = 0.2f;

//     private bool applyMode = false;   // 是否处于 ApplyMode
//     private GameObject currentTarget = null;

//     private LLMProcessorOnDevice llmProcessor;

//     void Awake()
//     {
//         if (Instance != null)
//         {
//             Destroy(this);
//             return;
//         }
//         Instance = this;
//         llmProcessor = FindObjectOfType<LLMProcessorOnDevice>();
//     }

//     void Update()
//     {
//         float pinchValue = pinchAction.action.ReadValue<float>();

//         if (applyMode && currentTarget != null && pinchValue < pinchUpThreshold)
//         {
//             // Pinch release → Apply
//             Debug.Log($"[ApplyTargetHighlighter] Pinch released → Apply on {currentTarget.name}");
//             ApplyPrompt(currentTarget);
//         }
//     }

//     public void SetApplyMode(bool state)
//     {
//         applyMode = state;
//     }

//     public void SetCurrentTarget(GameObject target)
//     {
//         currentTarget = target;
//     }

//     public GameObject GetCurrentTarget()
//     {
//         return currentTarget;
//     }

//     private void ApplyPrompt(GameObject target)
//     {
//         if (llmProcessor == null)
//         {
//             Debug.LogWarning("[ApplyTargetHighlighter] LLMProcessor not found");
//             return;
//         }

//         var textStore = target.GetComponentInChildren<CurrentTextStore>();
//         if (textStore == null || string.IsNullOrEmpty(textStore.CurrentText))
//         {
//             Debug.LogWarning("[ApplyTargetHighlighter] No text to apply");
//             return;
//         }

//         llmProcessor.ProcessPrompt(target, textStore.CurrentText);

//         Debug.Log($"[ApplyTargetHighlighter] Applied prompt to {target.name}");

//         // 清理状态
//         currentTarget = null;
//         applyMode = false;
//     }
// }


using UnityEngine;

public class ApplyTargetHighlighter : MonoBehaviour
{
    public static ApplyTargetHighlighter Instance;

    private bool applyMode = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void SetApplyMode(bool state)
    {
        applyMode = state;
        Debug.Log($"[ApplyTargetHighlighter] ApplyMode = {applyMode}");
    }

    public bool IsApplyMode()
    {
        return applyMode;
    }
}