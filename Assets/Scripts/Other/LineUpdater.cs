using UnityEngine;

public class LineUpdater : MonoBehaviour
{
    private Transform a;
    private Transform b;
    private LineRenderer lr;
    private float tileLength = 1f; 

    public void Init(Transform a, Transform b, LineRenderer lr, float tileLength = 1f)
    {
        this.a = a;
        this.b = b;
        this.lr = lr;
        this.tileLength = tileLength;
        UpdateLine();
    }

    void Update()
    {
        if (a != null && b != null && lr != null)
        {
            UpdateLine();
        }
    }

    void UpdateLine()
    {
        lr.SetPosition(0, a.position);
        lr.SetPosition(1, b.position);

        if (lr.material != null)
        {
            float distance = Vector3.Distance(a.position, b.position);
            Vector2 scale = lr.material.mainTextureScale;
            scale.x = distance / tileLength;
            lr.material.mainTextureScale = scale;
        }
    }
}