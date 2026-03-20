using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class CanvasHandler : MonoBehaviour, IActionHandler
{
    [SerializeField] private string compareWithFileName;
    [SerializeField] private string comparisonResultFileName;
    [SerializeField] private GameObject comparisonResultPrefab;
    [SerializeField] private Vector3 comparisonSpawnOffset = new Vector3(5f, 0f, 0f);

    private MarkdownRenderer markdownRenderer;

    private void Awake()
    {
        markdownRenderer = GetComponent<MarkdownRenderer>();
    }

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "canvas.page_up",
            summary = "Go to the previous page",
            description = "Navigate to the previous page of the document displayed on this canvas.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"canvas.page_up\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "canvas.page_down",
            summary = "Go to the next page",
            description = "Navigate to the next page of the document displayed on this canvas.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"canvas.page_down\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "canvas.compare_texts",
            summary = "Compare this document with another, generate explaination and analysis",
            description = "Analyze the current document and a second document, then produce a summary highlighting their shared themes and key commonalities. The result is displayed on a new canvas.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"canvas.compare_texts\",\"args\":{}}"
            }
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
            examples = new List<string>
            {
                "{\"type\":\"canvas.go_to_page\",\"args\":{\"page\":3}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs()
    {
        return actionSpecs;
    }

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

        if (actionType == "canvas.compare_texts")
            return ExecuteCompare();

        int page;
        switch (actionType)
        {
            case "canvas.page_up":
                page = markdownRenderer.CurrentPage - 1;
                break;
            case "canvas.page_down":
                page = markdownRenderer.CurrentPage + 1;
                break;
            case "canvas.go_to_page":
                var pageToken = argsObj["page"];
                if (pageToken == null)
                    return new ActionResult { success = false, errorCode = "MISSING_ARG", message = "Missing required argument: page" };
                page = pageToken.Value<int>();
                break;
            default:
                return new ActionResult { success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}" };
        }

        if (page < 1 || page > markdownRenderer.TotalPages)
            return new ActionResult { success = false, errorCode = "OUT_OF_RANGE", message = $"Page {page} out of range (1-{markdownRenderer.TotalPages})." };

        markdownRenderer.GoToPage(page);
        return new ActionResult { success = true, errorCode = "", message = $"Now on page {markdownRenderer.CurrentPage}/{markdownRenderer.TotalPages}" };
    }

    private ActionResult ExecuteCompare()
    {
        if (comparisonResultPrefab == null)
            return new ActionResult { success = false, errorCode = "NO_PREFAB", message = "Comparison result prefab is not assigned." };

        if (string.IsNullOrEmpty(comparisonResultFileName))
            return new ActionResult { success = false, errorCode = "NO_RESULT_FILE", message = "Comparison result file name is not assigned." };

        var instance = Instantiate(comparisonResultPrefab, transform.position + comparisonSpawnOffset, transform.rotation);
        var renderer = instance.GetComponent<MarkdownRenderer>();
        if (renderer == null)
            return new ActionResult { success = false, errorCode = "NO_RENDERER", message = "Prefab does not have a MarkdownRenderer component." };

        renderer.LoadMarkdown(comparisonResultFileName);
        return new ActionResult { success = true, errorCode = "", message = $"Comparison result loaded: {comparisonResultFileName}" };
    }
}
