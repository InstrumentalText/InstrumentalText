using UnityEngine;

/// <summary>
/// Pure evaluation functions for spatial conditions.
/// For onEnter/onExit, the caller (TriggerSystem) is responsible for tracking
/// the previous state and detecting transitions.
/// </summary>
public static class SpatialConditions
{
    /// <summary>
    /// spatial.onEnter / spatial.onExit shared check:
    /// Is subjectA currently inside subjectB's trigger collider?
    /// TriggerSystem detects false→true for onEnter, true→false for onExit.
    /// </summary>
    public static bool IsInsideTrigger(GameObject subjectA, GameObject subjectB)
    {
        if (subjectA == null || subjectB == null) return false;

        var colliderB = subjectB.GetComponent<Collider>();
        if (colliderB == null) return false;

        return colliderB.bounds.Contains(subjectA.transform.position);
    }

    /// <summary>
    /// spatial.onCollision: Are the two objects currently in contact?
    /// Uses Physics.ComputePenetration or a simple bounds overlap check.
    /// </summary>
    public static bool IsColliding(GameObject subjectA, GameObject subjectB)
    {
        if (subjectA == null || subjectB == null) return false;

        var colA = subjectA.GetComponent<Collider>();
        var colB = subjectB.GetComponent<Collider>();
        if (colA == null || colB == null) return false;

        // Quick bounds check first
        if (!colA.bounds.Intersects(colB.bounds)) return false;

        // Precise penetration check
        return Physics.ComputePenetration(
            colA, subjectA.transform.position, subjectA.transform.rotation,
            colB, subjectB.transform.position, subjectB.transform.rotation,
            out _, out _
        );
    }

    /// <summary>
    /// spatial.distance: Is the distance between A and B within the threshold?
    /// </summary>
    public static bool IsWithinDistance(GameObject subjectA, GameObject subjectB, float threshold)
    {
        if (subjectA == null || subjectB == null) return false;

        float dist = Vector3.Distance(
            subjectA.transform.position,
            subjectB.transform.position
        );
        return dist <= threshold;
    }
}
