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

    [Header("Spawn Settings")]
    public float spawnDistance = 0.5f;  
    public float heightOffset = 0.0f;   

    [Header("Gaze Reference (optional)")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeInteractor;

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

        float gazeY = cam.transform.position.y;
        if (gazeInteractor != null &&
            gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            gazeY = hit.point.y;
        }

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
        spawnPos.y = gazeY + heightOffset;  // 高度 = gaze 高度 + 手动偏移

        libraryUI.transform.position = spawnPos;
        libraryUI.transform.rotation = Quaternion.LookRotation(cam.transform.forward);

        if (libraryUIController != null)
            libraryUIController.RefreshLibrary();

        libraryUI.SetActive(true);

        if (debug)
            Debug.Log($"[LibraryMode] Library UI 位置: {spawnPos}, 高度偏移: {heightOffset}");
    }
}