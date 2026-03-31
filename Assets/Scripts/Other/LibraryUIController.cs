using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // ✅ 必须加这个

public class LibraryUIController : MonoBehaviour
{
    [Header("UI References")]
    public Transform content;
    public GameObject itemTemplate;

    [Header("Tag Settings")]
    public string textTag = "ItemText";

    [Header("Debug")]
    public bool debug = true;

    private List<GameObject> spawnedItems = new List<GameObject>();

    void Start()
    {
        // RefreshLibrary();
    }


    public void RefreshLibrary()
    {
        if (TextObjectManager.Instance == null)
        {
            Debug.LogWarning("[LibraryUI] TextObjectManager 未找到！");
            return;
        }

        ClearItems();

        var allTextObjects = TextObjectManager.Instance.GetAllTextObjects();

        HashSet<string> uniqueTexts = new HashSet<string>();

        foreach (var obj in allTextObjects)
        {
            if (obj == null) continue;

            var store = obj.GetComponent<CurrentTextStore>();
            if (store == null || string.IsNullOrEmpty(store.CurrentText)) continue;

            uniqueTexts.Add(store.CurrentText.Trim());
        }

        if (InteractionLibraryManager.Instance != null)
        {
            foreach (var record in InteractionLibraryManager.Instance.GetAllRecords())
            {
                if (!string.IsNullOrEmpty(record.prompt))
                    uniqueTexts.Add(record.prompt.Trim());
            }
        }

        List<string> textList = new List<string>(uniqueTexts);

        if (textList.Count == 0) return;

        if (debug)
            Debug.Log($"[LibraryUI] 生成 {textList.Count} 个 Item");


        List<GameObject> items = new List<GameObject>();

        items.Add(itemTemplate);

        for (int i = 1; i < textList.Count; i++)
        {
            GameObject newItem = Instantiate(itemTemplate, content);
            newItem.SetActive(true);
            items.Add(newItem);
        }

        for (int i = 0; i < items.Count; i++)
        {
            GameObject item = items[i];
            string text = textList[i];

            SetItemText(item, text);

            spawnedItems.Add(item);
        }
    }

    void SetItemText(GameObject item, string value)
    {
        Transform[] allChildren = item.GetComponentsInChildren<Transform>(true);

        foreach (var t in allChildren)
        {
            if (!t.CompareTag(textTag)) continue;

            TMP_Text tmp = t.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = value;
                if (debug) Debug.Log($"[LibraryUI] TMP_Text 设置: {value}");
                return;
            }

            Text uiText = t.GetComponent<Text>();
            if (uiText != null)
            {
                uiText.text = value;
                if (debug) Debug.Log($"[LibraryUI] UI.Text 设置: {value}");
                return;
            }

            Debug.LogWarning("[LibraryUI] 找到 ItemText 但没有 Text 组件！");
        }

        Debug.LogWarning("[LibraryUI] 没找到 Tag=ItemText 的物体！");
    }

    void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null && item != itemTemplate)
                Destroy(item);
        }

        spawnedItems.Clear();

        SetItemText(itemTemplate, "");
    }
}