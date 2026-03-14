using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using DoorScript;

public class DoorHandler : MonoBehaviour, IActionHandler
{
    [SerializeField] Material m_energiseMaterial;

    private Door doorScript;

    private void Awake()
    {
        doorScript = GetComponent<Door>();
    }

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "door.on",
            summary = "Open the door",
            description = "Open the door if it is currently closed.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"door.on\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "door.off",
            summary = "Close the door",
            description = "Close the door if it is currently open.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"door.off\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "door.energise",
            summary = "Switch door to an energetic material",
            description = "Replace the door's material with a more striking texture.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"door.energise\",\"args\":{}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs()
    {
        return actionSpecs;
    }

    public bool CanHandle(string actionType)
    {
        return actionType == "door.on" || actionType == "door.off" || actionType == "door.energise";
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
            case "door.on":
                return HandleDoorOn(spec, argsObj, target);
            case "door.off":
                return HandleDoorOff(spec, argsObj, target);
            case "door.energise":
                return HandleDoorEnergise(spec, argsObj, target);
            default:
                return new ActionResult{success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}"};
        }
    }

    private ActionResult HandleDoorOn(ActionSpec spec, JObject argsObj, ExecutionContext target)
    {
        if (doorScript == null)
            return new ActionResult{success = false, errorCode = "NO_DOOR", message = "Door component not found."};

        if (doorScript.open)
            return new ActionResult{success = true, errorCode = "", message = "Door is already open."};

        doorScript.OpenDoor();
        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private ActionResult HandleDoorOff(ActionSpec spec, JObject argsObj, ExecutionContext target)
    {
        if (doorScript == null)
            return new ActionResult{success = false, errorCode = "NO_DOOR", message = "Door component not found."};

        if (!doorScript.open)
            return new ActionResult{success = true, errorCode = "", message = "Door is already closed."};

        doorScript.OpenDoor();
        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private ActionResult HandleDoorEnergise(ActionSpec spec, JObject argsObj, ExecutionContext target)
    {
        if (m_energiseMaterial == null)
            return new ActionResult{success = false, errorCode = "NO_MATERIAL", message = "Energise material is not assigned."};

        var renderer = target.Target.GetComponentInChildren<Renderer>();
        if (renderer == null)
            return new ActionResult{success = false, errorCode = "NO_RENDERER", message = "No Renderer found on target."};

        renderer.material = m_energiseMaterial;
        return new ActionResult{success = true, errorCode = "", message = ""};
    }
}
