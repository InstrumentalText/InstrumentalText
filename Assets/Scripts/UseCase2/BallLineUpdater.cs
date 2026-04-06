using UnityEngine;

public class BallLineUpdater : MonoBehaviour
{
    public float tilingPerUnit = 50f;

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

        float length = Vector3.Distance(transform.position, target.position);
        lr.material.mainTextureScale = new Vector2(length * tilingPerUnit, 1f);
    }
}
