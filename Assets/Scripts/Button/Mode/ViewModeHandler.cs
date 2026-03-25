using UnityEngine;
using UnityEngine.InputSystem;


public class ViewModeHandler : MonoBehaviour
{
    [Header("Mode Reference")]
    public ModeButton targetButton;   

    [Header("Reference to Interaction Visualizer")]
    public InteractionViewVisualizer viewVisualizer;

    [Header("Pinch Settings")]
    public InputActionProperty pinchAction;
    public float pinchDownThreshold = 0.8f;

    [Header("Debug")]
    public bool debug = true;

    private bool isModeActive = false;     
    private bool viewActivated = false;   
    private bool firstPinchTriggered = false;

    private void Awake()
    {
        if (targetButton == null)
        {
            Debug.LogError("[ViewModeHandler] 请拖入 ModeButton！");
            return;
        }

        if (viewVisualizer == null)
        {
            Debug.LogError("[ViewModeHandler] 请指定 InteractionViewVisualizer！");
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
        isModeActive = active;

        if (debug)
            Debug.Log($"[ViewModeHandler] Mode 状态 → {(active ? "激活" : "关闭")}");

        if (!active)
        {

            if (viewActivated || viewVisualizer.openView)
            {
                viewVisualizer.openView = false;

                if (debug)
                    Debug.Log("[ViewModeHandler] 退出模式 → 关闭 View");
            }

            viewActivated = false;
            firstPinchTriggered = false;
        }
    }

    private void Update()
    {
        if (!isModeActive || viewVisualizer == null || pinchAction == null)
            return;

        if (!firstPinchTriggered)
        {
            float pinchValue = pinchAction.action.ReadValue<float>();

            if (pinchValue >= pinchDownThreshold)
            {
                viewVisualizer.openView = true;
                viewActivated = true;
                firstPinchTriggered = true;

                if (debug)
                    Debug.Log("[ViewModeHandler] 第一次 Pinch → 开启 View（保持开启）");
            }
        }

    }
}