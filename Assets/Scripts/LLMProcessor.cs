using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Text;

public class LLMProcessor : MonoBehaviour
{
    public enum LLMProvider
    {
        OpenAI,
        DeepSeek
    }

    [Header("LLM Provider")]
    public LLMProvider provider = LLMProvider.DeepSeek;

    private string apiKey;
    private string endpoint;
    private string model;

    private const string k_PlannerPrompt =
        @"You are an action planner for a Unity game. The user will describe what they want to do, and you will be given a list of available actions with their parameters.

        Your job is to output a JSON action object that fulfills the user's intent.

        Rules:
        1. Output ONLY valid JSON, no extra text or explanation.
        2. The JSON must have a ""type"" field matching one of the available action types.
        3. The JSON must have an ""args"" object containing the required parameters.
        4. Use default values for optional parameters unless the user specifies otherwise.
        5. If the user's intent cannot be fulfilled by any available action, output: {""error"": true, ""reason"": ""<explanation of why no action matched and what actions are available>""}";

    void Start()
    {
        // Provider configuration
        if (provider == LLMProvider.DeepSeek)
        {
            endpoint = "https://api.deepseek.com/v1/chat/completions";
            model = "deepseek-chat";
            apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
        }
        else
        {
            endpoint = "https://api.openai.com/v1/chat/completions";
            model = "gpt-4o";
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("[LLMProcessor] API key environment variable is not set.");
        }

        Debug.Log($"[LLMProcessor] Provider: {provider}, Model: {model}");
    }

    public void ProcessPrompt(GameObject obj, string userIntent)
    {
        string handlersDescription = PrintHandlers(obj);

        if (string.IsNullOrEmpty(handlersDescription))
        {
            Debug.LogWarning("[LLMProcessor] No handlers found on target object.");
            return;
        }

        string userMessage = $"User intent: {userIntent}\n\nAvailable actions:\n{handlersDescription}";
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

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LLMProcessor] API request failed: {request.error}\n{request.downloadHandler.text}");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        Debug.Log($"[LLMProcessor] Raw response: {responseText}");

        // Extract assistant message
        string actionJson;

        try
        {
            var response = JObject.Parse(responseText);
            actionJson = response["choices"]?[0]?["message"]?["content"]?.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessor] Failed to parse response: {e.Message}");
            yield break;
        }

        if (string.IsNullOrEmpty(actionJson))
        {
            Debug.LogError("[LLMProcessor] Empty response from LLM.");
            yield break;
        }

        actionJson = ExtractJson(actionJson);

        Debug.Log($"[LLMProcessor] Action JSON: {actionJson}");

        // Check planner error
        try
        {
            var parsed = JObject.Parse(actionJson);

            if (parsed["error"] != null)
            {
                string reason = parsed["reason"]?.ToString() ?? "Unknown reason";
                Debug.LogWarning($"[LLMProcessor] Planner error: {reason}");
                yield break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessor] LLM returned invalid JSON: {actionJson}");
            Debug.LogError($"[LLMProcessor] Exception {e.Message}");
            yield break;
        }

        ExecuteJson(obj, actionJson);
    }

    private string ExtractJson(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        return text.Substring(start, end - start + 1);
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

        sb.AppendLine($"[ActionDebugger] '{obj.name}' — {handlers.Length} handler(s):");

        foreach (var handler in handlers)
        {
            var specs = handler.GetActionSpecs();

            sb.AppendLine($"  Handler: {handler.GetType().Name} ({specs.Count} action(s))");

            foreach (var spec in specs)
            {
                sb.AppendLine($"    [{spec.type}] {spec.summary}");
                sb.AppendLine($"      Description: {spec.description}");

                foreach (var arg in spec.args)
                {
                    string req = arg.required ? "required" : "optional";
                    string def = arg.defaultValue != null ? $", default={arg.defaultValue}" : "";

                    sb.AppendLine($"      - {arg.name} ({arg.argType}, {req}{def}): {arg.description}");
                }

                if (spec.examples.Count > 0)
                {
                    sb.AppendLine($"      Examples:");

                    foreach (var ex in spec.examples)
                        sb.AppendLine($"        {ex}");
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
        catch (System.Exception e)
        {
            Debug.LogError($"[ActionDebugger] Invalid JSON: {e.Message}");
            return;
        }

        string actionType = root["type"]?.ToString();

        if (string.IsNullOrEmpty(actionType))
        {
            Debug.LogError("[ActionDebugger] JSON missing 'type' field.");
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
                Debug.Log($"[ActionDebugger] '{obj.name}' -> {actionType} executed successfully.");
            else
                Debug.LogWarning($"[ActionDebugger] '{obj.name}' -> {actionType} failed: [{result.errorCode}] {result.message}");

            return;
        }

        Debug.LogWarning($"[ActionDebugger] '{obj.name}' has no handler for '{actionType}'.");
    }
}