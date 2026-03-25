using System.Collections.Generic;
using UnityEngine;

public class InteractionLibraryManager : MonoBehaviour
{
    public static InteractionLibraryManager Instance;

    [Header("Runtime Records")]
    [SerializeField]
    private List<InteractionRecord> records = new List<InteractionRecord>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[Library] Manager Initialized");
    }


    public void AddRecord(string prompt, string targetName, GameObject target = null)
    {
        if (string.IsNullOrEmpty(prompt) || string.IsNullOrEmpty(targetName))
        {
            Debug.LogWarning("[Library] Invalid record");
            return;
        }

        InteractionRecord record = new InteractionRecord(prompt, targetName, target);
        records.Add(record);

        Debug.Log($"[Library] Added: {prompt} → {targetName}");
    }


    public List<InteractionRecord> GetAllRecords()
    {
        return records;
    }

    public List<InteractionRecord> GetRecordsByTarget(string targetName)
    {
        return records.FindAll(r => r.targetName == targetName);
    }

    public List<InteractionRecord> SearchByPrompt(string keyword)
    {
        return records.FindAll(r => r.prompt.ToLower().Contains(keyword.ToLower()));
    }



    [ContextMenu("Print All Records")]
    public void PrintAllRecords()
    {
        Debug.Log($"[Library] Total Records: {records.Count}");

        foreach (var r in records)
        {
            Debug.Log($"{r.time:F2} | {r.targetName} ← {r.prompt}");
        }
    }



    public void Clear()
    {
        records.Clear();
        Debug.Log("[Library] Cleared");
    }
}