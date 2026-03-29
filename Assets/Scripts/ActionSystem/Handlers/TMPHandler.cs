// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using Newtonsoft.Json.Linq;

// public class TMPHandler : MonoBehaviour, IActionHandler
// {
//     [Header("TMP setup")]
//     [Tooltip("Target TMP component (auto get if null)")]
//     [SerializeField] private TMP_Text tmpText;

//     [Tooltip("Default color to restore")]
//     [SerializeField] private Color defaultColor = Color.white;

//     private void Awake()
//     {
//         if (tmpText == null)
//             tmpText = GetComponent<TMP_Text>();

//         if (tmpText == null)
//             Debug.LogWarning("[TMPHandler] TMP_Text not found on this GameObject.");
//     }

//     private static readonly List<ActionSpec> actionSpecs = new()
//     {
//         new ActionSpec
//         {
//             type = "tmp.set_color",
//             summary = "Set text color",
//             description = "Set the color of the TMP text using RGB values (0-255).",
//             args = new List<ArgSpec>
//             {
//                 new ArgSpec
//                 {
//                     name = "r",
//                     argType = "int",
//                     required = true,
//                     description = "Red channel (0-255)",
//                     constraints = new ArgConstraints { min = 0, max = 255 }
//                 },
//                 new ArgSpec
//                 {
//                     name = "g",
//                     argType = "int",
//                     required = true,
//                     description = "Green channel (0-255)",
//                     constraints = new ArgConstraints { min = 0, max = 255 }
//                 },
//                 new ArgSpec
//                 {
//                     name = "b",
//                     argType = "int",
//                     required = true,
//                     description = "Blue channel (0-255)",
//                     constraints = new ArgConstraints { min = 0, max = 255 }
//                 }
//             },
//             examples = new List<string>
//             {
//                 "{\"type\":\"tmp.set_color\",\"args\":{\"r\":255,\"g\":0,\"b\":0}}"
//             }
//         },
//         new ActionSpec
//         {
//             type = "tmp.reset_color",
//             summary = "Reset text color",
//             description = "Reset the TMP text color to default.",
//             args = new List<ArgSpec>(),
//             examples = new List<string>
//             {
//                 "{\"type\":\"tmp.reset_color\",\"args\":{}}"
//             }
//         }
//     };

//     public IReadOnlyList<ActionSpec> GetActionSpecs() => actionSpecs;

//     public bool CanHandle(string actionType)
//     {
//         return actionType == "tmp.set_color"
//             || actionType == "tmp.reset_color";
//     }

//     public ActionResult Execute(string actionType, string argsJson, ExecutionContext target)
//     {
//         if (tmpText == null)
//             return new ActionResult { success = false, errorCode = "NO_TMP", message = "TMP_Text not found." };

//         JObject argsObj;
//         try { argsObj = JObject.Parse(argsJson ?? "{}"); }
//         catch (Exception e) { return new ActionResult { success = false, errorCode = "INVALID_JSON", message = e.Message }; }

//         switch (actionType)
//         {
//             case "tmp.set_color":
//                 return ExecuteSetColor(argsObj);

//             case "tmp.reset_color":
//                 tmpText.color = defaultColor;
//                 return new ActionResult { success = true, message = "Text color reset." };

//             default:
//                 return new ActionResult { success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}" };
//         }
//     }

//     private ActionResult ExecuteSetColor(JObject argsObj)
//     {
//         var rToken = argsObj["r"];
//         var gToken = argsObj["g"];
//         var bToken = argsObj["b"];

//         if (rToken == null || gToken == null || bToken == null)
//             return new ActionResult { success = false, errorCode = "MISSING_ARG", message = "Missing required arguments: r, g, b" };

//         int r = rToken.Value<int>();
//         int g = gToken.Value<int>();
//         int b = bToken.Value<int>();

//         Color newColor = new Color(r / 255f, g / 255f, b / 255f);
//         tmpText.color = newColor;

//         return new ActionResult
//         {
//             success = true,
//             message = $"Text color set to RGB({r},{g},{b})"
//         };
//     }
// }
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;

public class TMPHandler : MonoBehaviour, IActionHandler
{
    [Header("TMP setup")]
    [SerializeField] private TMP_Text tmpText;

    [SerializeField] private Color defaultColor = Color.white;

    private ColorTriggerZone triggerZone;

    private void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();

        triggerZone = GetComponentInChildren<ColorTriggerZone>();

        if (triggerZone != null)
            triggerZone.Initialize(tmpText, defaultColor);

        if (tmpText == null)
            Debug.LogWarning("[TMPHandler] TMP_Text not found.");

        if (triggerZone == null)
            Debug.LogWarning("[TMPHandler] ColorTriggerZone not found.");
    }

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "tmp.enable_mirror_color",
            summary = "Enable mirror color behavior",
            description = "Activate color mirroring from nearby objects.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"tmp.enable_mirror_color\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "tmp.disable_mirror_color",
            summary = "Disable mirror color behavior",
            description = "Disable color mirroring and reset color.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"tmp.disable_mirror_color\",\"args\":{}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs() => actionSpecs;

    public bool CanHandle(string actionType)
    {
        return actionType == "tmp.enable_mirror_color"
            || actionType == "tmp.disable_mirror_color";
    }

    public ActionResult Execute(string actionType, string argsJson, ExecutionContext context)
    {
        if (triggerZone == null)
            return new ActionResult
            {
                success = false,
                errorCode = "NO_ZONE",
                message = "ColorTriggerZone missing."
            };

        switch (actionType)
        {
            case "tmp.enable_mirror_color":
                triggerZone.SetActive(true);
                return new ActionResult
                {
                    success = true,
                    message = "Mirror color enabled."
                };

            case "tmp.disable_mirror_color":
                triggerZone.SetActive(false);
                return new ActionResult
                {
                    success = true,
                    message = "Mirror color disabled."
                };

            default:
                return new ActionResult
                {
                    success = false,
                    errorCode = "UNKNOWN_ACTION"
                };
        }
    }
}