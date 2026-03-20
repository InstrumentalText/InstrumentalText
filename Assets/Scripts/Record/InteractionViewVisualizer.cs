// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class InteractionViewVisualizer : MonoBehaviour
// {
//     [Header("View Toggle")]
//     public bool openView = false;

//     [Header("Prefabs")]
//     public GameObject panelPrefab;

//     [Header("Line Settings")]
//     public Material lineMaterial;
//     public Color lineColor = Color.white;
//     public float lineWidth = 0.01f; // 可在 Inspector 调整

//     [Header("Layout")]
//     public float heightOffset = 0.0f;

//     [Header("Placement")]
//     public float spawnDistance = 2f; // 面板出现距离
//     public float rootPanelSpacing = 0.5f; // 根面板之间的基础间距
//     public float rootPanelOffsetRange = 0.05f; // 随机偏移范围

//     [Header("Radius Random")]
//     public float radiusMin = 0.3f;
//     public float radiusMax = 0.7f;

//     private List<GameObject> spawnedPanels = new List<GameObject>();
//     private List<GameObject> spawnedLines = new List<GameObject>();
//     private bool lastState = false;

//     void Update()
//     {
//         if (openView != lastState)
//         {
//             if (openView)
//                 ShowView();
//             else
//                 ClearView();

//             lastState = openView;
//         }
//     }

//     // =========================
//     // 主入口（语义 View）
//     // =========================
//     void ShowView()
//     {
//         ClearView();

//         var records = InteractionLibraryManager.Instance.GetAllRecords();

//         if (records == null || records.Count == 0)
//         {
//             Debug.Log("[View] Using Mock Data");
//             records = GetMockRecords();
//         }

//         if (records.Count == 0)
//         {
//             Debug.Log("[View] No records");
//             return;
//         }

//         HideAllTextObjects();

//         // 按 target 分组
//         Dictionary<string, List<InteractionRecord>> grouped = new Dictionary<string, List<InteractionRecord>>();
//         foreach (var r in records)
//         {
//             if (!grouped.ContainsKey(r.targetName))
//                 grouped[r.targetName] = new List<InteractionRecord>();
//             grouped[r.targetName].Add(r);
//         }

//         Transform cam = Camera.main.transform;
//         Vector3 forwardPos = cam.position + cam.forward * spawnDistance;

//         float angleStep = 360f / Mathf.Max(grouped.Count, 1);
//         float currentAngle = 0f;

//         foreach (var pair in grouped)
//         {
//             string targetName = pair.Key;
//             List<InteractionRecord> list = pair.Value;

//             // 生成 root panel 的位置：使用角度 + 随机半径偏移
//             float distance = rootPanelSpacing + Random.Range(0f, rootPanelOffsetRange);
//             Vector3 offset = new Vector3(Mathf.Cos(currentAngle * Mathf.Deg2Rad) * distance,
//                                          Random.Range(-0.2f, 0.2f), // 上下轻微偏移
//                                          Mathf.Sin(currentAngle * Mathf.Deg2Rad) * distance);
//             Vector3 targetCenterPos = forwardPos + offset;
//             currentAngle += angleStep;

//             GameObject centerPanel = CreateCenterPanel(targetName, targetCenterPos);
//             spawnedPanels.Add(centerPanel);

//             int count = list.Count;

//             // 每组内部 panel 布局：完整圆 + 随机半径
//             for (int i = 0; i < count; i++)
//             {
//                 float angle = i * Mathf.PI * 2f / count;
//                 float r = Random.Range(radiusMin, radiusMax);

//                 Vector3 pos = targetCenterPos + new Vector3(
//                     Mathf.Cos(angle) * r,
//                     Mathf.Sin(angle) * r,
//                     0
//                 );

//                 GameObject panel = CreatePromptPanel(list[i], pos);
//                 spawnedPanels.Add(panel);

//                 // 线条从中心连接
//                 CreateLine(panel.transform.position, centerPanel.transform.position);
//             }
//         }

//         Debug.Log("[View] Semantic Wall View Spawned");
//     }

//     // =========================
//     // 创建中心 Panel
//     // =========================
//     GameObject CreateCenterPanel(string targetName, Vector3 pos)
//     {
//         GameObject panel = Instantiate(panelPrefab, pos, Quaternion.identity);
//         panel.transform.forward = Camera.main.transform.forward;

//         TMP_Text text = panel.GetComponentInChildren<TMP_Text>();
//         if (text != null) text.text = targetName;

//         return panel;
//     }

//     // =========================
//     // 创建 Prompt Panel
//     // =========================
//     GameObject CreatePromptPanel(InteractionRecord record, Vector3 pos)
//     {
//         GameObject panel = Instantiate(panelPrefab, pos, Quaternion.identity);
//         panel.transform.forward = Camera.main.transform.forward;

//         TMP_Text text = panel.GetComponentInChildren<TMP_Text>();
//         if (text != null) text.text = record.prompt;

//         return panel;
//     }

//     // =========================
//     // 创建线条
//     // =========================
//     void CreateLine(Vector3 a, Vector3 b)
//     {
//         GameObject lineObj = new GameObject("Line");
//         LineRenderer lr = lineObj.AddComponent<LineRenderer>();

//         lr.material = lineMaterial;
//         lr.startColor = lineColor;
//         lr.endColor = lineColor;
//         lr.startWidth = lineWidth;
//         lr.endWidth = lineWidth;
//         lr.positionCount = 2;
//         lr.SetPosition(0, a);
//         lr.SetPosition(1, b);

//         spawnedLines.Add(lineObj);
//     }

//     // =========================
//     // 隐藏原 Text Objects
//     // =========================
//     void HideAllTextObjects()
//     {
//         foreach (var t in FindObjectsOfType<CurrentTextStore>())
//             t.gameObject.SetActive(false);
//     }

//     // =========================
//     // 显示原 Text Objects
//     // =========================
//     void ShowAllTextObjects()
//     {
//         foreach (var t in FindObjectsOfType<CurrentTextStore>())
//             t.gameObject.SetActive(true);
//     }

//     // =========================
//     // 清理
//     // =========================
//     void ClearView()
//     {
//         foreach (var p in spawnedPanels) Destroy(p);
//         foreach (var l in spawnedLines) Destroy(l);
//         spawnedPanels.Clear();
//         spawnedLines.Clear();
//         ShowAllTextObjects();
//         Debug.Log("[View] Cleared");
//     }

//     // =========================
//     // Mock 数据
//     // =========================
//     List<InteractionRecord> GetMockRecords()
//     {
//         List<InteractionRecord> mock = new List<InteractionRecord>();

//         // TV
//         mock.Add(new InteractionRecord("turn on", "TV"));
//         mock.Add(new InteractionRecord("turn off", "TV"));
//         mock.Add(new InteractionRecord("switch channel", "TV"));
//         mock.Add(new InteractionRecord("turn off after 2 hours", "TV"));
//         mock.Add(new InteractionRecord("increase volume", "TV"));
//         mock.Add(new InteractionRecord("decrease volume", "TV"));
//         mock.Add(new InteractionRecord("mute", "TV"));

//         // Air Conditioner
//         mock.Add(new InteractionRecord("turn on", "Air Conditioner"));
//         mock.Add(new InteractionRecord("turn off", "Air Conditioner"));
//         mock.Add(new InteractionRecord("increase temperature", "Air Conditioner"));
//         mock.Add(new InteractionRecord("decrease temperature", "Air Conditioner"));
//         mock.Add(new InteractionRecord("turn off at 2 AM", "Air Conditioner"));
//         mock.Add(new InteractionRecord("set to 24 degrees", "Air Conditioner"));
//         mock.Add(new InteractionRecord("enable eco mode", "Air Conditioner"));

//         // Computer
//         mock.Add(new InteractionRecord("send today's schedule to Alex", "Computer"));
//         mock.Add(new InteractionRecord("open email", "Computer"));
//         mock.Add(new InteractionRecord("start meeting", "Computer"));
//         mock.Add(new InteractionRecord("shutdown", "Computer"));
//         mock.Add(new InteractionRecord("restart", "Computer"));
//         mock.Add(new InteractionRecord("open browser", "Computer"));

//         return mock;
//     }
// }


using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionViewVisualizer : MonoBehaviour
{
    [Header("View Toggle")]
    public bool openView = false;

    [Header("Prefabs")]
    public GameObject panelPrefab;

    [Header("Line Settings")]
    public Material lineMaterial;
    public Color lineColor = Color.white;
    public float lineWidth = 0.01f; // 可在 Inspector 调整

    [Header("Layout")]
    public float heightOffset = 0.0f;

    [Header("Placement")]
    public float spawnDistance = 2f;          // root panel 出现距离
    public float rootPanelSpacingX = 1.0f;    // 列间距
    public float rootPanelSpacingY = 0.8f;    // 行间距
    public int maxPerRow = 4;                 // 每行最多 root panel 个数

    [Header("Radius Random")]
    public float radiusMin = 0.3f; // prompt panel 半径最小值
    public float radiusMax = 0.7f; // prompt panel 半径最大值

    private List<GameObject> spawnedPanels = new List<GameObject>();
    private List<GameObject> spawnedLines = new List<GameObject>();
    private bool lastState = false;

    void Update()
    {
        if (openView != lastState)
        {
            if (openView)
                ShowView();
            else
                ClearView();

            lastState = openView;
        }
    }

    // =========================
    // 主入口（语义 View）
    // =========================
    void ShowView()
{
    ClearView();

    var records = InteractionLibraryManager.Instance.GetAllRecords();
    if (records == null || records.Count == 0)
    {
        Debug.Log("[View] Using Mock Data");
        records = GetMockRecords();
    }

    if (records.Count == 0)
    {
        Debug.Log("[View] No records");
        return;
    }

    HideAllTextObjects();

    // 按 target 分组
    Dictionary<string, List<InteractionRecord>> grouped = new Dictionary<string, List<InteractionRecord>>();
    foreach (var r in records)
    {
        if (!grouped.ContainsKey(r.targetName))
            grouped[r.targetName] = new List<InteractionRecord>();
        grouped[r.targetName].Add(r);
    }

    Transform cam = Camera.main.transform;
    Vector3 forwardPos = cam.position + cam.forward * spawnDistance;

    int total = grouped.Count;
    int rows = Mathf.CeilToInt((float)total / maxPerRow);

    // ✅ 计算整体宽高
    float totalWidth = (maxPerRow - 1) * rootPanelSpacingX;
    float totalHeight = (rows - 1) * rootPanelSpacingY;

    // ✅ 关键：中心偏移（让整个 grid 居中）
    Vector3 centerOffset = new Vector3(
        -totalWidth / 2f,
         totalHeight / 2f,
         0f
    );

    int index = 0;

    foreach (var pair in grouped)
    {
        string targetName = pair.Key;
        List<InteractionRecord> list = pair.Value;

        int row = index / maxPerRow;
        int col = index % maxPerRow;

        Vector3 offset = new Vector3(
            col * rootPanelSpacingX,
            -row * rootPanelSpacingY,
            0f
        );

        // ✅ 应用中心偏移
        Vector3 targetCenterPos = forwardPos + centerOffset + offset;

        GameObject centerPanel = CreateCenterPanel(targetName, targetCenterPos);
        spawnedPanels.Add(centerPanel);

        int count = list.Count;

        for (int i = 0; i < count; i++)
        {
            float panelAngle = i * Mathf.PI * 2f / count;
            float r = Random.Range(radiusMin, radiusMax);

            Vector3 pos = targetCenterPos + new Vector3(
                Mathf.Cos(panelAngle) * r,
                Mathf.Sin(panelAngle) * r,
                0f
            );

            GameObject panel = CreatePromptPanel(list[i], pos);
            spawnedPanels.Add(panel);

            CreateLine(panel.transform.position, centerPanel.transform.position);
        }

        index++;
    }

    Debug.Log("[View] Semantic Wall View Spawned (Centered)");
}

    // =========================
    // 创建中心 Panel
    // =========================
    GameObject CreateCenterPanel(string targetName, Vector3 pos)
    {
        GameObject panel = Instantiate(panelPrefab, pos, Quaternion.identity);
        panel.transform.forward = Camera.main.transform.forward;

        TMP_Text text = panel.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = targetName;

        return panel;
    }

    // =========================
    // 创建 Prompt Panel
    // =========================
    GameObject CreatePromptPanel(InteractionRecord record, Vector3 pos)
    {
        GameObject panel = Instantiate(panelPrefab, pos, Quaternion.identity);
        panel.transform.forward = Camera.main.transform.forward;

        TMP_Text text = panel.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = record.prompt;

        return panel;
    }

    // =========================
    // 创建线条
    // =========================
    void CreateLine(Vector3 a, Vector3 b)
    {
        GameObject lineObj = new GameObject("Line");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        lr.material = lineMaterial;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);

        spawnedLines.Add(lineObj);
    }

    // =========================
    // 隐藏原 Text Objects
    // =========================
    void HideAllTextObjects()
    {
        foreach (var t in FindObjectsOfType<CurrentTextStore>())
            t.gameObject.SetActive(false);
    }

    // =========================
    // 显示原 Text Objects
    // =========================
    void ShowAllTextObjects()
    {
        foreach (var t in FindObjectsOfType<CurrentTextStore>())
            t.gameObject.SetActive(true);
    }

    // =========================
    // 清理
    // =========================
    void ClearView()
    {
        foreach (var p in spawnedPanels) Destroy(p);
        foreach (var l in spawnedLines) Destroy(l);
        spawnedPanels.Clear();
        spawnedLines.Clear();
        ShowAllTextObjects();
        Debug.Log("[View] Cleared");
    }

    // =========================
    // Mock 数据
    // =========================
    List<InteractionRecord> GetMockRecords()
    {
        List<InteractionRecord> mock = new List<InteractionRecord>();

        // TV
        mock.Add(new InteractionRecord("turn on", "TV"));
        mock.Add(new InteractionRecord("turn off", "TV"));
        mock.Add(new InteractionRecord("switch channel", "TV"));
        mock.Add(new InteractionRecord("turn off after 2 hours", "TV"));
        mock.Add(new InteractionRecord("increase volume", "TV"));
        mock.Add(new InteractionRecord("decrease volume", "TV"));
        mock.Add(new InteractionRecord("mute", "TV"));

        // Air Conditioner
        mock.Add(new InteractionRecord("turn on", "Air Conditioner"));
        mock.Add(new InteractionRecord("turn off", "Air Conditioner"));
        mock.Add(new InteractionRecord("increase temperature", "Air Conditioner"));
        mock.Add(new InteractionRecord("decrease temperature", "Air Conditioner"));
        mock.Add(new InteractionRecord("turn off at 2 AM", "Air Conditioner"));
        mock.Add(new InteractionRecord("set to 24 degrees", "Air Conditioner"));
        mock.Add(new InteractionRecord("enable eco mode", "Air Conditioner"));

        // Computer
        mock.Add(new InteractionRecord("send today's schedule to Alex", "Computer"));
        mock.Add(new InteractionRecord("open email", "Computer"));
        mock.Add(new InteractionRecord("start meeting", "Computer"));
        mock.Add(new InteractionRecord("shutdown", "Computer"));
        mock.Add(new InteractionRecord("restart", "Computer"));
        mock.Add(new InteractionRecord("open browser", "Computer"));

        // Lamp
        mock.Add(new InteractionRecord("turn on", "lamp"));
        mock.Add(new InteractionRecord("turn off", "lamp"));
        mock.Add(new InteractionRecord("more energetic", "lamp"));

        return mock;
    }
}