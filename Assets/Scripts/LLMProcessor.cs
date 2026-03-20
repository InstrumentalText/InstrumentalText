using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Text;

public class LLMProcessor : MonoBehaviour
{
    private TriggerSystem triggerSystem;
    private InstrumentRegistry instrumentRegistry;
    private ConditionSystem conditionSystem;

    private void Awake()
    {
        triggerSystem = FindAnyObjectByType<TriggerSystem>();
        instrumentRegistry = FindAnyObjectByType<InstrumentRegistry>();
        conditionSystem = FindAnyObjectByType<ConditionSystem>();
    }

    private string m_OpenaiAPIKey;

    private const string k_OpenaiEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string k_Model = "gpt-4o";

    private const string k_PlannerPrompt =
        @"You are an action planner for a Unity game. The user will describe what they want to do, and you will be given a list of available actions and conditions.

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
        m_OpenaiAPIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(m_OpenaiAPIKey))
        {
            Debug.LogError("[LLMProcessor] OPENAI_API_KEY environment variable is not set.");
        }
    }

    public void ProcessPrompt(GameObject obj, string userIntent)
    {
        string handlersDescription = PrintHandlers(obj);

        if (string.IsNullOrEmpty(handlersDescription))
        {
            Debug.LogWarning("[LLMProcessor] No handlers found on target object.");
            return;
        }

        string conditionsDescription = conditionSystem.GetFormattedSpecs();

        string userMessage = $"User intent: {userIntent}\n\nAvailable actions:\n{handlersDescription}\n\n{conditionsDescription}";
        StartCoroutine(SendOpenAIRequest(obj, userIntent, userMessage));
    }

    private IEnumerator SendOpenAIRequest(GameObject obj, string userIntent, string userMessage)
    {
        var requestBody = new JObject
        {
            ["model"] = k_Model,
            ["messages"] = new JArray
            {
                new JObject { ["role"] = "system", ["content"] = k_PlannerPrompt },
                new JObject { ["role"] = "user", ["content"] = userMessage }
            },
            ["temperature"] = 0
        };

        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody.ToString());

        using var request = new UnityWebRequest(k_OpenaiEndpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {m_OpenaiAPIKey}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LLMProcessor] OpenAI request failed: {request.error}\n{request.downloadHandler.text}");
            yield break;
        }

        string responseText = request.downloadHandler.text;
        Debug.Log($"[LLMProcessor] Raw response: {responseText}");

        string contentJson;
        try
        {
            var response = JObject.Parse(responseText);
            contentJson = response["choices"]?[0]?["message"]?["content"]?.ToString();
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessor] Failed to parse OpenAI response: {e.Message}");
            yield break;
        }

        if (string.IsNullOrEmpty(contentJson))
        {
            Debug.LogError("[LLMProcessor] Empty response from OpenAI.");
            yield break;
        }

        contentJson = ExtractJson(contentJson);
        Debug.Log($"[LLMProcessor] Extracted JSON: {contentJson}");

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

        RegisterTriggers(obj, userIntent, contentJson);
    }

    /// <summary>
    /// Extract JSON content (array or object) from markdown-wrapped text.
    /// </summary>
    private string ExtractJson(string text)
    {
        int arrayStart = text.IndexOf('[');
        int objStart = text.IndexOf('{');

        // Pick whichever comes first: array or object
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

    /// <summary>
    /// Register triggers as a single instrument.
    /// </summary>
    private void RegisterTriggers(GameObject target, string userIntent, string json)
    {
        JArray entries;
        try
        {
            entries = JArray.Parse(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LLMProcessor] Failed to parse trigger array: {e.Message}");
            return;
        }

        // Reserve an instrument ID first, then register triggers with it
        var triggerIds = new System.Collections.Generic.List<int>();
        // Use a temporary ID; InstrumentRegistry assigns the real one
        // We need the instrument ID before registering triggers, so we do a two-phase approach:
        // Phase 1: register triggers with instrumentId=0
        // Phase 2: create instrument, then patch the instrumentId on triggers
        foreach (var token in entries)
        {
            string entryJson = token.ToString();
            int id = triggerSystem.Register(entryJson, target);
            if (id >= 0)
                triggerIds.Add(id);
        }

        if (triggerIds.Count > 0)
        {
            int instrumentId = instrumentRegistry.RegisterInstrument(target, userIntent, triggerIds);
            triggerSystem.PatchInstrumentId(triggerIds, instrumentId);
        }

        Debug.Log($"[LLMProcessor] Registered {triggerIds.Count}/{entries.Count} trigger(s) on '{target.name}'");
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
}