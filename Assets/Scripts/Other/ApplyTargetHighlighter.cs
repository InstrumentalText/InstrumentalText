using UnityEngine;

public class ApplyTargetHighlighter : MonoBehaviour
{
    public static ApplyTargetHighlighter Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void SetApplyMode(bool state)
    {
        Debug.Log($"[ApplyTargetHighlighter] (Deprecated) SetApplyMode = {state}");
    }

    public bool IsApplyMode()
    {
        return GazePinchPromptApplierOnDevice_Case2.ActiveInstance != null;
    }
}