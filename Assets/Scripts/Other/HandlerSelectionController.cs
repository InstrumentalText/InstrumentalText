using UnityEngine;

public class HandlerSelectionController : MonoBehaviour
{
    [Header("当前生成的 TextObject")]
    public GameObject currentTextObject;

    private Collider[] currentColliders;
    private Behaviour[] currentRaycasters;
    private Behaviour gazePinchApplier;

    /// <summary>
    /// Handler 被选中时调用（XR Grab Interactable Select Enter）
    /// </summary>
    public void OnSelected()
    {
        if (currentTextObject == null) return;

        // 1. 禁用 GazePinchPromptApplier
        // gazePinchApplier = currentTextObject.GetComponent<GazePinchPromptApplierOnDevice_Case2>();
        // if (gazePinchApplier != null)
        //     gazePinchApplier.enabled = false;

        // 2. 禁用所有 Collider
        currentColliders = currentTextObject.GetComponentsInChildren<Collider>();
        foreach (var col in currentColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // 3. 禁用 TrackedDeviceGraphicRaycaster
        currentRaycasters = currentTextObject.GetComponentsInChildren<Behaviour>();
        foreach (var b in currentRaycasters)
        {
            if (b != null && b.GetType().Name == "TrackedDeviceGraphicRaycaster")
                b.enabled = false;
        }

        Debug.Log("[Handler] Selected -> 已禁用 TextObject 交互组件");
    }

    /// <summary>
    /// Handler 取消选中时调用（XR Grab Interactable Select Exit）
    /// </summary>
    public void OnDeselected()
    {
        if (currentTextObject == null) return;

        // // 1. 恢复 GazePinchPromptApplier
        // if (gazePinchApplier != null)
        //     gazePinchApplier.enabled = true;

        // 2. 恢复 Collider
        if (currentColliders != null)
        {
            foreach (var col in currentColliders)
            {
                if (col != null)
                    col.enabled = true;
            }
        }

        // 3. 恢复 Raycaster
        if (currentRaycasters != null)
        {
            foreach (var b in currentRaycasters)
            {
                if (b != null && b.GetType().Name == "TrackedDeviceGraphicRaycaster")
                    b.enabled = true;
            }
        }

        Debug.Log("[Handler] Deselected -> 已恢复 TextObject 交互组件");
    }
}