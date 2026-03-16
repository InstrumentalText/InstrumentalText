using UnityEngine;
using System;

[Serializable]
public class TriggerEntry
{
    public int id;

    // ── Condition side ──
    public string conditionType;
    public GameObject subjectA;
    public GameObject subjectB;
    public string conditionArgsJson;

    // Runtime state (managed by TriggerSystem)
    public float startTime;
    public float lastFireTime;
    public int fireCount;
    public int maxCount;       // For temporal.loop, 0 = infinite
    public bool wasInside;     // For spatial.onEnter / onExit transition detection
    public bool completed;

    // ── Action side ──
    public GameObject targetObject;
    public string actionType;
    public string actionArgsJson;
}
