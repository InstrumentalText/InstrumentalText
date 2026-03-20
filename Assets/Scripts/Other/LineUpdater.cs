using UnityEngine;

public class LineUpdater : MonoBehaviour
{
    private Transform a;
    private Transform b;
    private LineRenderer lr;

    public void Init(Transform a, Transform b, LineRenderer lr)
    {
        this.a = a;
        this.b = b;
        this.lr = lr;
    }

    void Update()
    {
        if (a == null || b == null)
        {
            Destroy(gameObject);
            return;
        }

        lr.SetPosition(0, a.position);
        lr.SetPosition(1, b.position);
    }
}