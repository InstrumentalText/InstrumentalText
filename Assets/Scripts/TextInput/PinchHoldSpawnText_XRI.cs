using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

public class PinchHoldSpawnText_XRI : MonoBehaviour
{
    [Header("Pinch Input")]
    public InputActionProperty pinchAction;

    [Header("Pinch Threshold")]
    public float pinchDownThreshold = 0.8f;

    [Header("Spawn Settings")]
    public GameObject textPrefab;
    public float spawnDistance = 0.5f;

    [Header("Gaze Reference (optional)")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeInteractor;

    [Header("Mode Control")]
    public BuilderModeHandler builderModeHandler;

    [Header("Debug")]
    public bool debug = true;

    private bool keyboardOpen = false;
    private int textCounter = 0;

    void Update()
    {
        if (builderModeHandler == null || !builderModeHandler.IsBuilderActive())
        {
            if (keyboardOpen)
                CloseKeyboard();
            return;
        }

        if (!keyboardOpen)
        {
            float pinchValue = pinchAction.action.ReadValue<float>();

            if (pinchValue >= pinchDownThreshold)
            {
                OpenKeyboard();

                if (debug)
                    Debug.Log("[Spawn] Pinch → 打开键盘");
            }
        }
    }

    void OpenKeyboard()
    {
        var globalKeyboard = GlobalNonNativeKeyboard.instance;
        if (globalKeyboard == null || globalKeyboard.keyboard == null)
        {
            Debug.LogWarning("[Spawn] GlobalNonNativeKeyboard 未找到");
            return;
        }

        var keyboard = globalKeyboard.keyboard;

        keyboard.onTextSubmitted.AddListener(OnKeyboardSubmitted);
        globalKeyboard.ShowKeyboard(true);

        keyboardOpen = true;
    }

    void CloseKeyboard()
    {
        var globalKeyboard = GlobalNonNativeKeyboard.instance;
        if (globalKeyboard != null && globalKeyboard.keyboard != null)
        {
            globalKeyboard.keyboard.onTextSubmitted.RemoveListener(OnKeyboardSubmitted);
            globalKeyboard.HideKeyboard();
        }

        keyboardOpen = false;
    }

    void OnKeyboardSubmitted(KeyboardTextEventArgs args)
    {
        string text = args.keyboardText?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            if (debug) Debug.Log("[Spawn] 输入为空，忽略");
            return;
        }

        GameObject newTextObject = SpawnText();
        if (newTextObject == null) return;

        var store = newTextObject.GetComponent<CurrentTextStore>();
        if (store != null)
            store.CurrentText = text;

        var inputField = newTextObject.GetComponentInChildren<TMPro.TMP_InputField>(true);
        if (inputField != null)
            inputField.text = text;

        if (TextObjectManager.Instance != null)
            TextObjectManager.Instance.RegisterTextObject(newTextObject);

        if (debug)
            Debug.Log($"[Spawn] Enter → 创建 TextObject '{newTextObject.name}'，内容='{text}'");

        CloseKeyboard();
    }

    GameObject SpawnText()
    {
        if (textPrefab == null)
        {
            if (debug) Debug.LogWarning("[Spawn] textPrefab 未设置");
            return null;
        }

        Camera cam = Camera.main;
        if (cam == null) return null;

        float gazeY = cam.transform.position.y;
        if (gazeInteractor != null &&
            gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            gazeY = hit.point.y;
        }

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
        spawnPos.y = gazeY;

        Quaternion rot = Quaternion.LookRotation(cam.transform.forward);

        GameObject newText = Instantiate(textPrefab, spawnPos, rot);

        textCounter++;
        newText.name = $"Text object {textCounter}";

        if (debug) Debug.Log($"[Spawn] 创建 {newText.name}，位置 {spawnPos}");

        return newText;
    }
}
