using UnityEngine;

public class BallLineUpdater : MonoBehaviour
{
    private LineRenderer lr;
    private Transform target;

    public void Init(LineRenderer lineRenderer, Transform targetTransform)
    {
        lr = lineRenderer;
        target = targetTransform;
    }

    void Update()
    {
        if (lr == null || target == null) return;

        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, target.position);
    }
}
