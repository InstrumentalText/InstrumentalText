using System.Collections.Generic;
using UnityEngine;

public class TextObjectManager : MonoBehaviour
{
    public static TextObjectManager Instance;


    [System.Serializable]
    public class TextObjectEntry
    {
        public GameObject textObject;
        public List<GameObject> applyTargets = new List<GameObject>();
    }

    public List<TextObjectEntry> staticTextObjects = new List<TextObjectEntry>();


    private List<GameObject> allTextObjects = new List<GameObject>();

    private Dictionary<GameObject, List<GameObject>> applyMap =
        new Dictionary<GameObject, List<GameObject>>();

    private List<GameObject> spawnedLines = new List<GameObject>();

    [Header("Dot Connection")]
    public string dotName = "Dot";

    public Material lineMaterial;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        InitStaticData();
    }

    void InitStaticData()
    {
        foreach (var entry in staticTextObjects)
        {
            if (entry == null || entry.textObject == null) continue;

            RegisterTextObject(entry.textObject);

            if (!applyMap.ContainsKey(entry.textObject))
                applyMap[entry.textObject] = new List<GameObject>();

            foreach (var target in entry.applyTargets)
            {
                if (target == null) continue;

                if (!applyMap[entry.textObject].Contains(target))
                    applyMap[entry.textObject].Add(target);
            }
        }
    }

    public void RegisterTextObject(GameObject obj)
    {
        if (obj == null) return;

        if (!allTextObjects.Contains(obj))
        {
            allTextObjects.Add(obj);

            if (!applyMap.ContainsKey(obj))
                applyMap[obj] = new List<GameObject>();
        }
    }

    public void UnregisterTextObject(GameObject obj)
    {
        if (obj == null) return;

        allTextObjects.Remove(obj);

        if (applyMap.ContainsKey(obj))
            applyMap.Remove(obj);
    }

    public void AddApplyTarget(GameObject textObj, GameObject target)
    {
        if (textObj == null || target == null) return;

        if (!applyMap.ContainsKey(textObj))
            applyMap[textObj] = new List<GameObject>();

        if (!applyMap[textObj].Contains(target))
            applyMap[textObj].Add(target);
    }

    public List<GameObject> GetApplyTargets(GameObject textObj)
    {
        if (textObj == null) return new List<GameObject>();

        if (applyMap.ContainsKey(textObj))
            return new List<GameObject>(applyMap[textObj]);

        return new List<GameObject>();
    }

    public List<GameObject> GetAllTextObjects()
    {
        return new List<GameObject>(allTextObjects);
    }

    public void ShowAll()
    {
        foreach (var obj in allTextObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        ClearLines();

        foreach (var pair in applyMap)
        {
            GameObject textObj = pair.Key;

            foreach (var target in pair.Value)
            {
                if (textObj != null && target != null)
                    ConnectDots(textObj, target);
            }
        }
    }

    public void HideAll()
    {
        foreach (var obj in allTextObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        ClearLines();
    }

    void ClearLines()
    {
        foreach (var l in spawnedLines)
        {
            if (l != null)
                Destroy(l);
        }

        spawnedLines.Clear();
    }

    void ConnectDots(GameObject a, GameObject b)
    {
        Transform dotA = a.transform.Find(dotName);
        Transform dotB = b.transform.Find(dotName);

        if (dotA == null || dotB == null)
        {
            Debug.LogWarning("[Dot] Dot not found");
            return;
        }

        dotA.gameObject.SetActive(true);
        dotB.gameObject.SetActive(true);

        GameObject lineObj = new GameObject("DotConnection");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        if (lineMaterial != null)
            lr.material = lineMaterial;
        else
            Debug.LogWarning("[Dot] LineMaterial 未设置！");

        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.textureMode = LineTextureMode.Tile;

        var updater = lineObj.AddComponent<LineUpdater>();
        updater.Init(dotA, dotB, lr);

        spawnedLines.Add(lineObj);
    }

    //查询object的所有应用过的text
    public List<GameObject> GetTextObjectsByTarget(GameObject target)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (var pair in applyMap)
        {
            if (pair.Value.Contains(target))
            {
                result.Add(pair.Key);
            }
        }

        return result;
    }
    public void ShowLineBetween(GameObject textObj, GameObject target)
    {
        if (textObj == null || target == null) return;

        ConnectDots(textObj, target);
    }
}