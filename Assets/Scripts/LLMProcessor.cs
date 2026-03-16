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

        Your job is to output a JSON array of trigger entries that fulfills the user's intent.

        Each trigger entry has:
        - ""condition"" (optional): { ""type"": ""..."", ""args"": {} }
          If omitted, defaults to immediate execution (temporal.once with delay=0).
        - ""action"" (required): { ""type"": ""..."", ""args"": {} }

        Rules:
        1. Output ONLY valid JSON, no extra text or explanation.
        2. The output must be a JSON array, even if there is only one entry.
        3. Each action ""type"" must match one of the available action types.
        4. Each condition ""type"" must match one of the available condition types.
        5. Use default values for optional parameters unless the user specifies otherwise.
        6. If the user's intent cannot be fulfilled, output: {""error"": true, ""reason"": ""<explanation>""}";

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
            contentJson = response["choices"]?[0]?["message"]?["content"]?.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessor] Failed to parse response: {e.Message}");
            yield break;
        }

        if (string.IsNullOrEmpty(contentJson))
        {
            Debug.LogError("[LLMProcessor] Empty response from LLM.");
            yield break;
        }

        actionJson = ExtractJson(actionJson);

        // Check for error response
        if (contentJson.TrimStart().StartsWith("{"))
        {
            try
            {
                var parsed = JObject.Parse(contentJson);
                if (parsed["error"] != null)
                {
                    string reason = parsed["reason"]?.ToString() ?? "Unknown reason";
                    Debug.LogWarning($"[LLMProcessor] Planner error: {reason}");
                    yield break;
                }
            }
            catch { }
        }

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
            Debug.LogError($"[LLMProcessor] Failed to parse trigger array: {e.Message}");
            return;
        }

        // Unregister all previous triggers for this target
        triggerSystem.UnregisterByTarget(target);

        int registered = 0;
        foreach (var token in entries)
        {
            string entryJson = token.ToString();
            int id = triggerSystem.Register(entryJson, target);
            if (id >= 0)
                registered++;
        }

        Debug.Log($"[LLMProcessor] Registered {registered}/{entries.Count} trigger(s) on '{target.name}'");
    }

    private string PrintHandlers(GameObject obj)
    {
        var handlers = obj.GetComponents<IActionHandler>();

        if (handlers.Length == 0)
        {
            Debug.Log($"[LLMProcessor] '{obj.name}' has no IActionHandler.");
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
