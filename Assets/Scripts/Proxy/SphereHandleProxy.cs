using UnityEngine;

public class SphereHandleProxy : MonoBehaviour
{
    public Transform[] targetObjects; 

    Vector3 initialHandlePos;
    Vector3[] initialTargetPos;

    void Start()
    {
        initialHandlePos = transform.position;

        initialTargetPos = new Vector3[targetObjects.Length];
        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (targetObjects[i] != null)
                initialTargetPos[i] = targetObjects[i].position;
        }
    }

    void Update()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        Vector3 delta = transform.position - initialHandlePos;

        for (int i = 0; i < targetObjects.Length; i++)
        {
            if (targetObjects[i] != null)
                targetObjects[i].position = initialTargetPos[i] + delta;
        }
    }
}