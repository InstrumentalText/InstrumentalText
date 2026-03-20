using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityPdfViewer;

public class PdfViewerHandler : MonoBehaviour, IActionHandler
{
    [SerializeField] private string pdfFileName;
    [SerializeField] private string extractResultFileName;
    [SerializeField] private GameObject extractResultObject;
    [SerializeField] private Vector3 extractSpawnOffset = new Vector3(2f, 0f, 0f);

    private PdfViewerUI pdfViewerUI;

    private void Awake()
    {
        pdfViewerUI = GetComponent<PdfViewerUI>();
    }

    private void Start()
    {
        if (pdfViewerUI != null && !string.IsNullOrEmpty(pdfFileName))
            pdfViewerUI.LoadPDF(pdfFileName);
    }

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "pdf.page_up",
            summary = "Go to the previous page",
            description = "Navigate to the previous page of the PDF document.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"pdf.page_up\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "pdf.page_down",
            summary = "Go to the next page",
            description = "Navigate to the next page of the PDF document.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"pdf.page_down\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "pdf.load",
            summary = "Load a PDF file",
            description = "Load and display a PDF file by its file name.",
            args = new List<ArgSpec>
            {
                new ArgSpec
                {
                    name = "fileName",
                    argType = "string",
                    required = true,
                    description = "The PDF file name to load."
                }
            },
            examples = new List<string>
            {
                "{\"type\":\"pdf.load\",\"args\":{\"fileName\":\"document.pdf\"}}"
            }
        },
        new ActionSpec
        {
            type = "pdf.extract",
            summary = "Extract key information from the PDF and display as a summary",
            description = "Extract and summarize the key information from the current PDF document. The result is displayed on a new canvas.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"pdf.extract\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "pdf.go_to_page",
            summary = "Jump to a specific page",
            description = "Navigate to a specific page number of the PDF document.",
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
                "{\"type\":\"pdf.go_to_page\",\"args\":{\"page\":3}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs()
    {
        return actionSpecs;
    }

    public bool CanHandle(string actionType)
    {
        return actionType == "pdf.page_up"
            || actionType == "pdf.page_down"
            || actionType == "pdf.go_to_page"
            || actionType == "pdf.load"
            || actionType == "pdf.extract";
    }

    public ActionResult Execute(string actionType, string argsJson, ExecutionContext target)
    {
        if (pdfViewerUI == null)
            return new ActionResult { success = false, errorCode = "NO_VIEWER", message = "PDFViewerUI not found." };

        JObject argsObj;
        try { argsObj = JObject.Parse(argsJson ?? "{}"); }
        catch (Exception e) { return new ActionResult { success = false, errorCode = "INVALID_JSON", message = e.Message }; }

        if (actionType == "pdf.extract")
            return ExecuteExtract();

        switch (actionType)
        {
            case "pdf.page_up":
                pdfViewerUI.PreviousPage();
                break;
            case "pdf.page_down":
                pdfViewerUI.NextPage();
                break;
            case "pdf.load":
                var fileToken = argsObj["fileName"];
                if (fileToken == null)
                    return new ActionResult { success = false, errorCode = "MISSING_ARG", message = "Missing required argument: fileName" };
                pdfViewerUI.LoadPDF(fileToken.Value<string>());
                break;
            case "pdf.go_to_page":
                var pageToken = argsObj["page"];
                if (pageToken == null)
                    return new ActionResult { success = false, errorCode = "MISSING_ARG", message = "Missing required argument: page" };
                pdfViewerUI.GoToPage(pageToken.Value<int>());
                break;
            default:
                return new ActionResult { success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}" };
        }

        return new ActionResult { success = true, errorCode = "", message = "" };
    }

    private ActionResult ExecuteExtract()
    {
        if (extractResultObject == null)
            return new ActionResult { success = false, errorCode = "NO_OBJECT", message = "Extract result object is not assigned." };

        if (string.IsNullOrEmpty(extractResultFileName))
            return new ActionResult { success = false, errorCode = "NO_RESULT_FILE", message = "Extract result file name is not assigned." };

        extractResultObject.SetActive(true);
        extractResultObject.transform.position = transform.position + extractSpawnOffset;

        var renderer = extractResultObject.GetComponent<MarkdownRenderer>();
        if (renderer == null)
            return new ActionResult { success = false, errorCode = "NO_RENDERER", message = "Extract result object does not have a MarkdownRenderer component." };

        renderer.LoadMarkdown(extractResultFileName);
        return new ActionResult { success = true, errorCode = "", message = $"Extract result loaded: {extractResultFileName}" };
    }
}
