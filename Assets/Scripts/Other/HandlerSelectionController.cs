using UnityEngine;

public class HandlerSelectionController : MonoBehaviour
{
    [Header("当前生成的 TextObject")]
    public GameObject currentTextObject;

    private Collider[] currentColliders;
    private Behaviour[] currentRaycasters;
    private Behaviour gazePinchApplier;


    public void OnSelected()
    {
        if (currentTextObject == null) return;

        // gazePinchApplier = currentTextObject.GetComponent<GazePinchPromptApplierOnDevice_Case2>();
        // if (gazePinchApplier != null)
        //     gazePinchApplier.enabled = false;

  
        currentColliders = currentTextObject.GetComponentsInChildren<Collider>();
        foreach (var col in currentColliders)
        {
            if (col != null)
                col.enabled = false;
        }

        currentRaycasters = currentTextObject.GetComponentsInChildren<Behaviour>();
        foreach (var b in currentRaycasters)
        {
            if (b != null && b.GetType().Name == "TrackedDeviceGraphicRaycaster")
                b.enabled = false;
        }

        Debug.Log("[Handler] Selected -> 已禁用 TextObject 交互组件");
    }

    public void OnDeselected()
    {
        if (currentTextObject == null) return;

        // if (gazePinchApplier != null)
        //     gazePinchApplier.enabled = true;


        if (currentColliders != null)
        {
            foreach (var col in currentColliders)
            {
                if (col != null)
                    col.enabled = true;
            }
        }

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