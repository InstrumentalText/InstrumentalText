using System;
using System.Collections.Generic;

/// <summary>
/// All condition type definitions — specs for LLM consumption.
/// Default: If no condition is specified, use "temporal.once" with delay = 0 (immediate execution).
/// </summary>
public static class ConditionDefinitions
{
    public static readonly List<ConditionSpec> All = new()
    {
        // ── Temporal ──

        new ConditionSpec
        {
            type = "temporal.once",
            category = "temporal",
            summary = "Fire once after a delay (DEFAULT)",
            description = "Returns true once when the specified delay has elapsed since the start time. This is the default condition — if the user does not specify any condition, use this with delay = 0 for immediate execution.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "delay",
                    argType = "float",
                    required = true,
                    description = "Seconds to wait before triggering",
                    constraints = new ArgConstraints { min = 0f, unit = "seconds" }
                }
            },
            examples = new List<string>
            {
                "{ \"conditionType\": \"temporal.once\", \"args\": { \"delay\": 5.0 } }"
            }
        },

        new ConditionSpec
        {
            type = "temporal.loop",
            category = "temporal",
            summary = "Fire repeatedly at an interval",
            description = "Returns true each time the interval has elapsed since the last trigger. Can be limited to a max count.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "interval",
                    argType = "float",
                    required = true,
                    description = "Seconds between each trigger",
                    constraints = new ArgConstraints { min = 0.01f, unit = "seconds" }
                },
                new ArgSpec
                {
                    name = "maxCount",
                    argType = "int",
                    required = false,
                    description = "Maximum trigger count. 0 or omitted means infinite.",
                    defaultValue = "0",
                    constraints = new ArgConstraints { min = 0 }
                }
            },
            examples = new List<string>
            {
                "{ \"conditionType\": \"temporal.loop\", \"args\": { \"interval\": 2.0, \"maxCount\": 5 } }"
            }
        },

        // ── Spatial ──

        new ConditionSpec
        {
            type = "spatial.onEnter",
            category = "spatial",
            summary = "Object A enters object B's trigger zone",
            description = "Returns true when subjectA is inside subjectB's trigger collider (transition from outside to inside). SubjectB needs a Collider with isTrigger. SubjectA and subjectB are assigned in the Inspector.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{ \"conditionType\": \"spatial.onEnter\" }"
            }
        },

        new ConditionSpec
        {
            type = "spatial.onExit",
            category = "spatial",
            summary = "Object A exits object B's trigger zone",
            description = "Returns true when subjectA leaves subjectB's trigger collider (transition from inside to outside). SubjectB needs a Collider with isTrigger. SubjectA and subjectB are assigned in the Inspector.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{ \"conditionType\": \"spatial.onExit\" }"
            }
        },

        new ConditionSpec
        {
            type = "spatial.onCollision",
            category = "spatial",
            summary = "Object A collides with object B",
            description = "Returns true when subjectA is in physical contact with subjectB. Both need Colliders, at least one needs a Rigidbody. SubjectA and subjectB are assigned in the Inspector.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{ \"conditionType\": \"spatial.onCollision\" }"
            }
        },

        new ConditionSpec
        {
            type = "spatial.distance",
            category = "spatial",
            summary = "Two objects are within a distance threshold",
            description = "Returns true when the distance between subjectA and subjectB is less than or equal to the threshold. SubjectA and subjectB are assigned in the Inspector.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "threshold",
                    argType = "float",
                    required = true,
                    description = "Maximum distance to satisfy the condition",
                    constraints = new ArgConstraints { min = 0f, unit = "meters" }
                }
            },
            examples = new List<string>
            {
                "{ \"conditionType\": \"spatial.distance\", \"args\": { \"threshold\": 2.0 } }"
            }
        },

        new ConditionSpec
        {
            type = "spatial.distanceExit",
            category = "spatial",
            summary = "Two objects move beyond a distance threshold",
            description = "Fires once when the distance between subjectA and subjectB exceeds the threshold (transition from within to outside). SubjectA and subjectB are assigned in the Inspector.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "threshold",
                    argType = "float",
                    required = true,
                    description = "Distance threshold to trigger when exceeded",
                    constraints = new ArgConstraints { min = 0f, unit = "meters" }
                }
            },
            examples = new List<string>
            {
                "{ \"conditionType\": \"spatial.distanceExit\", \"args\": { \"threshold\": 2.0 } }"
            }
        }
    };
}

[Serializable]
public class ConditionSpec
{
    public string type;        // e.g., "temporal.once", "spatial.onEnter"
    public string category;    // "temporal" or "spatial"
    public string summary;
    public string description;
    public List<ArgSpec> args = new(); // Reuses ArgSpec from ActionSystem
    public List<string> examples = new();
}
