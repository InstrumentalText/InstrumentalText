using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TriggerAreaPromptExecutorOnDevice : MonoBehaviour
{
    [Header("颜色设置")]
    public Color enterColor = Color.red;
    public Color exitColor = Color.white;

    [Header("Prompt UI")]
    public TMP_Text displayText;

    [Header("Pinch Input")]
    public InputActionProperty pinchAction;
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;

    private bool hasProcessed = false;
    private Collider currentHandle;

    private LLMProcessorOnDevice llmProcessor;

    private void Start()
    {
        llmProcessor = FindFirstObjectByType<LLMProcessorOnDevice>();

        if (llmProcessor == null)
        {
            Debug.LogError("[TriggerAreaPromptExecutorOnDevice] ❌ LLMProcessorOnDevice NOT FOUND in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name != "handle") return;

        Debug.Log("[TriggerAreaPromptExecutorOnDevice] Handle 进入");

        currentHandle = other;
        hasProcessed = false;

        Renderer rend = other.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = enterColor;
        }

        if (displayText != null)
        {
            displayText.text = "";
        }
    }

    private void Update()
    {
        if (currentHandle == null || hasProcessed) return;

        float pinchValue = pinchAction.action.ReadValue<float>();

        // 检测 pinch release
        if (pinchValue < pinchUpThreshold)
        {
            ProcessPrompt();
        }
    }

    private void ProcessPrompt()
    {
        if (currentHandle == null) return;

        // ⭐ 从 root 查找 TextObject
        Transform root = currentHandle.transform.root;

        CurrentTextStore textStore = root.GetComponent<CurrentTextStore>();

        if (textStore == null)
        {
            Debug.LogWarning("[TriggerAreaPromptExecutorOnDevice] No CurrentTextStore on root.");
            return;
        }

        string prompt = textStore.CurrentText;

        if (string.IsNullOrEmpty(prompt)) return;

        hasProcessed = true;

        Debug.Log($"[TriggerAreaPromptExecutorOnDevice] Prompt detected: {prompt}");

        if (displayText != null)
        {
            displayText.text = "Prompt: " + prompt;
        }

        if (llmProcessor != null)
        {
            llmProcessor.ProcessPrompt(gameObject, prompt);
        }
        else
        {
            Debug.LogError("[TriggerAreaPromptExecutorOnDevice] LLMProcessorOnDevice is NULL!");
        }

        // 删除整个 TextObject
        Destroy(root.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == currentHandle)
        {
            Debug.Log("[TriggerAreaPromptExecutorOnDevice] Handle 离开");

            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = exitColor;
            }

            currentHandle = null;
            hasProcessed = false;
        }
    }
}