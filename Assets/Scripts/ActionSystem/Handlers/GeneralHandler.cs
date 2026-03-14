using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

public class GeneralHandler : MonoBehaviour, IActionHandler
{
    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "transform.modify",
            summary = "Modify target transform",
            description = "Set or offset position/rotation/scale in world or local space.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "mode",
                    argType = "enum",
                    required = true,
                    description = "absolute sets values directly; relative adds offsets.",
                    constraints = new ArgConstraints { enumValues = new List<string>{ "absolute", "relative" } },
                    defaultValue = "absolute"
                },
                new ArgSpec
                {
                    name = "space",
                    argType = "enum",
                    required = true,
                    description = "world uses global coordinates; local uses local coordinates.",
                    constraints = new ArgConstraints { enumValues = new List<string>{ "world", "local" } },
                    defaultValue = "world"
                },
                new ArgSpec
                {
                    name = "position",
                    argType = "object",
                    required = false,
                    description = "Position vector (x,y,z). Unit: meters."
                },
                new ArgSpec
                {
                    name = "rotation",
                    argType = "object",
                    required = false,
                    description = "Rotation euler angles (x,y,z). Unit: degrees."
                },
                new ArgSpec
                {
                    name = "scale",
                    argType = "object",
                    required = false,
                    description = "Scale vector (x,y,z)."
                }
            },
            examples = new List<string>
            {
                "{\"type\":\"transform.modify\",\"args\":{\"mode\":\"relative\",\"space\":\"world\",\"position\":{\"x\":0,\"y\":0.1,\"z\":0}}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs()
    {
        return actionSpecs;
    }

    public bool CanHandle(string actionType)
    {
        return actionType == "transform.modify";
    }

    public ActionResult Execute(string actionType, string argsJson, ExecutionContext target)
    {
        var spec = actionSpecs.Find(s => s.type == actionType);
        if (spec == null) return new ActionResult{success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}"};

        if(target == null || target.Target == null) return new ActionResult{success = false, errorCode = "INVALID_TARGET", message = $"Target is null"};


        JObject argsObj;
        try { argsObj = JObject.Parse(argsJson ?? "{}");}
        catch (Exception e) {return new ActionResult{success = false, errorCode = "INVALID_JSON", message = e.Message};}

        switch(spec.type)
        {
            case "transform.modify":
                return HandleTransformModify(spec, argsObj, target);
            default:
                return new ActionResult{success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}"};
        }
    }

    private ActionResult HandleTransformModify(ActionSpec spec, JObject argsObj, ExecutionContext target)
    {
        foreach(var arg in spec.args)
        {
            var token = argsObj[arg.name];

            if (token == null)
            {
                if(arg.required && arg.defaultValue == null)
                    return new ActionResult{success = false, errorCode = "MISSING_ARG", message = $"Missing required arg: {arg.name}"};
                continue;
            }

            if (arg.argType == "enum" && arg.constraints?.enumValues != null)
            {
                string val = token.ToString();
                if (!arg.constraints.enumValues.Contains(val))
                    return new ActionResult{success = false, errorCode = "INVALID_ENUM", message = $"Invalid value '{val}' for arg '{arg.name}'. Expected: {string.Join(", ", arg.constraints.enumValues)}"};
            }
        }

        // Extract values
        string mode = argsObj["mode"]?.ToString() ?? "absolute";
        string space = argsObj["space"]?.ToString() ?? "world";
        Vector3? position = ParseVector3(argsObj["position"]);
        Vector3? rotation = ParseVector3(argsObj["rotation"]);
        Vector3? scale = ParseVector3(argsObj["scale"]);

        // Apply to target transform
        Transform t = target.Target.transform;
        bool isAbsolute = mode == "absolute";
        bool isWorld = space == "world";

        if (position.HasValue)
        {
            Vector3 pos = position.Value;
            if (isAbsolute)
            {
                if (isWorld) t.position = pos;
                else t.localPosition = pos;
            }
            else
            {
                if (isWorld) t.position += pos;
                else t.localPosition += pos;
            }
        }

        if (rotation.HasValue)
        {
            Vector3 rot = rotation.Value;
            if (isAbsolute)
            {
                if (isWorld) t.eulerAngles = rot;
                else t.localEulerAngles = rot;
            }
            else
            {
                t.Rotate(rot, isWorld ? Space.World : Space.Self);
            }
        }

        if (scale.HasValue)
        {
            Vector3 scl = scale.Value;
            if (isAbsolute) t.localScale = scl;
            else t.localScale += scl;
        }

        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private Vector3? ParseVector3(JToken token)
    {
        if (token == null || token.Type != JTokenType.Object) return null;
        var obj = (JObject)token;
        return new Vector3(
            obj["x"]?.Value<float>() ?? 0f,
            obj["y"]?.Value<float>() ?? 0f,
            obj["z"]?.Value<float>() ?? 0f
        );
    }
}
