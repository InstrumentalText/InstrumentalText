using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Text;

public class LLMProcessorOnDevice_Case2 : MonoBehaviour
{
    public enum LLMProvider
    {
        OpenAI,
        DeepSeek
    }

    [Header("LLM Provider")]
    public LLMProvider provider = LLMProvider.DeepSeek;

    [Header("API Settings")]
    [TextArea]
    public string apiKey;

    public string model = "deepseek-chat";

    public string endpoint = "https://api.deepseek.com/v1/chat/completions";

    private const string k_PlannerPrompt =
        @"You are an action planner for a Unity game.

        The user will describe what they want to do. You will also receive a list of available actions and their parameters.

        Your job is to select ONE action and output a JSON object describing it.

        Rules:
        1. Output ONLY valid JSON. No explanation.
        2. JSON format must be:
        {
        ""type"": ""<action_type>"",
        ""args"": { ... }
        }
        3. ""type"" must match one of the available actions.
        4. ""args"" must contain required parameters.
        5. Optional parameters should use defaults unless specified.
        6. If the intent cannot be fulfilled, output:
        {""error"": true, ""reason"": ""<explanation>""}";

    void Start()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("[LLMProcessorOnDevice_Case2] API Key is empty!");
        }

        Debug.Log($"[LLMProcessorOnDevice_Case2] Provider: {provider}");
        Debug.Log($"[LLMProcessorOnDevice_Case2] Model: {model}");
        Debug.Log($"[LLMProcessorOnDevice_Case2] Endpoint: {endpoint}");
    }

    public void ProcessPrompt(GameObject obj, string userIntent)
    {
        string handlersDescription = PrintHandlers(obj);

        if (string.IsNullOrEmpty(handlersDescription))
        {
            Debug.LogWarning("[LLMProcessorOnDevice_Case2] No handlers found on target object.");
            return;
        }

        string userMessage =
            $@"User intent: {userIntent}

            Available actions:
            {handlersDescription}";

        StartCoroutine(SendLLMRequest(obj, userMessage));
    }

    private IEnumerator SendLLMRequest(GameObject obj, string userMessage)
    {
        var requestBody = new JObject
        {
            ["model"] = model,
            ["messages"] = new JArray
            {
                new JObject { ["role"] = "system", ["content"] = k_PlannerPrompt },
                new JObject { ["role"] = "user", ["content"] = userMessage }
            },
            ["temperature"] = 0
        };

        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody.ToString());

        using var request = new UnityWebRequest(endpoint, "POST");

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

        request.timeout = 30;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LLMProcessorOnDevice_Case2] API request failed: {request.error}");
            Debug.LogError(request.downloadHandler.text);
            yield break;
        }

        string responseText = request.downloadHandler.text;

        Debug.Log($"[LLMProcessorOnDevice_Case2] Raw response: {responseText}");

        string actionJson;

        try
        {
            var response = JObject.Parse(responseText);
            actionJson = response["choices"]?[0]?["message"]?["content"]?.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessorOnDevice_Case2] Failed to parse response: {e.Message}");
            yield break;
        }

        if (string.IsNullOrEmpty(actionJson))
        {
            Debug.LogError("[LLMProcessorOnDevice_Case2] Empty response from LLM.");
            yield break;
        }

        actionJson = ExtractJson(actionJson);

        Debug.Log($"[LLMProcessorOnDevice_Case2] Action JSON: {actionJson}");

        try
        {
            var parsed = JObject.Parse(actionJson);

            if (parsed["error"] != null)
            {
                string reason = parsed["reason"]?.ToString() ?? "Unknown reason";
                Debug.LogWarning($"[LLMProcessorOnDevice_Case2] Planner error: {reason}");
                yield break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessorOnDevice_Case2] Invalid JSON: {actionJson}");
            Debug.LogError(e.Message);
            yield break;
        }

        ExecuteJson(obj, actionJson);
    }

    private string ExtractJson(string text)
    {
        int startArray = text.IndexOf('[');
        int startObj = text.IndexOf('{');

        bool isArray = startArray >= 0 && (startObj < 0 || startArray < startObj);

        if (isArray)
        {
            int end = text.LastIndexOf(']');
            return text.Substring(startArray, end - startArray + 1);
        }
        else
        {
            int end = text.LastIndexOf('}');
            return text.Substring(startObj, end - startObj + 1);
        }
    }

    private string PrintHandlers(GameObject obj)
    {
        var handlers = obj.GetComponents<IActionHandler>();

        if (handlers.Length == 0)
        {
            Debug.Log($"[ActionDebugger] '{obj.name}' has no IActionHandler.");
            return "";
        }

        var sb = new StringBuilder();

        sb.AppendLine($"'{obj.name}' — {handlers.Length} handler(s):");

        foreach (var handler in handlers)
        {
            var specs = handler.GetActionSpecs();

            sb.AppendLine($"Handler: {handler.GetType().Name}");

            foreach (var spec in specs)
            {
                sb.AppendLine($"[{spec.type}] {spec.summary}");

                foreach (var arg in spec.args)
                {
                    string req = arg.required ? "required" : "optional";
                    string def = arg.defaultValue != null ? $", default={arg.defaultValue}" : "";

                    sb.AppendLine($"- {arg.name} ({arg.argType}, {req}{def})");
                }
            }
        }

        return sb.ToString();
    }

    private void ExecuteJson(GameObject obj, string json)
    {
        JObject root;

        try
        {
            root = JObject.Parse(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[ActionDebugger] Invalid JSON: {e.Message}");
            return;
        }

        string actionType = root["type"]?.ToString();

        if (string.IsNullOrEmpty(actionType))
        {
            Debug.LogError("[ActionDebugger] JSON missing 'type'.");
            return;
        }

        string argsJson = root["args"]?.ToString() ?? "{}";

        var context = new ExecutionContext(obj);
        var handlers = obj.GetComponents<IActionHandler>();

        foreach (var handler in handlers)
        {
            if (!handler.CanHandle(actionType))
                continue;

            var result = handler.Execute(actionType, argsJson, context);

            if (result.success)
                Debug.Log($"[ActionDebugger] '{obj.name}' -> {actionType} executed.");
            else
                Debug.LogWarning($"[ActionDebugger] '{obj.name}' -> {actionType} failed.");

            return;
        }

        Debug.LogWarning($"[ActionDebugger] '{obj.name}' has no handler for '{actionType}'.");
    }
}