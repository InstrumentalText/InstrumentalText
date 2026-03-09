using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(RectTransform))]
public class GazeCursorFollowUI : MonoBehaviour
{
    [Header("Gaze Interactor")]
    public XRGazeInteractor gazeInteractor;

    [Header("Placement")]
    public float defaultDistance = 0.5f;       
    public float surfaceOffset = 0.01f;         
    [Header("Visual Size")]
    public float referenceDistance = 0.5f;
    public float referenceScale = 0.01f;

    [Header("Gaze Highlight")]
    public GazeHighlighter gazeHighlighter;

    RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(32f, 32f);
    }

    void Update()
    {
        if (gazeInteractor == null)
            return;

        Transform cam = gazeInteractor.transform;
        Vector3 targetPosition;
        Transform gazedObject = null;

        if (gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            targetPosition = hit.point + hit.normal * surfaceOffset;
            gazedObject = hit.transform;
        }
        else
        {
            targetPosition = cam.position + cam.forward * defaultDistance;
        }

        transform.position = targetPosition;

        // 始终面向摄像机
        transform.forward = cam.forward;

        float distance = Vector3.Distance(cam.position, targetPosition);
        float scale = (distance / referenceDistance) * referenceScale;
        transform.localScale = Vector3.one * scale;

        if (gazeHighlighter != null)
        {
            gazeHighlighter.UpdateGaze(gazedObject);
        }
    }
}