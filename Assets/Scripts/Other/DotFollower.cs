using UnityEngine;


public class DotFollower : MonoBehaviour
{
    [Header("Target Dot")]
    public Transform dot;

    [Header("Offset")]
    public Vector3 localOffset;

    private void LateUpdate()
    {
        if (dot == null) return;

        dot.position = transform.TransformPoint(localOffset);
        dot.rotation = transform.rotation;
    }
}