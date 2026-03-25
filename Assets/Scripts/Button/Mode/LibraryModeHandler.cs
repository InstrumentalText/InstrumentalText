using UnityEngine;
using UnityEngine.InputSystem;


public class LibraryModeHandler : MonoBehaviour
{
    [Header("Mode Reference")]
    public ModeButton targetButton;

    [Header("Library UI")]
    public GameObject libraryUI;
    public LibraryUIController libraryUIController;

    [Header("Pinch Settings")]
    public InputActionProperty pinchAction;
    public float pinchDownThreshold = 0.8f;

    [Header("Debug")]
    public bool debug = true;

    private bool isModeActive = false;
    private bool firstPinchTriggered = false;

    private void Awake()
    {
        if (targetButton == null)
        {
            Debug.LogError("[LibraryModeHandler] 请拖入 ModeButton！");
            return;
        }

        if (libraryUI == null)
        {
            Debug.LogError("[LibraryModeHandler] 请拖入 Library UI！");
        }

        if (libraryUIController == null && libraryUI != null)
        {
            libraryUIController = libraryUI.GetComponent<LibraryUIController>();
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
            Debug.Log($"[LibraryMode] 状态 → {(active ? "激活" : "关闭")}");

        if (!active)
        {
            if (libraryUI != null)
                libraryUI.SetActive(false);

            firstPinchTriggered = false;
        }
        else
        {
            firstPinchTriggered = false;
        }
    }

    private void Update()
    {
        if (!isModeActive || pinchAction == null || libraryUI == null)
            return;

        if (!firstPinchTriggered)
        {
            float pinchValue = pinchAction.action.ReadValue<float>();

            if (pinchValue >= pinchDownThreshold)
            {
                ShowLibraryUI();
                firstPinchTriggered = true;

                if (debug)
                    Debug.Log("[LibraryMode] Pinch → 刷新并显示 Library UI");
            }
        }
    }

    void ShowLibraryUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        libraryUI.transform.position =
            cam.transform.position + cam.transform.forward * 0.5f;

        libraryUI.transform.rotation =
            Quaternion.LookRotation(cam.transform.forward);

        if (libraryUIController != null)
            libraryUIController.RefreshLibrary();

        libraryUI.SetActive(true);
    }
}