using UnityEngine;

public class SimplePromptTriggerOnDevice : MonoBehaviour
{
    private LLMProcessorOnDevice llmProcessor;

    // 防止 OnTriggerStay 每帧重复执行
    private bool hasTriggered = false;

    private void Start()
    {
        llmProcessor = FindFirstObjectByType<LLMProcessorOnDevice>();

        if (llmProcessor == null)
        {
            Debug.LogError("[SimplePromptTriggerOnDevice] ❌ LLMProcessorOnDevice NOT FOUND in scene!");
        }
        else
        {
            Debug.Log("[SimplePromptTriggerOnDevice] ✅ LLMProcessorOnDevice connected.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[SimplePromptTriggerOnDevice] OnTriggerEnter: {other.name}");
        TrySendPrompt(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TrySendPrompt(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[SimplePromptTriggerOnDevice] OnTriggerExit: {other.name}");
        hasTriggered = false;
    }

    void TrySendPrompt(Collider other)
    {
        if (hasTriggered)
            return;

        Debug.Log($"[SimplePromptTriggerOnDevice] Checking collider: {other.name}");

        // ⭐ 找到整个 TextObject prefab 的 root
        Transform root = other.transform.root;

        Debug.Log($"[SimplePromptTriggerOnDevice] Root object: {root.name}");

        CurrentTextStore textStore = root.GetComponent<CurrentTextStore>();

        if (textStore == null)
        {
            Debug.LogWarning("[SimplePromptTriggerOnDevice] ❌ No CurrentTextStore found on ROOT object.");
            return;
        }

        string prompt = textStore.CurrentText;

        Debug.Log($"[SimplePromptTriggerOnDevice] CurrentText = {prompt}");

        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogWarning("[SimplePromptTriggerOnDevice] ⚠ Prompt is EMPTY.");
            return;
        }

        Debug.Log($"[SimplePromptTriggerOnDevice] 🚀 Sending prompt to LLM: {prompt}");

        if (llmProcessor != null)
        {
            llmProcessor.ProcessPrompt(gameObject, prompt);
            hasTriggered = true;
        }
        else
        {
            Debug.LogError("[SimplePromptTriggerOnDevice] ❌ LLMProcessorOnDevice is NULL when sending prompt.");
        }
    }
}