// using UnityEngine;

// public class SphereHandleProxy : MonoBehaviour
// {
//     public Transform targetInputField;

//     Vector3 initialHandlePos;
//     Vector3 initialInputPos;

//     float initialDistance;

//     Vector3 initialInputScale;

//     void Start()
//     {
//         initialHandlePos = transform.position;
//         initialInputPos = targetInputField.position;

//         initialInputScale = targetInputField.localScale;

//         initialDistance = Vector3.Distance(Camera.main.transform.position, transform.position);
//     }

//     void Update()
//     {
//         UpdatePosition();
//         UpdateScale();
//     }

//     void UpdatePosition()
//     {
//         Vector3 delta = transform.position - initialHandlePos;
//         targetInputField.position = initialInputPos + delta;
//     }

//     void UpdateScale()
//     {
//         float currentDistance = Vector3.Distance(Camera.main.transform.position, transform.position);

//         float scaleFactor = currentDistance / initialDistance;

//         targetInputField.localScale = initialInputScale * scaleFactor;
//     }
// }


using UnityEngine;

public class SphereHandleProxy : MonoBehaviour
{
    public Transform targetInputField;

    Vector3 initialHandlePos;
    Vector3 initialInputPos;

    float initialDistance;

    Vector3 initialInputScale;

    Collider inputCollider;
    Collider sphereCollider;

    float initialGap;

    void Start()
    {
        initialHandlePos = transform.position;
        initialInputPos = targetInputField.position;

        initialInputScale = targetInputField.localScale;

        initialDistance = Vector3.Distance(Camera.main.transform.position, transform.position);

        inputCollider = targetInputField.GetComponent<Collider>();
        sphereCollider = GetComponent<Collider>();

        if (inputCollider != null && sphereCollider != null)
        {
            float inputBottom = inputCollider.bounds.min.y;
            float sphereTop = sphereCollider.bounds.max.y;

            initialGap = inputBottom - sphereTop;
        }
    }

    void Update()
    {
        UpdatePosition();
        UpdateScale();
        UpdateSphereOffset();
    }

    void UpdatePosition()
    {
        Vector3 delta = transform.position - initialHandlePos;
        targetInputField.position = initialInputPos + delta;
    }

    void UpdateScale()
    {
        float currentDistance = Vector3.Distance(Camera.main.transform.position, transform.position);

        float scaleFactor = currentDistance / initialDistance;

        targetInputField.localScale = initialInputScale * scaleFactor;
    }

    void UpdateSphereOffset()
    {
        if (inputCollider == null || sphereCollider == null)
            return;

        float scaleFactor = targetInputField.localScale.x / initialInputScale.x;

        float scaledGap = initialGap * scaleFactor;

        float inputBottom = inputCollider.bounds.min.y;
        float sphereHalfHeight = sphereCollider.bounds.extents.y;

        Vector3 spherePos = transform.position;

        spherePos.y = inputBottom - sphereHalfHeight - scaledGap;

        transform.position = spherePos;
    }
}