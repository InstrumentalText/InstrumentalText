using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reads hand tracking position/rotation from Input Actions (tracking space)
/// and converts them to world space, bypassing TrackedPoseDriver's local-space approach.
/// Use this as the palm anchor for HandMenu when XR Origin can move/rotate.
/// </summary>
public class WorldSpaceHandAnchor : MonoBehaviour
{
    [Header("XR Origin")]
    public Transform xrOriginTransform;

    [Header("Input Actions")]
    public InputActionProperty positionInput;
    public InputActionProperty rotationInput;

    [Header("World Space Offset")]
    public Vector3 worldOffset = new Vector3(0f, -2f, 0f);

    void OnEnable()
    {
        positionInput.action?.Enable();
        rotationInput.action?.Enable();
    }

    void OnDisable()
    {
        positionInput.action?.Disable();
        rotationInput.action?.Disable();
    }

    void LateUpdate()
    {
        if (xrOriginTransform == null) return;

        var trackingPos = positionInput.action?.ReadValue<Vector3>() ?? Vector3.zero;
        var trackingRot = rotationInput.action?.ReadValue<Quaternion>() ?? Quaternion.identity;

        transform.position = xrOriginTransform.TransformPoint(trackingPos) + worldOffset;
        transform.rotation = xrOriginTransform.rotation * trackingRot;
    }
}
