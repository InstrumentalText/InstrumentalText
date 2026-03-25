using UnityEngine;


[RequireComponent(typeof(ModeButton))]
public class BuilderModeHandler : MonoBehaviour
{
    [Header("Mode Reference")]
    public ModeButton targetButton; 

    [Header("Debug")]
    public bool debug = true;

    private bool isBuilderActive = false;

    private void Awake()
    {
        if (targetButton == null)
        {
            Debug.LogError("[BuilderModeHandler] 请拖入 Builder 的 ModeButton！");
            return;
        }

        targetButton.OnModeStateChanged += HandleModeChanged;
    }

    private void OnDestroy()
    {
        if (targetButton != null)
            targetButton.OnModeStateChanged -= HandleModeChanged;
    }


    private void HandleModeChanged(bool active)
    {
        isBuilderActive = active;

        if (debug)
            Debug.Log($"[BuilderModeHandler] Builder 状态 → {(active ? "激活" : "关闭")}");
    }


    public bool IsBuilderActive()
    {
        return isBuilderActive;
    }


    private void Update()
    {
        if (targetButton == null)
            return;

        if (isBuilderActive)
        {
            if (debug)
                Debug.Log("[BuilderModeHandler] Builder 模式激活 → 可以生成 TextObject");
        }
    }
}