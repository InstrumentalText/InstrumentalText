using System.IO;
using UnityEngine;
using TMPro;

public class MarkdownRenderer : MonoBehaviour
{
    [SerializeField] private string markdownFileName;

    private TextMeshProUGUI tmp;
    private int currentPage = 1;
    private int totalPages = 1;

    void Awake()
    {
        var canvas = GetComponentInChildren<Canvas>();
        if (canvas != null)
            tmp = canvas.GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(markdownFileName))
            LoadMarkdown(markdownFileName);
    }

    public void LoadMarkdown(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"[MarkdownRenderer] Markdown file not found: {path}");
            return;
        }

        string raw = File.ReadAllText(path);
        string richText = MarkdownToTMP.Convert(raw);

        if (tmp != null)
        {
            tmp.text = richText;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Page;

            tmp.ForceMeshUpdate();
            totalPages = tmp.textInfo.pageCount;
            currentPage = 1;
            tmp.pageToDisplay = currentPage;

            Debug.Log($"[MarkdownRenderer] Loaded {fileName}: {totalPages} page(s)");
        }
    }

    public void NextPage()
    {
        if (tmp == null || currentPage >= totalPages) return;
        currentPage++;
        tmp.pageToDisplay = currentPage;
    }

    public void PreviousPage()
    {
        if (tmp == null || currentPage <= 1) return;
        currentPage--;
        tmp.pageToDisplay = currentPage;
    }

    public void GoToPage(int page)
    {
        if (tmp == null) return;
        currentPage = Mathf.Clamp(page, 1, totalPages);
        tmp.pageToDisplay = currentPage;
    }

    public int CurrentPage => currentPage;
    public int TotalPages => totalPages;
}
