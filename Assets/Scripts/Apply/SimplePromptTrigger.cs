using UnityEngine;

public class SimplePromptTrigger : MonoBehaviour
{
    private LLMProcessor llmProcessor;

    // 防止 OnTriggerStay 每帧重复执行
    private bool hasTriggered = false;

    private void Start()
    {
        llmProcessor = FindFirstObjectByType<LLMProcessor>();

        if (llmProcessor == null)
        {
            Debug.LogError("[SimplePromptTrigger] ❌ LLMProcessor NOT FOUND in scene!");
        }
        else
        {
            Debug.Log("[SimplePromptTrigger] ✅ LLMProcessor connected.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[SimplePromptTrigger] OnTriggerEnter: {other.name}");
        TrySendPrompt(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TrySendPrompt(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[SimplePromptTrigger] OnTriggerExit: {other.name}");
        hasTriggered = false;
    }

    void TrySendPrompt(Collider other)
    {
        if (hasTriggered)
            return;

        Debug.Log($"[SimplePromptTrigger] Checking collider: {other.name}");

        // ⭐ 找到整个 TextObject prefab 的 root
        Transform root = other.transform.root;

        Debug.Log($"[SimplePromptTrigger] Root object: {root.name}");

        CurrentTextStore textStore = root.GetComponent<CurrentTextStore>();

        if (textStore == null)
        {
            Debug.LogWarning("[SimplePromptTrigger] ❌ No CurrentTextStore found on ROOT object.");
            return;
        }

        string prompt = textStore.CurrentText;

        Debug.Log($"[SimplePromptTrigger] CurrentText = {prompt}");

        if (string.IsNullOrEmpty(prompt))
        {
            Debug.LogWarning("[SimplePromptTrigger] ⚠ Prompt is EMPTY.");
            return;
        }

        Debug.Log($"[SimplePromptTrigger] 🚀 Sending prompt to LLM: {prompt}");

        if (llmProcessor != null)
        {
            llmProcessor.ProcessPrompt(gameObject, prompt);
            hasTriggered = true;
        }
        else
        {
            Debug.LogError("[SimplePromptTrigger] ❌ LLMProcessor is NULL when sending prompt.");
        }
    }
}