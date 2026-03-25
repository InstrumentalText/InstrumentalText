using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Collections.Generic;

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

    //需要inspector手动修改为ChatGPT的配置
    public string model = "deepseek-chat";
    public string endpoint = "https://api.deepseek.com/v1/chat/completions";

    [Header("Trigger Systems")]
    [SerializeField] private TriggerSystem triggerSystem;
    [SerializeField] private ConditionSystem conditionSystem;

    [SerializeField] private InstrumentRegistry instrumentRegistry;

    private const string k_PlannerPrompt =
@"You are an action planner for a Unity game.

The user will describe what they want to do.
You will also receive a list of available actions and conditions.

Your job is to output a JSON array of trigger entries.

Each entry must contain:
- ""condition"" (optional)
- ""action"" (required)

Rules:
1. Output ONLY JSON.
2. Output must be a JSON array.
3. Action types must match available actions.
4. Condition types must match available conditions.
5. If no condition is needed, omit it (default immediate execution).
6. For spatial.distance conditions, ensure that the ""threshold"" parameter is **at least 4** (Unity units/meters).
7. If impossible, output:
{""error"": true, ""reason"": ""<explanation>""}

Example:
[
 {
 ""condition"": { ""type"": ""temporal.once"", ""args"": { ""delay"": 2 } },
 ""action"": { ""type"": ""move"", ""args"": { ""direction"": ""up"" } }
 }
]";

    void Start()
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("[LLMProcessorOnDevice_Case2] API Key is empty!");
        }

        Debug.Log($"[LLMProcessorOnDevice_Case2] Provider: {provider}");
        Debug.Log($"[LLMProcessorOnDevice_Case2] Model: {model}");
    }

    public void ProcessPrompt(GameObject obj, string userIntent)
    {
        string handlersDescription = PrintHandlers(obj);

        if (string.IsNullOrEmpty(handlersDescription))
        {
            Debug.LogWarning("[LLMProcessorOnDevice_Case2] No handlers found.");
            return;
        }

        string conditionsDescription = conditionSystem.GetFormattedSpecs();

        string userMessage =
$@"User intent: {userIntent}

Available actions:
{handlersDescription}

{conditionsDescription}";

        StartCoroutine(SendLLMRequest(obj, userIntent, userMessage));
    }

    private IEnumerator SendLLMRequest(GameObject obj, string userIntent, string userMessage)
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

        string json;

        try
        {
            var response = JObject.Parse(responseText);
            json = response["choices"]?[0]?["message"]?["content"]?.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessorOnDevice_Case2] Parse error: {e.Message}");
            yield break;
        }

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("[LLMProcessorOnDevice_Case2] Empty response.");
            yield break;
        }

        json = ExtractJson(json);

        Debug.Log($"[LLMProcessorOnDevice_Case2] Extracted JSON: {json}");

        if (json.TrimStart().StartsWith("{"))
        {
            try
            {
                var parsed = JObject.Parse(json);
                if (parsed["error"] != null)
                {
                    string reason = parsed["reason"]?.ToString() ?? "Unknown reason";
                    Debug.LogWarning($"[LLMProcessorOnDevice_Case2] Planner error: {reason}");
                    yield break;
                }
            }
            catch { }
        }

        RegisterTriggers(obj, userIntent, json);
    }

    private string ExtractJson(string text)
    {
        int arrayStart = text.IndexOf('[');
        int objStart = text.IndexOf('{');

        bool isArray = arrayStart >= 0 && (objStart < 0 || arrayStart < objStart);

        if (isArray)
        {
            int end = text.LastIndexOf(']');
            return text.Substring(arrayStart, end - arrayStart + 1);
        }
        else
        {
            int end = text.LastIndexOf('}');
            return text.Substring(objStart, end - objStart + 1);
        }
    }

    private void RegisterTriggers(GameObject target, string userIntent, string json)
    {
        JArray entries;

        try
        {
            entries = JArray.Parse(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessorOnDevice_Case2] Failed to parse array: {e.Message}");
            return;
        }

        triggerSystem.UnregisterByTarget(target);

        var triggerIds = new List<int>();

        foreach (var token in entries)
        {
            string entryJson = token.ToString();
            int id = triggerSystem.Register(entryJson, target);

            if (id >= 0)
                triggerIds.Add(id);
        }

        if (triggerIds.Count > 0 && instrumentRegistry != null)
        {
            int instrumentId = instrumentRegistry.RegisterInstrument(target, userIntent, triggerIds);
            triggerSystem.PatchInstrumentId(triggerIds, instrumentId);
        }

        Debug.Log($"[LLMProcessorOnDevice_Case2] Registered {triggerIds.Count}/{entries.Count} trigger(s) on '{target.name}'");
    }

    private string PrintHandlers(GameObject obj)
    {
        var handlers = obj.GetComponents<IActionHandler>();

        if (handlers.Length == 0)
        {
            Debug.Log($"'{obj.name}' has no IActionHandler.");
            return "";
        }

        var sb = new StringBuilder();

        sb.AppendLine($"'{obj.name}' — {handlers.Length} handler(s):");

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

                if (spec.examples != null && spec.examples.Count > 0)
                {
                    sb.AppendLine($"      Examples:");
                    foreach (var ex in spec.examples)
                        sb.AppendLine($"        {ex}");
                }
            }
        }

        return sb.ToString();
    }
}