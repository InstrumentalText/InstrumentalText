using UnityEngine;
using System.Text;
using Newtonsoft.Json.Linq;

/// <summary>
/// Central entry point for condition evaluation.
/// Routes condition type to the correct evaluation function and returns bool.
/// Stateless — all state (startTime, lastFireTime, wasInside, etc.) is managed by TriggerSystem.
/// </summary>
public class ConditionSystem : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(GetFormattedSpecs());
    }

    /// <summary>
    /// Main evaluation entry point. Routes to the correct condition check.
    /// </summary>
    /// <param name="conditionType">e.g., "temporal.once", "spatial.distance"</param>
    /// <param name="argsJson">Condition parameters as JSON</param>
    /// <param name="subjectA">Primary object (for spatial conditions)</param>
    /// <param name="subjectB">Secondary object (for spatial conditions)</param>
    /// <param name="startTime">When this condition was first registered (for temporal)</param>
    /// <param name="lastFireTime">When this condition last fired (for temporal.loop)</param>
    /// <returns>true if the condition is currently satisfied</returns>
    public bool Evaluate(
        string conditionType,
        string argsJson = null,
        GameObject subjectA = null,
        GameObject subjectB = null,
        float startTime = 0f,
        float lastFireTime = 0f)
    {
        var args = string.IsNullOrEmpty(argsJson) ? new JObject() : JObject.Parse(argsJson);

        return conditionType switch
        {
            // ── Temporal ──
            "temporal.once" => TemporalConditions.EvaluateOnce(
                startTime,
                args["delay"]?.Value<float>() ?? 0f),

            "temporal.loop" => TemporalConditions.EvaluateLoop(
                lastFireTime,
                args["interval"]?.Value<float>() ?? 1f),

            // ── Spatial ──
            "spatial.onEnter" => SpatialConditions.IsInsideTrigger(subjectA, subjectB),
            "spatial.onExit"  => SpatialConditions.IsInsideTrigger(subjectA, subjectB),
            // Note: onEnter/onExit both check IsInsideTrigger.
            // TriggerSystem detects the transition (false→true = enter, true→false = exit).

            "spatial.onCollision" => SpatialConditions.IsColliding(subjectA, subjectB),

            "spatial.distance" => SpatialConditions.IsWithinDistance(
                subjectA, subjectB,
                args["threshold"]?.Value<float>() ?? 1f),

            // distanceExit reuses IsWithinDistance — TriggerSystem detects true→false transition
            "spatial.distanceExit" => SpatialConditions.IsWithinDistance(
                subjectA, subjectB,
                args["threshold"]?.Value<float>() ?? 1f),

            _ => false
        };
    }

    /// <summary>
    /// Returns a debug string with the concrete values used in the last evaluation.
    /// </summary>
    public string GetDebugDetail(
        string conditionType,
        string argsJson = null,
        GameObject subjectA = null,
        GameObject subjectB = null,
        float startTime = 0f,
        float lastFireTime = 0f)
    {
        var args = string.IsNullOrEmpty(argsJson) ? new JObject() : JObject.Parse(argsJson);

        switch (conditionType)
        {
            case "temporal.once":
            {
                float delay = args["delay"]?.Value<float>() ?? 0f;
                float elapsed = Time.time - startTime;
                return $"elapsed={elapsed:F2}s target={delay:F2}s";
            }
            case "temporal.loop":
            {
                float interval = args["interval"]?.Value<float>() ?? 1f;
                float sinceLast = Time.time - lastFireTime;
                return $"sinceLast={sinceLast:F2}s interval={interval:F2}s";
            }
            case "spatial.onEnter":
            case "spatial.onExit":
            {
                return $"A={subjectA?.name} B={subjectB?.name} boundsContains={SpatialConditions.IsInsideTrigger(subjectA, subjectB)}";
            }
            case "spatial.onCollision":
            {
                return $"A={subjectA?.name} B={subjectB?.name} colliding={SpatialConditions.IsColliding(subjectA, subjectB)}";
            }
            case "spatial.distance":
            case "spatial.distanceExit":
            {
                float threshold = args["threshold"]?.Value<float>() ?? 1f;
                float dist = (subjectA != null && subjectB != null)
                    ? Vector3.Distance(subjectA.transform.position, subjectB.transform.position)
                    : -1f;
                return $"A={subjectA?.name} B={subjectB?.name} dist={dist:F2} threshold={threshold:F2}";
            }
            default:
                return "";
        }
    }

    /// <summary>
    /// Returns all condition specs formatted for LLM prompts.
    /// </summary>
    public string GetFormattedSpecs()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Available Conditions ===");

        foreach (var spec in ConditionDefinitions.All)
        {
            sb.AppendLine($"\n[{spec.type}] ({spec.category})");
            sb.AppendLine($"  Summary: {spec.summary}");
            sb.AppendLine($"  Description: {spec.description}");

            if (spec.args.Count > 0)
            {
                sb.AppendLine("  Args:");
                foreach (var arg in spec.args)
                {
                    string req = arg.required ? "required" : "optional";
                    sb.AppendLine($"    - {arg.name} ({arg.argType}, {req}): {arg.description}");
                }
            }

            if (spec.examples.Count > 0)
            {
                sb.AppendLine("  Examples:");
                foreach (var ex in spec.examples)
                    sb.AppendLine($"    {ex}");
            }
        }

        return sb.ToString();
    }
}
