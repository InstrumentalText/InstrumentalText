using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages instrument lifecycle: registration, unregistration, queries,
/// and automatic cleanup when all triggers in an instrument complete.
/// </summary>
public class InstrumentRegistry : MonoBehaviour
{
    private TriggerSystem triggerSystem;

    private void Awake()
    {
        triggerSystem = FindAnyObjectByType<TriggerSystem>();
    }

    private int nextInstrumentId = 1;
    private readonly List<InstrumentRecord> instruments = new();

    /// <summary>
    /// Register a new instrument. Returns the instrument ID.
    /// </summary>
    public int RegisterInstrument(GameObject target, string userIntent, List<int> triggerIds)
    {
        int id = nextInstrumentId++;
        var record = new InstrumentRecord
        {
            id = id,
            userIntent = userIntent,
            target = target,
            triggerIds = new List<int>(triggerIds)
        };
        instruments.Add(record);
        Debug.Log($"[InstrumentRegistry] Registered instrument #{id} on '{target.name}' with {triggerIds.Count} trigger(s): \"{userIntent}\"");
        return id;
    }

    /// <summary>
    /// Unregister an instrument and all its triggers.
    /// </summary>
    public void UnregisterInstrument(int instrumentId)
    {
        var record = instruments.Find(r => r.id == instrumentId);
        if (record == null) return;

        triggerSystem.UnregisterByInstrumentId(instrumentId);
        instruments.Remove(record);
        Debug.Log($"[InstrumentRegistry] Unregistered instrument #{instrumentId} from '{record.target.name}'");
    }

    /// <summary>
    /// Called by TriggerSystem when a trigger completes.
    /// If all triggers in the instrument are done, auto-remove the instrument.
    /// </summary>
    public void OnTriggerCompleted(int instrumentId, int triggerId)
    {
        var record = instruments.Find(r => r.id == instrumentId);
        if (record == null) return;

        record.triggerIds.Remove(triggerId);

        if (record.triggerIds.Count == 0)
        {
            instruments.Remove(record);
            Debug.Log($"[InstrumentRegistry] Instrument #{instrumentId} on '{record.target.name}' auto-removed (all triggers completed)");
        }
    }

    /// <summary>
    /// Get all instruments on a specific target.
    /// </summary>
    public List<InstrumentRecord> GetInstrumentsByTarget(GameObject target)
    {
        return instruments.Where(r => r.target == target).ToList();
    }

    /// <summary>
    /// Get all active instruments.
    /// </summary>
    public IReadOnlyList<InstrumentRecord> GetAllInstruments()
    {
        return instruments;
    }

    /// <summary>
    /// Get a specific instrument by ID. Returns null if not found.
    /// </summary>
    public InstrumentRecord GetInstrument(int instrumentId)
    {
        return instruments.Find(r => r.id == instrumentId);
    }
}
