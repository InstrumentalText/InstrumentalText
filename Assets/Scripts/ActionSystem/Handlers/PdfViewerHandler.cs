using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

public class PdfViewerHandler : MonoBehaviour, IActionHandler
{
    [Header("Image-based PDF (XR Canvas)")]
    [Tooltip("PDF每页的图片，Inspector 按顺序填写")]
    [SerializeField] private List<Texture2D> pages;
    [SerializeField] private RawImage screenImage;

    [Header("Extract (Inspector prefab already configured)")]
    [Tooltip("Prefab 上挂 CanvasHandler + MarkdownRenderer，并填好每页内容")]
    [SerializeField] private GameObject extractResultObject;
    [SerializeField] private Vector3 extractSpawnOffset = new Vector3(0.4f, 0f, 0f);

    private int currentPage = 0;

    private void Start()
    {
        if (pages != null && pages.Count > 0 && screenImage != null)
        {
            currentPage = 0;
            screenImage.texture = pages[currentPage];
            Debug.Log("[PdfViewerHandler] Initialized first page");
        }
        else
        {
            Debug.LogWarning("[PdfViewerHandler] pages or screenImage not set");
        }
    }

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec { type = "pdf.page_up", summary = "Go to previous page", args = new List<ArgSpec>(), examples = new List<string>{ "{\"type\":\"pdf.page_up\",\"args\":{}}" } },
        new ActionSpec { type = "pdf.page_down", summary = "Go to next page", args = new List<ArgSpec>(), examples = new List<string>{ "{\"type\":\"pdf.page_down\",\"args\":{}}" } },
        new ActionSpec { type = "pdf.load", summary = "Load document", args = new List<ArgSpec>{ new ArgSpec{ name="fileName", argType="string", required=true } }, examples = new List<string>{ "{\"type\":\"pdf.load\",\"args\":{\"fileName\":\"doc.pdf\"}}" } },
        new ActionSpec { type = "pdf.extract", summary = "Extract summary", args = new List<ArgSpec>(), examples = new List<string>{ "{\"type\":\"pdf.extract\",\"args\":{}}" } },
        new ActionSpec { type = "pdf.go_to_page", summary = "Go to page", args = new List<ArgSpec>{ new ArgSpec{ name="page", argType="int", required=true } }, examples = new List<string>{ "{\"type\":\"pdf.go_to_page\",\"args\":{\"page\":3}}" } }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs() => actionSpecs;

    public bool CanHandle(string actionType) => actionType.StartsWith("pdf.");

    public ActionResult Execute(string actionType, string argsJson, ExecutionContext target)
    {
        JObject argsObj;
        try { argsObj = JObject.Parse(argsJson ?? "{}"); }
        catch (Exception e) { return new ActionResult { success=false, errorCode="INVALID_JSON", message=e.Message }; }

        if (actionType == "pdf.extract")
            return ExecuteExtract();

        switch (actionType)
        {
            case "pdf.load": return ExecuteLoad();
            case "pdf.page_up": return PageUp();
            case "pdf.page_down": return PageDown();
            case "pdf.go_to_page":
                var pageToken = argsObj["page"];
                if (pageToken == null)
                    return new ActionResult { success=false, errorCode="MISSING_ARG" };
                return GoToPage(pageToken.Value<int>());
            default: return new ActionResult { success=false, errorCode="UNKNOWN_ACTION" };
        }
    }

    private ActionResult ExecuteLoad()
    {
        if (pages == null || pages.Count==0 || screenImage==null)
            return new ActionResult { success=false, errorCode="NO_DATA" };
        currentPage = 0;
        screenImage.texture = pages[currentPage];
        return new ActionResult { success=true };
    }

    private ActionResult PageUp()
    {
        if (currentPage<=0) return new ActionResult { success=false, message="Already first page" };
        currentPage--;
        screenImage.texture = pages[currentPage];
        return new ActionResult { success=true };
    }

    private ActionResult PageDown()
    {
        if (currentPage>=pages.Count-1) return new ActionResult { success=false, message="Already last page" };
        currentPage++;
        screenImage.texture = pages[currentPage];
        return new ActionResult { success=true };
    }

    private ActionResult GoToPage(int page)
    {
        int index = page-1;
        if (index<0 || index>=pages.Count)
            return new ActionResult { success=false, errorCode="OUT_OF_RANGE" };
        currentPage = index;
        screenImage.texture = pages[currentPage];
        return new ActionResult { success=true };
    }

    private ActionResult ExecuteExtract()
    {
        if (extractResultObject == null)
            return new ActionResult { success=false, errorCode="NO_OBJECT" };

        extractResultObject.SetActive(true);
        extractResultObject.transform.position = transform.position + transform.right * extractSpawnOffset.x;
        extractResultObject.transform.rotation = transform.rotation;

        Debug.Log("[PdfViewerHandler] Extract prefab activated");

        return new ActionResult { success=true };
    }
}