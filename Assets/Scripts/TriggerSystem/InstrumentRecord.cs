using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a single instrument: a group of related triggers
/// generated from one LLM response for one user intent.
/// </summary>
[System.Serializable]
public class InstrumentRecord
{
    public int id;
    public string userIntent;
    public GameObject target;
    public List<int> triggerIds = new();
}
