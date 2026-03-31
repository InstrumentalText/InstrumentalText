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
    private float pinchTimer = 0f;
    private bool hasSpawned = false;
    private bool interactionActive = false;

    private GameObject currentTextObject = null;

    private Collider[] currentColliders = null;
    private TrackedDeviceGraphicRaycaster[] currentRaycasters = null;
    private GazePinchPromptApplierOnDevice_Case2[] currentAppliers = null;

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

        ResetState();

        if (debug)
            Debug.Log($"[LibrarySpawn] Toggle {(isOn ? "ON → 开启 Pinch 检测" : "OFF → 停止检测")}");
    }

    void Update()
    {
        if (!interactionActive)
            return;

        float pinchValue = pinchAction.action.ReadValue<float>();

        if (!pinchPressed && pinchValue > pinchDownThreshold)
        {
            pinchPressed = true;
            pinchTimer = 0f;
            hasSpawned = false;

            ResetCurrentObjectRefs();

            if (debug) Debug.Log("[LibrarySpawn] Pinch Start");
        }

        if (pinchPressed && pinchValue < pinchUpThreshold)
        {
            pinchPressed = false;
            pinchTimer = 0f;

            RestoreInteraction(); 

            ResetCurrentObjectRefs();
            hasSpawned = false;

            return;
        }

        // Pinch Hold
        if (pinchPressed)
        {
            pinchTimer += Time.deltaTime;

            if (!hasSpawned && pinchTimer >= pinchHoldTime)
            {
                string itemText = GetItemText();
                currentTextObject = SpawnFromPrefab(itemText);

                if (currentTextObject != null)
                {
                    CacheAndDisableInteraction(currentTextObject); 

                    if (TextObjectManager.Instance != null)
                        TextObjectManager.Instance.RegisterTextObject(currentTextObject);

                    if (debug) Debug.Log("[LibrarySpawn] 已生成 TextObject（跟随阶段）");
                }

                hasSpawned = true;
            }

            if (currentTextObject != null)
                FollowHand(currentTextObject);
        }
    }



    void CacheAndDisableInteraction(GameObject obj)
    {
        currentAppliers = obj.GetComponentsInChildren<GazePinchPromptApplierOnDevice_Case2>(true);
        currentColliders = obj.GetComponentsInChildren<Collider>(true);
        currentRaycasters = obj.GetComponentsInChildren<TrackedDeviceGraphicRaycaster>(true);

        foreach (var a in currentAppliers)
            if (a != null) a.enabled = false;

        foreach (var col in currentColliders)
            if (col != null) col.enabled = false;

        foreach (var ray in currentRaycasters)
            if (ray != null) ray.enabled = false;
    }


    void RestoreInteraction()
    {
        if (currentTextObject == null) return;

        foreach (var a in currentAppliers)
            if (a != null) a.enabled = true;

        foreach (var col in currentColliders)
            if (col != null) col.enabled = true;

        foreach (var ray in currentRaycasters)
            if (ray != null) ray.enabled = true;

        if (debug) Debug.Log($"[LibrarySpawn] Pinch Release → 完整恢复 {currentTextObject.name}");
    }

    void ResetCurrentObjectRefs()
    {
        currentTextObject = null;
        currentColliders = null;
        currentRaycasters = null;
        currentAppliers = null;
    }

    void ResetState()
    {
        pinchPressed = false;
        pinchTimer = 0f;
        hasSpawned = false;
        ResetCurrentObjectRefs();
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

    void FollowHand(GameObject obj)
    {
        Camera cam = Camera.main;
        if (cam == null || obj == null) return;

        Vector3 targetPos = cam.transform.position + cam.transform.forward * spawnDistance;
        targetPos += localOffset;

        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
    }
}