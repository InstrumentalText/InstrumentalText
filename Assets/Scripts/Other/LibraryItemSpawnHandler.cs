// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.InputSystem;

// using System.Linq;

// [RequireComponent(typeof(Toggle))]
// public class LibraryItemSpawnHandler : MonoBehaviour
// {
//     private Toggle toggle;

//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;
//     public float pinchDownThreshold = 0.8f;
//     public float pinchUpThreshold = 0.2f;
//     public float pinchHoldTime = 1.0f;

//     [Header("Spawn Settings")]
//     public float spawnDistance = 0.6f;

//     [Header("Text Offset Relative to Library UI")]
//     public Vector3 localOffset = Vector3.zero;

//     [Header("Tag")]
//     public string textTag = "ItemText";

//     [Header("Gaze Reference (optional)")]
//     public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeInteractor;

//     [Header("Debug")]
//     public bool debug = true;

//     private bool pinchPressed = false;
//     private float pinchTimer = 0f;
//     private bool hasSpawned = false;
//     private bool interactionActive = false;

//     private GameObject currentTextObject = null;
//     private Collider[] currentColliders = null;
//     private Behaviour[] currentRaycasters = null;

//     void Awake()
//     {
//         toggle = GetComponent<Toggle>();

//         if (toggle == null)
//             Debug.LogError("[LibrarySpawn] Toggle 未找到！");

//         toggle.onValueChanged.AddListener(OnToggleChanged);
//     }

//     void OnDestroy()
//     {
//         if (toggle != null)
//             toggle.onValueChanged.RemoveListener(OnToggleChanged);
//     }

//     void OnToggleChanged(bool isOn)
//     {
//         interactionActive = isOn;

//         pinchPressed = false;
//         pinchTimer = 0f;
//         hasSpawned = false;
//         currentTextObject = null;
//         currentColliders = null;
//         currentRaycasters = null;

//         if (debug)
//             Debug.Log($"[LibrarySpawn] Toggle {(isOn ? "ON → 开启 Pinch 检测" : "OFF → 停止检测")}");
//     }

//     void Update()
//     {
//         if (!interactionActive)
//             return;

//         float pinchValue = pinchAction.action.ReadValue<float>();

//         // Pinch Start
//         if (!pinchPressed && pinchValue > pinchDownThreshold)
//         {
//             pinchPressed = true;
//             pinchTimer = 0f;
//             hasSpawned = false;
//             currentTextObject = null;
//             currentColliders = null;
//             currentRaycasters = null;

//             if (debug) Debug.Log("[LibrarySpawn] Pinch Start");
//         }

//         // Pinch Release
//         if (pinchPressed && pinchValue < pinchUpThreshold)
//         {
//             pinchPressed = false;
//             pinchTimer = 0f;

//             if (currentTextObject != null)
//             {
//                 var applier = currentTextObject.GetComponent<GazePinchPromptApplierOnDevice_Case2>();
//                 if (applier != null) applier.enabled = true;

//                 if (currentColliders != null)
//                 {
//                     foreach (var col in currentColliders)
//                         if (col != null) col.enabled = true;
//                 }

//                 if (currentRaycasters != null)
//                 {
//                     foreach (var b in currentRaycasters)
//                         if (b != null && b.GetType().Name == "TrackedDeviceGraphicRaycaster")
//                             b.enabled = true;
//                 }

//                 if (debug) Debug.Log($"[LibrarySpawn] Pinch Release → 固定 {currentTextObject.name}，恢复脚本、Collider、Raycaster");
//             }

//             currentTextObject = null;
//             currentColliders = null;
//             currentRaycasters = null;
//             hasSpawned = false;

//             return;
//         }

//         // Pinch Hold
//         if (pinchPressed)
//         {
//             pinchTimer += Time.deltaTime;

//             if (!hasSpawned && pinchTimer >= pinchHoldTime)
//             {
//                 string itemText = GetItemText();
//                 currentTextObject = SpawnFromExistingTextObject(itemText);

//                 if (currentTextObject != null)
//                 {
//                     var applier = currentTextObject.GetComponent<GazePinchPromptApplierOnDevice_Case2>();
//                     if (applier != null) applier.enabled = false;

//                     currentColliders = currentTextObject.GetComponentsInChildren<Collider>();
//                     foreach (var col in currentColliders)
//                         if (col != null) col.enabled = false;

//                     currentRaycasters = currentTextObject.GetComponentsInChildren<Behaviour>();
//                     foreach (var b in currentRaycasters)
//                         if (b != null && b.GetType().Name == "TrackedDeviceGraphicRaycaster")
//                             b.enabled = false;

//                     if (TextObjectManager.Instance != null)
//                         TextObjectManager.Instance.RegisterTextObject(currentTextObject);

//                     if (debug) Debug.Log("[LibrarySpawn] 已生成 TextObject（跟随阶段），禁用脚本、Collider、Raycaster");
//                 }

//                 hasSpawned = true;
//             }

//             // 跟随手部
//             if (currentTextObject != null)
//                 FollowHand(currentTextObject);
//         }
//     }

//     string GetItemText()
//     {
//         var txt = GetComponentsInChildren<UnityEngine.UI.Text>(true)
//                     .FirstOrDefault(t => t.CompareTag(textTag));
//         return txt != null ? txt.text.Trim() : "";
//     }

//     GameObject SpawnFromExistingTextObject(string itemText)
//     {
//         if (string.IsNullOrEmpty(itemText) || TextObjectManager.Instance == null)
//             return null;

//         var all = TextObjectManager.Instance.GetAllTextObjects();
//         GameObject source = null;

//         foreach (var obj in all)
//         {
//             if (obj == null) continue;
//             var store = obj.GetComponent<CurrentTextStore>();
//             if (store == null) continue;
//             if (!string.IsNullOrEmpty(store.CurrentText) && store.CurrentText.Trim() == itemText)
//             {
//                 source = obj;
//                 break;
//             }
//         }

//         if (source == null)
//         {
//             Debug.LogWarning($"[LibrarySpawn] 找不到匹配: {itemText}");
//             return null;
//         }

//         Camera cam = Camera.main;
//         if (cam == null) return null;

//         float gazeY = cam.transform.position.y;
//         if (gazeInteractor != null &&
//             gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
//         {
//             gazeY = hit.point.y;
//         }

//         Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
//         spawnPos += localOffset; // 相对 LibraryUI 的偏移
//         spawnPos.y = gazeY + localOffset.y;

//         Quaternion rot = Quaternion.LookRotation(cam.transform.forward);

//         GameObject newObj = Instantiate(source, spawnPos, rot);
//         newObj.SetActive(true);

//         return newObj;
//     }

//     void FollowHand(GameObject obj)
//     {
//         Camera cam = Camera.main;
//         if (cam == null || obj == null) return;

//         float gazeY = cam.transform.position.y;
//         if (gazeInteractor != null &&
//             gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
//         {
//             gazeY = hit.point.y;
//         }

//         Vector3 targetPos = cam.transform.position + cam.transform.forward * spawnDistance;
//         targetPos += localOffset;
//         targetPos.y = gazeY + localOffset.y;

//         obj.transform.position = targetPos;
//         obj.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
//     }

// }


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

    // ✅ 强类型缓存（关键修复）
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

        // Pinch Start
        if (!pinchPressed && pinchValue > pinchDownThreshold)
        {
            pinchPressed = true;
            pinchTimer = 0f;
            hasSpawned = false;

            ResetCurrentObjectRefs();

            if (debug) Debug.Log("[LibrarySpawn] Pinch Start");
        }

        // Pinch Release
        if (pinchPressed && pinchValue < pinchUpThreshold)
        {
            pinchPressed = false;
            pinchTimer = 0f;

            RestoreInteraction(); // ✅ 统一恢复

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
                currentTextObject = SpawnFromExistingTextObject(itemText);

                if (currentTextObject != null)
                {
                    CacheAndDisableInteraction(currentTextObject); // ✅ 统一禁用

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

    // =========================
    // ✅ 核心：缓存 + 禁用
    // =========================
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

    // =========================
    // ✅ 核心：恢复（完全对称）
    // =========================
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

    GameObject SpawnFromExistingTextObject(string itemText)
    {
        if (string.IsNullOrEmpty(itemText) || TextObjectManager.Instance == null)
            return null;

        var all = TextObjectManager.Instance.GetAllTextObjects();
        GameObject source = null;

        foreach (var obj in all)
        {
            if (obj == null) continue;
            var store = obj.GetComponent<CurrentTextStore>();
            if (store == null) continue;
            if (!string.IsNullOrEmpty(store.CurrentText) && store.CurrentText.Trim() == itemText)
            {
                source = obj;
                break;
            }
        }

        if (source == null)
        {
            Debug.LogWarning($"[LibrarySpawn] 找不到匹配: {itemText}");
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

        GameObject newObj = Instantiate(source, spawnPos, rot);
        newObj.SetActive(true);

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