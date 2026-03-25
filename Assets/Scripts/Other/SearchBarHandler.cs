using System.Collections.Generic;
using UnityEngine;

public class SearchBarHandler : MonoBehaviour
{
    [Header("Mode Control")]
    public SearchModeHandler searchModeHandler;

    [Header("Debug")]
    public bool debug = true;

    [Header("Current Text Store")]
    public CurrentTextStore currentTextStore;

    public void OnTextSubmitted()
    {
        Debug.Log("[OnTextSubmitted] Triggered");

        if (searchModeHandler == null)
        {
            Debug.LogError("[SearchBarHandler] searchModeHandler 未绑定！");
            return;
        }

        if (!searchModeHandler.IsSearchActive())
        {
            if (debug) Debug.Log("[SearchBarHandler] 非 Search 模式 → 忽略提交");
            return;
        }

        if (currentTextStore == null)
        {
            Debug.LogError("[SearchBarHandler] CurrentTextStore 未设置！");
            return;
        }

        string searchText = currentTextStore.CurrentText?.Trim();

        if (string.IsNullOrEmpty(searchText))
        {
            Debug.Log("[OnTextSubmitted] 输入为空 → 清除高亮");
            ClearAllHighlights();
            return;
        }

        if (debug) Debug.Log($"[OnTextSubmitted] 搜索内容 = '{searchText}'");

        if (TextObjectManager.Instance == null)
        {
            Debug.LogError("[SearchBarHandler] TextObjectManager 未找到！");
            return;
        }

        var allTextObjects = TextObjectManager.Instance.GetAllTextObjects();
        List<GameObject> matchedObjects = new List<GameObject>();

        ClearAllHighlights();

        foreach (var obj in allTextObjects)
        {
            if (obj == null) continue;

            CurrentTextStore objTextStore = obj.GetComponent<CurrentTextStore>();
            if (objTextStore == null) continue;

            string objText = objTextStore.CurrentText;
            if (string.IsNullOrEmpty(objText)) continue;

            if (objText.IndexOf(searchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                matchedObjects.Add(obj);
                if (debug) Debug.Log($"[SearchBarHandler] 匹配成功 → {obj.name} ({objText})");
            }
        }

        foreach (var obj in matchedObjects)
        {
            Transform highlight = obj.transform.Find("Highlight");
            if (highlight != null)
            {
                highlight.gameObject.SetActive(true);
            }
            else
            {
                if (debug) Debug.LogWarning($"[SearchBarHandler] {obj.name} 没有 Highlight 子物体！");
            }
        }

        if (debug) Debug.Log($"[SearchBarHandler] 总匹配数量: {matchedObjects.Count}");
    }


    void ClearAllHighlights()
    {
        if (TextObjectManager.Instance == null) return;

        var allTextObjects = TextObjectManager.Instance.GetAllTextObjects();

        foreach (var obj in allTextObjects)
        {
            if (obj == null) continue;

            Transform highlight = obj.transform.Find("Highlight");
            if (highlight != null)
            {
                highlight.gameObject.SetActive(false);
            }
        }

        if (debug) Debug.Log("[SearchBarHandler] 已清除所有 Highlight");
    }
}