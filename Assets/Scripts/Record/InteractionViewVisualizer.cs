using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractionViewVisualizer : MonoBehaviour
{
    [Header("View Toggle")]
    public bool openView = false;

    [Header("XR Gaze")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeInteractor;

    [Header("Prefabs")]
    public GameObject panelPrefab;

    [Header("Dot Connection")]
    public string dotName = "Dot";
    public Color lineColor = Color.green;
    public float lineWidth = 0.02f;

    [Header("Layout")]
    public float spawnDistance = 1.2f;
    public float rootPanelSpacingX = 0.5f;
    public float rootPanelSpacingY = 0.4f;
    public int maxPerRow = 4;

    [Header("Radius Random")]
    public float radiusMin = 0.15f;
    public float radiusMax = 0.3f;

    [Header("Diagonal Layout")]
    [Tooltip("Leaf nodes snap to 4 diagonal directions (45/135/225/315°). This controls how far each node can deviate from its sector center.")]
    public float diagonalSectorHalfAngle = 25f;

    [Header("Colors")]
    public Color centerColor = Color.yellow;
    public Color leafColor = Color.green;

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

    void ShowView()
    {
        ClearView();


        Dictionary<string, List<string>> grouped = new Dictionary<string, List<string>>();

        if (TextObjectManager.Instance == null)
        {
            Debug.LogWarning("[View] TextObjectManager 未找到");
            return;
        }

        var allTextObjects = TextObjectManager.Instance.GetAllTextObjects();

        foreach (var textObj in allTextObjects)
        {
            if (textObj == null) continue;

            var store = textObj.GetComponent<CurrentTextStore>();
            if (store == null || string.IsNullOrEmpty(store.CurrentText)) continue;

            string prompt = store.CurrentText;

            var targets = TextObjectManager.Instance.GetApplyTargets(textObj);

            foreach (var target in targets)
            {
                if (target == null) continue;

                string targetName = target.name;

                if (!grouped.ContainsKey(targetName))
                    grouped[targetName] = new List<string>();

                grouped[targetName].Add(prompt);
            }
        }

        if (grouped.Count == 0)
        {
            Debug.Log("[View] 没有任何 Apply 数据");
            return;
        }

        HideAllTextObjects();

        Transform cam = Camera.main.transform;

        Vector3 basePos = GetGazePosition(cam);
        Vector3 right = cam.right;
        Vector3 up = cam.up;

        int total = grouped.Count;
        int rows = Mathf.CeilToInt((float)total / maxPerRow);

        int index = 0;

        foreach (var pair in grouped)
        {
            int row = index / maxPerRow;
            int col = index % maxPerRow;

            int currentRowCount = Mathf.Min(maxPerRow, total - row * maxPerRow);

            float totalWidth = (currentRowCount - 1) * rootPanelSpacingX;
            float totalHeight = (rows - 1) * rootPanelSpacingY;

            float xOffset = col * rootPanelSpacingX - totalWidth / 2f;
            float yOffset = -(row * rootPanelSpacingY - totalHeight / 2f);

            Vector3 centerPos = basePos + right * xOffset + up * yOffset;

            GameObject centerPanel = CreateCenterPanel(pair.Key, centerPos, centerColor);
            spawnedPanels.Add(centerPanel);

            int count = pair.Value.Count;

            for (int i = 0; i < count; i++)
            {
                float[] sectorCenters = { 45f, 135f, 225f, 315f };
                float sectorCenter = sectorCenters[i % sectorCenters.Length] * Mathf.Deg2Rad;
                float deviation = Random.Range(-diagonalSectorHalfAngle, diagonalSectorHalfAngle) * Mathf.Deg2Rad;
                float angle = sectorCenter + deviation;
                float r = Random.Range(radiusMin, radiusMax);

                Vector3 offset =
                    right * (Mathf.Cos(angle) * r) +
                    up * (Mathf.Sin(angle) * r);

                Vector3 pos = centerPos + offset;

                GameObject panel = CreatePromptPanel(pair.Value[i], pos, leafColor);
                spawnedPanels.Add(panel);

                ConnectDots(panel, centerPanel);
            }

            index++;
        }

        Debug.Log("[View] Dot-based Line System Final");
    }

    void ConnectDots(GameObject a, GameObject b)
    {
        Transform dotA = a.transform.Find(dotName);
        Transform dotB = b.transform.Find(dotName);

        if (dotA == null || dotB == null)
        {
            Debug.LogWarning("[View] Dot not found on panel");
            return;
        }

        dotA.gameObject.SetActive(true);
        dotB.gameObject.SetActive(true);

        GameObject lineObj = new GameObject("DotConnection");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = lineColor;

        lr.material = mat;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 2;
        lr.useWorldSpace = true;

        LineUpdater updater = lineObj.AddComponent<LineUpdater>();
        updater.Init(dotA, dotB, lr);

        spawnedLines.Add(lineObj);
    }

    Vector3 GetGazePosition(Transform cam)
    {
        if (gazeInteractor != null &&
            gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            return hit.point;
        }

        return cam.position + cam.forward * spawnDistance;
    }

    GameObject CreateCenterPanel(string text, Vector3 pos, Color color)
    {
        GameObject panel = Instantiate(panelPrefab, pos, Quaternion.identity);
        panel.transform.forward = Camera.main.transform.forward;
        panel.transform.localScale *= 0.6f;

        TMP_Text t = panel.GetComponentInChildren<TMP_Text>();
        if (t != null) t.text = text;

        ApplyPanelColor(panel, color);
        return panel;
    }

    GameObject CreatePromptPanel(string prompt, Vector3 pos, Color color)
    {
        GameObject panel = Instantiate(panelPrefab, pos, Quaternion.identity);
        panel.transform.forward = Camera.main.transform.forward;
        panel.transform.localScale *= 0.6f;

        TMP_Text t = panel.GetComponentInChildren<TMP_Text>();
        if (t != null) t.text = prompt;

        ApplyPanelColor(panel, color);
        return panel;
    }

    void ApplyPanelColor(GameObject panel, Color color)
    {
        var image = panel.GetComponentInChildren<UnityEngine.UI.Image>();
        if (image != null)
            image.color = color;

        var planeTransform = panel.transform.Find("Plane");
        if (planeTransform != null)
        {
            var rend = planeTransform.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = color;
        }
    }

    void HideAllTextObjects()
    {
        foreach (var applier in FindObjectsOfType<GazePinchPromptApplierOnDevice_Case2>())
            applier.SetConnectionLinesVisible(false);

        foreach (var t in FindObjectsOfType<CurrentTextStore>())
            t.gameObject.SetActive(false);
    }

    void ShowAllTextObjects()
    {
        foreach (var t in FindObjectsOfType<CurrentTextStore>())
            t.gameObject.SetActive(true);

        foreach (var applier in FindObjectsOfType<GazePinchPromptApplierOnDevice_Case2>())
            applier.SetConnectionLinesVisible(true);
    }

    void ClearView()
    {
        foreach (var p in spawnedPanels)
            if (p != null) Destroy(p);

        foreach (var l in spawnedLines)
            if (l != null) Destroy(l);

        spawnedPanels.Clear();
        spawnedLines.Clear();

        ShowAllTextObjects();
    }

    List<InteractionRecord> GetMockRecords()
    {
        return new List<InteractionRecord>
        {
            new InteractionRecord("Switch channel","TV"),
            new InteractionRecord("Turn off after 2 hours","TV"),
            new InteractionRecord("Increase volume","TV"),

            new InteractionRecord("Reverse","Air Conditioner"),
            new InteractionRecord("Decrease temperature","Air Conditioner"),
            new InteractionRecord("Enable eco mode","Air Conditioner"),

            new InteractionRecord("Turn on","Lamp"),
            new InteractionRecord("More energetic","Lamp"),
        };
    }
}