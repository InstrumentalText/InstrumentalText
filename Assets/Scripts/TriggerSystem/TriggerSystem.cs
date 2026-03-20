using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class TriggerSystem : MonoBehaviour
{
    private ConditionSystem conditionSystem;
    private InstrumentRegistry instrumentRegistry;
    [SerializeField] private GameObject subjectA;
    [SerializeField] private GameObject subjectB;

    private void Awake()
    {
        conditionSystem = FindAnyObjectByType<ConditionSystem>();
        instrumentRegistry = FindAnyObjectByType<InstrumentRegistry>();
    }

    private int nextId = 1;
    private readonly List<TriggerEntry> activeTriggers = new();

    /// <summary>
    /// Register a trigger from JSON. Returns the trigger ID, or -1 on failure.
    /// The target GameObject is the object the user selected (clicked on).
    /// Expected format:
    /// {
    ///   "condition": { "type": "...", "args": {} },
    ///   "action": { "type": "...", "args": {} }
    /// }
    /// If "condition" is omitted, defaults to temporal.once with delay=0.
    /// </summary>
    public int Register(string json, GameObject target, int instrumentId = 0)
    {
        JObject root;
        try { root = JObject.Parse(json); }
        catch (System.Exception e)
        {
            Debug.LogError($"[TriggerSystem] Invalid JSON: {e.Message}");
            return -1;
        }

        var entry = new TriggerEntry { id = nextId++, instrumentId = instrumentId };

        // ── Parse condition ──
        var condition = root["condition"] as JObject;
        if (condition == null)
        {
            // Default: immediate execution
            entry.conditionType = "temporal.once";
            entry.conditionArgsJson = "{ \"delay\": 0 }";
        }
        else
        {
            entry.conditionType = condition["type"]?.ToString();
            if (string.IsNullOrEmpty(entry.conditionType))
            {
                Debug.LogError("[TriggerSystem] Condition missing 'type' field.");
                return -1;
            }

            entry.conditionArgsJson = condition["args"]?.ToString() ?? "{}";

            // Use Inspector-assigned subjects for spatial conditions
            if (entry.conditionType.StartsWith("spatial."))
            {
                entry.subjectA = subjectA;
                entry.subjectB = subjectB;

                if (entry.subjectA == null || entry.subjectB == null)
                {
                    Debug.LogError("[TriggerSystem] Spatial condition requires subjectA and subjectB to be assigned in Inspector.");
                    return -1;
                }
            }

            // Extract maxCount for loop
            if (entry.conditionType == "temporal.loop")
            {
                var condArgs = JObject.Parse(entry.conditionArgsJson);
                entry.maxCount = condArgs["maxCount"]?.Value<int>() ?? 0;
            }
        }

        // ── Parse action ──
        var action = root["action"] as JObject;
        if (action == null)
        {
            Debug.LogError("[TriggerSystem] Missing 'action' field.");
            return -1;
        }

        if (target == null)
        {
            Debug.LogError("[TriggerSystem] Target GameObject is null.");
            return -1;
        }

        entry.targetObject = target;

        entry.actionType = action["type"]?.ToString();
        if (string.IsNullOrEmpty(entry.actionType))
        {
            Debug.LogError("[TriggerSystem] Action missing 'type' field.");
            return -1;
        }

        entry.actionArgsJson = action["args"]?.ToString() ?? "{}";

        // ── Init runtime state ──
        entry.startTime = Time.time;
        entry.lastFireTime = Time.time;
        entry.fireCount = 0;
        entry.wasInside = false;
        entry.completed = false;

        activeTriggers.Add(entry);
        Debug.Log($"[TriggerSystem] Registered trigger #{entry.id}: {entry.conditionType} → {entry.actionType} on '{target.name}'");
        return entry.id;
    }

    public void Unregister(int id)
    {
        activeTriggers.RemoveAll(e => e.id == id);
    }

    /// <summary>
    /// Unregister all triggers bound to a specific target GameObject.
    /// </summary>
    public void UnregisterByTarget(GameObject target)
    {
        int removed = activeTriggers.RemoveAll(e => e.targetObject == target);
        if (removed > 0)
            Debug.Log($"[TriggerSystem] Unregistered {removed} trigger(s) on '{target.name}'");
    }

    /// <summary>
    /// Assign an instrument ID to a batch of triggers after registration.
    /// </summary>
    public void PatchInstrumentId(List<int> triggerIds, int instrumentId)
    {
        foreach (var entry in activeTriggers)
        {
            if (triggerIds.Contains(entry.id))
                entry.instrumentId = instrumentId;
        }
    }

    /// <summary>
    /// Unregister all triggers belonging to a specific instrument.
    /// </summary>
    public void UnregisterByInstrumentId(int instrumentId)
    {
        int removed = activeTriggers.RemoveAll(e => e.instrumentId == instrumentId);
        if (removed > 0)
            Debug.Log($"[TriggerSystem] Unregistered {removed} trigger(s) for instrument #{instrumentId}");
    }

    private void Update()
    {
        for (int i = activeTriggers.Count - 1; i >= 0; i--)
        {
            var e = activeTriggers[i];
            if (e.completed) continue;

            bool raw = conditionSystem.Evaluate(
                e.conditionType, e.conditionArgsJson,
                e.subjectA, e.subjectB,
                e.startTime, e.lastFireTime);

            string detail = conditionSystem.GetDebugDetail(
                e.conditionType, e.conditionArgsJson,
                e.subjectA, e.subjectB,
                e.startTime, e.lastFireTime);
            Debug.Log($"[TriggerSystem] Trigger #{e.id} [{e.conditionType}] → {e.actionType} on '{e.targetObject.name}' | raw={raw} wasInside={e.wasInside} | {detail}");

            switch (e.conditionType)
            {
                case "temporal.once":
                    if (raw)
                    {
                        ExecuteAction(e);
                        e.completed = true;
                    }
                    break;

                case "temporal.loop":
                    if (raw)
                    {
                        ExecuteAction(e);
                        e.lastFireTime = Time.time;
                        e.fireCount++;
                        if (e.maxCount > 0 && e.fireCount >= e.maxCount)
                            e.completed = true;
                    }
                    break;

                case "spatial.onEnter":
                case "spatial.onCollision":
                case "spatial.distance":
                    // Fire once on false → true transition
                    if (!e.wasInside && raw)
                        ExecuteAction(e);
                    e.wasInside = raw;
                    break;

                case "spatial.onExit":
                case "spatial.distanceExit":
                    // Fire once on true → false transition
                    if (e.wasInside && !raw)
                        ExecuteAction(e);
                    e.wasInside = raw;
                    break;
            }
        }

        // Clean up completed entries and notify registry
        for (int j = activeTriggers.Count - 1; j >= 0; j--)
        {
            if (!activeTriggers[j].completed) continue;
            var done = activeTriggers[j];
            if (done.instrumentId > 0 && instrumentRegistry != null)
                instrumentRegistry.OnTriggerCompleted(done.instrumentId, done.id);
            activeTriggers.RemoveAt(j);
        }
    }

    private void ExecuteAction(TriggerEntry entry)
    {
        var handlers = entry.targetObject.GetComponents<IActionHandler>();
        var context = new ExecutionContext(entry.targetObject);

        foreach (var handler in handlers)
        {
            if (!handler.CanHandle(entry.actionType)) continue;

            var result = handler.Execute(entry.actionType, entry.actionArgsJson, context);
            if (result.success)
                return;
                // Debug.Log($"[TriggerSystem] Trigger #{entry.id} fired: {entry.actionType} on '{entry.targetObject.name}'");
            else
                Debug.LogWarning($"[TriggerSystem] Trigger #{entry.id} action failed: [{result.errorCode}] {result.message}");
            return;
        }

        Debug.LogWarning($"[TriggerSystem] No handler for '{entry.actionType}' on '{entry.targetObject.name}'");
    }

}
