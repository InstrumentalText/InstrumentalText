using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CanvasHandler : MonoBehaviour, IActionHandler
{
    [Header("Compare setup")]
    [Tooltip("Compare 结果直接放预制体，Inspector 填好即可")]
    [SerializeField] private GameObject compareResultObject;
    [SerializeField] private Vector3 compareSpawnOffset = new Vector3(5f, 0f, 0f);

    private MarkdownRenderer markdownRenderer;

    private void Awake()
    {
        markdownRenderer = GetComponent<MarkdownRenderer>();
        if (markdownRenderer == null)
            Debug.LogWarning("[CanvasHandler] MarkdownRenderer not found on this GameObject.");
    }

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "canvas.page_up",
            summary = "Go to the previous page",
            description = "Navigate to the previous page of the document displayed on this canvas.",
            args = new List<ArgSpec>(),
            examples = new List<string> { "{\"type\":\"canvas.page_up\",\"args\":{}}" }
        },
        new ActionSpec
        {
            type = "canvas.page_down",
            summary = "Go to the next page",
            description = "Navigate to the next page of the document displayed on this canvas.",
            args = new List<ArgSpec>(),
            examples = new List<string> { "{\"type\":\"canvas.page_down\",\"args\":{}}" }
        },
        new ActionSpec
        {
            type = "canvas.compare_texts",
            summary = "Compare this document with another, display prepared result",
            description = "Display the comparison result (prepared in Inspector) on a new canvas.",
            args = new List<ArgSpec>(),
            examples = new List<string> { "{\"type\":\"canvas.compare_texts\",\"args\":{}}" }
        },
        new ActionSpec
        {
            type = "canvas.go_to_page",
            summary = "Jump to a specific page",
            description = "Navigate to a specific page number of the document displayed on this canvas.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "page",
                    argType = "int",
                    required = true,
                    description = "The page number to jump to (1-based).",
                    constraints = new ArgConstraints { min = 1 }
                }
            },
            examples = new List<string> { "{\"type\":\"canvas.go_to_page\",\"args\":{\"page\":3}}" }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs() => actionSpecs;

    public bool CanHandle(string actionType)
    {
        return actionType == "canvas.page_up"
            || actionType == "canvas.page_down"
            || actionType == "canvas.go_to_page"
            || actionType == "canvas.compare_texts";
    }

    public ActionResult Execute(string actionType, string argsJson, ExecutionContext target)
    {
        if (markdownRenderer == null)
            return new ActionResult { success = false, errorCode = "NO_RENDERER", message = "MarkdownRenderer not found." };

        JObject argsObj;
        try { argsObj = JObject.Parse(argsJson ?? "{}"); }
        catch (Exception e) { return new ActionResult { success = false, errorCode = "INVALID_JSON", message = e.Message }; }

        switch (actionType)
        {
            case "canvas.page_up":
                markdownRenderer.PreviousPage();
                HideCompare();
                return new ActionResult { success = true, message = $"Now on page {markdownRenderer.CurrentPage}/{markdownRenderer.TotalPages}" };

            case "canvas.page_down":
                markdownRenderer.NextPage();
                HideCompare();
                return new ActionResult { success = true, message = $"Now on page {markdownRenderer.CurrentPage}/{markdownRenderer.TotalPages}" };

            case "canvas.go_to_page":
                var pageToken = argsObj["page"];
                if (pageToken == null)
                    return new ActionResult { success = false, errorCode = "MISSING_ARG", message = "Missing required argument: page" };

                markdownRenderer.GoToPage(pageToken.Value<int>());
                HideCompare();
                return new ActionResult { success = true, message = $"Now on page {markdownRenderer.CurrentPage}/{markdownRenderer.TotalPages}" };

            case "canvas.compare_texts":
                return ExecuteCompare();

            default:
                return new ActionResult { success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}" };
        }
    }

    private ActionResult ExecuteCompare()
    {
        if (compareResultObject == null)
            return new ActionResult { success = false, errorCode = "NO_PREFAB", message = "Compare result object is not assigned." };

        compareResultObject.SetActive(true);
        compareResultObject.transform.position = transform.position + compareSpawnOffset;
        compareResultObject.transform.rotation = transform.rotation;

        Debug.Log("[CanvasHandler] Compare prefab activated");

        return new ActionResult { success = true, message = "Compare result displayed." };
    }

    private void HideCompare()
    {
        if (compareResultObject != null)
            compareResultObject.SetActive(false);
    }
}