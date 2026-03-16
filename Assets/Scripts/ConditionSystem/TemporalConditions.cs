using UnityEngine;

/// <summary>
/// Pure evaluation functions for temporal conditions.
/// No state — the caller (TriggerSystem) tracks startTime, lastFireTime, fireCount, etc.
/// </summary>
public static class TemporalConditions
{
    /// <summary>
    /// temporal.once: Has the delay elapsed since startTime?
    /// </summary>
    public static bool EvaluateOnce(float startTime, float delay)
    {
        return Time.time >= startTime + delay;
    }

    /// <summary>
    /// temporal.loop: Has the interval elapsed since lastFireTime?
    /// Caller should also check fireCount against maxCount if needed.
    /// </summary>
    public static bool EvaluateLoop(float lastFireTime, float interval)
    {
        return Time.time >= lastFireTime + interval;
    }
}
