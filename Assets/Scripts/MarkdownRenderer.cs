using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MarkdownRenderer : MonoBehaviour
{
    [Header("每个元素就是一页 Markdown 内容")]
    [TextArea(5, 20)]
    [SerializeField] private List<string> pageContents;

    private TextMeshProUGUI tmp;
    private int currentPageIndex = 0;

    void Awake()
    {
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
            tmp = canvas.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        if (pageContents != null && pageContents.Count > 0)
            LoadPage(0);
    }

    /// <summary>
    /// 渲染指定页
    /// </summary>
    public void LoadPage(int index)
    {
        if (tmp == null || pageContents == null || pageContents.Count == 0) return;

        index = Mathf.Clamp(index, 0, pageContents.Count - 1);
        currentPageIndex = index;

        string richText = MarkdownToTMP.Convert(pageContents[index]); // Markdown 转 TMP 方法
        tmp.text = richText;

        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Page;

        tmp.ForceMeshUpdate();
        tmp.pageToDisplay = 1; // TMP 每次只显示第一页
    }

    /// <summary>
    /// 直接用字符串内容渲染（CanvasHandler 调用）
    /// </summary>
    public void LoadMarkdownFromString(string content)
    {
        if (tmp == null) return;

        string richText = MarkdownToTMP.Convert(content);
        tmp.text = richText;

        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Page;
        tmp.ForceMeshUpdate();
        tmp.pageToDisplay = 1;
    }

    public void NextPage()
    {
        if (currentPageIndex + 1 >= pageContents.Count) return;
        LoadPage(currentPageIndex + 1);
    }

    public void PreviousPage()
    {
        if (currentPageIndex <= 0) return;
        LoadPage(currentPageIndex - 1);
    }

    public void GoToPage(int page) // 1-based
    {
        LoadPage(page - 1);
    }

    public int CurrentPage => currentPageIndex + 1;
    public int TotalPages => pageContents?.Count ?? 0;

    /// <summary>
    /// 可在 CanvasHandler 里动态注入多页内容
    /// </summary>
    public void SetPageContents(List<string> contents)
    {
        pageContents = contents;
        if (pageContents != null && pageContents.Count > 0)
            LoadPage(0);
    }
}