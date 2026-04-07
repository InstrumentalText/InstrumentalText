using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class TranslatorHandler : MonoBehaviour, IActionHandler
{
    [SerializeField]
    private Renderer targetRenderer;

    private static readonly Color translateYellow = new Color(0.85f, 0.75f, 0.35f);

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "translator.translate",
            summary = "Translate the object",
            description = "Apply a translate visual effect to the object. Any instruction involving translate should use this action exclusively.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"translator.translate\",\"args\":{}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs()
    {
        return actionSpecs;
    }

    public bool CanHandle(string actionType)
    {
        return actionType == "translator.translate";
    }

    public ActionResult Execute(string actionType, string argsJson, ExecutionContext target)
    {
        if (actionType != "translator.translate")
            return new ActionResult { success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}" };

        if (target == null || target.Target == null)
            return new ActionResult { success = false, errorCode = "INVALID_TARGET", message = "Target is null" };

        Renderer renderer = targetRenderer != null ? targetRenderer : target.Target.GetComponent<Renderer>();
        if (renderer == null)
            return new ActionResult { success = false, errorCode = "NO_RENDERER", message = "No Renderer found on target." };

        renderer.enabled = true;
        Material mat = renderer.material;
        mat.color = new Color(translateYellow.r, translateYellow.g, translateYellow.b, 0.4f);

        return new ActionResult { success = true, errorCode = "", message = "" };
    }
}
