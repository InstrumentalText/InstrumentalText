using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.UI;

using System.Linq;

[RequireComponent(typeof(Toggle))]
public class LibraryItemSpawnHandler : MonoBehaviour
{
    private Toggle toggle;

    [Header("Pinch Input")]
    public InputActionProperty pinchAction;
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;
    public float pinchHoldTime = 1.0f;

    [Header("Spawn Settings")]
    public GameObject textPrefab;
    public float spawnDistance = 0.6f;

    [Header("Text Offset Relative to Library UI")]
    public Vector3 localOffset = Vector3.zero;

    [Header("Tag")]
    public string textTag = "ItemText";

    [Header("Gaze Reference (optional)")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeInteractor;

    [Header("Debug")]
    public bool debug = true;

    private bool pinchPressed = false;
    private bool interactionActive = false;

    private GameObject currentTextObject = null;
    private GazePinchPromptApplierOnDevice_Case2 activeApplier = null;

    void Awake()
    {
        toggle = GetComponent<Toggle>();

        if (toggle == null)
            Debug.LogError("[LibrarySpawn] Toggle 未找到！");

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnDestroy()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        interactionActive = isOn;

        if (!isOn) CancelApplyMode();

        if (debug)
            Debug.Log($"[LibrarySpawn] Toggle {(isOn ? "ON → 开启 Pinch 检测" : "OFF → 停止检测")}");
    }

    void Update()
    {
        if (!interactionActive) return;

        float pinchValue = pinchAction.action.ReadValue<float>();

        if (!pinchPressed && pinchValue > pinchDownThreshold)
        {
            pinchPressed = true;
            EnterLibraryApplyMode();
            if (debug) Debug.Log("[LibrarySpawn] Pinch → 进入 Library Apply Mode");
        }

        if (pinchPressed && pinchValue < pinchUpThreshold)
        {
            pinchPressed = false;
            HandlePinchRelease();
        }
    }

    void EnterLibraryApplyMode()
    {
        string itemText = GetItemText();
        currentTextObject = SpawnFromPrefab(itemText);
        if (currentTextObject == null) return;

        TextObjectManager.Instance?.RegisterTextObject(currentTextObject);

        activeApplier = currentTextObject.GetComponentInChildren<GazePinchPromptApplierOnDevice_Case2>(true);
        if (activeApplier != null)
        {
            if (activeApplier.textRoot == null)
                activeApplier.textRoot = currentTextObject;
            activeApplier.ForceEnterApplyMode();
        }
    }

    void HandlePinchRelease()
    {
        if (activeApplier == null)
        {
            currentTextObject = null;
            return;
        }

        var target = activeApplier.GetCurrentTarget();
        if (target != null)
        {
            // activeApplier.ApplyPromptToTarget(target);
            if (debug) Debug.Log($"[LibrarySpawn] Apply 成功 → {target.name}");
        }
        else
        {
            CancelApplyMode();
            if (debug) Debug.Log("[LibrarySpawn] 无目标 → 取消");
        }

        activeApplier = null;
        currentTextObject = null;
    }

    void CancelApplyMode()
    {
        if (activeApplier != null)
            activeApplier.ForceExitApplyMode();

        if (currentTextObject != null)
        {
            TextObjectManager.Instance?.UnregisterTextObject(currentTextObject);
            Destroy(currentTextObject);
            currentTextObject = null;
        }

        activeApplier = null;
        pinchPressed = false;
    }

    string GetItemText()
    {
        var txt = GetComponentsInChildren<UnityEngine.UI.Text>(true)
                    .FirstOrDefault(t => t.CompareTag(textTag));
        return txt != null ? txt.text.Trim() : "";
    }

    GameObject SpawnFromPrefab(string itemText)
    {
        if (string.IsNullOrEmpty(itemText))
            return null;

        if (textPrefab == null)
        {
            Debug.LogWarning("[LibrarySpawn] textPrefab 未设置");
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
        spawnPos += localOffset;

        Quaternion rot = Quaternion.LookRotation(cam.transform.forward);

        GameObject newObj = Instantiate(textPrefab, spawnPos, rot);
        newObj.SetActive(true);

        var store = newObj.GetComponent<CurrentTextStore>();
        if (store != null)
            store.CurrentText = itemText;

        var inputField = newObj.GetComponentInChildren<TMPro.TMP_InputField>(true);
        if (inputField != null)
            inputField.text = itemText;

        return newObj;
    }

}