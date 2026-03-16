using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class LightHandler : MonoBehaviour, IActionHandler
{
    [SerializeField]
    private Light lightComponent;
    [SerializeField]
    private Renderer bulbRenderer;
    [SerializeField]
    private Color emissionBaseColor = Color.white;

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "light.on",
            summary = "Turn on the light",
            description = "Enable the light if it is currently off.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"light.on\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "light.off",
            summary = "Turn off the light",
            description = "Disable the light if it is currently on.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"light.off\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "light.toggle",
            summary = "Toggle the light on/off",
            description = "Flip the current light state: if on, turn off; if off, turn on.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"light.toggle\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "light.intensity",
            summary = "Adjust light intensity",
            description = "Set or offset the light intensity value.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "mode",
                    argType = "enum",
                    required = true,
                    description = "absolute sets intensity directly; relative adds an offset to current intensity.",
                    constraints = new ArgConstraints { enumValues = new List<string>{ "absolute", "relative" } },
                    defaultValue = "absolute"
                },
                new ArgSpec
                {
                    name = "value",
                    argType = "float",
                    required = true,
                    description = "The intensity value to set or offset.",
                    constraints = new ArgConstraints { min = 0f }
                }
            },
            examples = new List<string>
            {
                "{\"type\":\"light.intensity\",\"args\":{\"mode\":\"absolute\",\"value\":2.0}}",
                "{\"type\":\"light.intensity\",\"args\":{\"mode\":\"relative\",\"value\":-0.5}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs()
    {
        return actionSpecs;
    }

    public bool CanHandle(string actionType)
    {
        return actionType == "light.on" || actionType == "light.off" || actionType == "light.toggle" || actionType == "light.intensity";
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
            case "light.on":
                return HandleLightOn();
            case "light.off":
                return HandleLightOff();
            case "light.toggle":
                return HandleLightToggle();
            case "light.intensity":
                return HandleLightIntensity(argsObj);
            default:
                return new ActionResult{success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}"};
        }
    }

    private ActionResult HandleLightOn()
    {
        if (lightComponent == null)
            return new ActionResult{success = false, errorCode = "NO_LIGHT", message = "Light component not found."};

        if (lightComponent.enabled)
            return new ActionResult{success = true, errorCode = "", message = "Light is already on."};

        lightComponent.enabled = true;
        UpdateBulbEmission();
        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private ActionResult HandleLightOff()
    {
        if (lightComponent == null)
            return new ActionResult{success = false, errorCode = "NO_LIGHT", message = "Light component not found."};

        if (!lightComponent.enabled)
            return new ActionResult{success = true, errorCode = "", message = "Light is already off."};

        lightComponent.enabled = false;
        UpdateBulbEmission();
        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private ActionResult HandleLightToggle()
    {
        if (lightComponent == null)
            return new ActionResult{success = false, errorCode = "NO_LIGHT", message = "Light component not found."};

        lightComponent.enabled = !lightComponent.enabled;
        UpdateBulbEmission();
        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private ActionResult HandleLightIntensity(JObject argsObj)
    {
        if (lightComponent == null)
            return new ActionResult{success = false, errorCode = "NO_LIGHT", message = "Light component not found."};

        var valueToken = argsObj["value"];
        if (valueToken == null)
            return new ActionResult{success = false, errorCode = "MISSING_ARG", message = "Missing required arg: value"};

        float value = valueToken.Value<float>();
        string mode = argsObj["mode"]?.ToString() ?? "absolute";

        if (mode == "absolute")
        {
            if (value < 0f)
                return new ActionResult{success = false, errorCode = "INVALID_VALUE", message = "Absolute intensity cannot be negative."};
            lightComponent.intensity = value;
        }
        else
        {
            float newIntensity = lightComponent.intensity + value;
            if (newIntensity < 0f) newIntensity = 0f;
            lightComponent.intensity = newIntensity;
        }

        UpdateBulbEmission();
        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private void UpdateBulbEmission()
    {
        if (bulbRenderer == null) return;

        var mat = bulbRenderer.material;
        if (lightComponent != null && lightComponent.enabled)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emissionBaseColor * lightComponent.intensity);
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.black);
        }
    }
}
