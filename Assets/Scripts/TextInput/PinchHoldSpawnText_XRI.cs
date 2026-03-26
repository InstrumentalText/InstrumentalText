// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.Interaction.Toolkit;


// public class PinchHoldSpawnText_XRI : MonoBehaviour
// {
//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;

//     [Header("Pinch Threshold & Hold")]
//     public float pinchDownThreshold = 0.8f;
//     public float pinchUpThreshold = 0.2f;
//     public float pinchHoldTime = 1.0f;

//     [Header("Spawn Settings")]
//     public GameObject textPrefab;
//     public float spawnDistance = 0.5f;

//     [Header("Gaze Reference (optional)")]
//     public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeInteractor;

//     [Header("Mode Control")]
//     public BuilderModeHandler builderModeHandler;

//     [Header("Debug")]
//     public bool debug = true;

//     private float pinchTimer = 0f;
//     private bool pinchPressed = false;

//     private bool hasSpawnedThisPinch = false; 
//     private GameObject currentTextObject = null; 

//     private int textCounter = 0;

//     void Update()
//     {
//         if (builderModeHandler == null || !builderModeHandler.IsBuilderActive())
//             return;

//         float pinchValue = pinchAction.action.ReadValue<float>();


//         if (!pinchPressed && pinchValue > pinchDownThreshold)
//         {
//             pinchPressed = true;
//             pinchTimer = 0f;
//             hasSpawnedThisPinch = false;
//             currentTextObject = null;

//             if (debug) Debug.Log("[Spawn] Pinch Start");
//         }


//         if (pinchPressed && pinchValue < pinchUpThreshold)
//         {
//             pinchPressed = false;
//             pinchTimer = 0f;

//             if (currentTextObject != null)
//             {
//                 if (debug) Debug.Log($"[Spawn] Pinch Release → 固定 {currentTextObject.name}");
//             }

//             currentTextObject = null;
//             hasSpawnedThisPinch = false;

//             return;
//         }


//         if (pinchPressed)
//         {
//             pinchTimer += Time.deltaTime;

//             if (!hasSpawnedThisPinch && pinchTimer >= pinchHoldTime)
//             {
//                 currentTextObject = SpawnText();
//                 hasSpawnedThisPinch = true;

//                 if (debug) Debug.Log("[Spawn] 已生成 TextObject（进入跟随阶段）");
//             }

//             if (currentTextObject != null)
//             {
//                 FollowHand(currentTextObject);
//             }
//         }
//     }

//     GameObject SpawnText()
//     {
//         if (textPrefab == null)
//         {
//             if (debug) Debug.LogWarning("[Spawn] textPrefab 未设置");
//             return null;
//         }

//         Camera cam = Camera.main;
//         if (cam == null)
//             return null;

//         float gazeY = cam.transform.position.y; 
//         if (gazeInteractor != null &&
//             gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
//         {
//             gazeY = hit.point.y;
//         }

//         Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
//         spawnPos.y = gazeY;

//         Quaternion rot = Quaternion.LookRotation(cam.transform.forward);

//         GameObject newText = Instantiate(textPrefab, spawnPos, rot);

//         textCounter++;
//         newText.name = $"Text object {textCounter}";

//         if (debug) Debug.Log($"[Spawn] 创建 {newText.name}，位置 {spawnPos}");

//         return newText;
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
//         targetPos.y = gazeY;

//         obj.transform.position = targetPos;
//         obj.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
//     }
// }




using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PinchHoldSpawnText_XRI : MonoBehaviour
{
    [Header("Pinch Input")]
    public InputActionProperty pinchAction;

    [Header("Pinch Threshold & Hold")]
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;
    public float pinchHoldTime = 1.0f;

    [Header("Spawn Settings")]
    public GameObject textPrefab;
    public float spawnDistance = 0.5f;

    [Header("Gaze Reference (optional)")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeInteractor;

    [Header("Mode Control")]
    public BuilderModeHandler builderModeHandler;

    [Header("Debug")]
    public bool debug = true;

    private float pinchTimer = 0f;
    private bool pinchPressed = false;

    private bool hasSpawnedThisPinch = false;
    private GameObject currentTextObject = null;

    private int textCounter = 0;

    private Collider[] currentColliders = null;
    private Behaviour[] currentRaycasters = null;

    void Update()
    {
        if (builderModeHandler == null || !builderModeHandler.IsBuilderActive())
            return;

        float pinchValue = pinchAction.action.ReadValue<float>();

        // -------------------------------
        // Pinch Start
        // -------------------------------
        if (!pinchPressed && pinchValue > pinchDownThreshold)
        {
            pinchPressed = true;
            pinchTimer = 0f;
            hasSpawnedThisPinch = false;
            currentTextObject = null;
            currentColliders = null;
            currentRaycasters = null;

            if (debug) Debug.Log("[Spawn] Pinch Start");
        }

        // -------------------------------
        // Pinch Release
        // -------------------------------
        if (pinchPressed && pinchValue < pinchUpThreshold)
        {
            pinchPressed = false;
            pinchTimer = 0f;

            if (currentTextObject != null)
            {
                var applier = currentTextObject.GetComponent<GazePinchPromptApplierOnDevice_Case2>();
                if (applier != null) applier.enabled = true;

                if (currentColliders != null)
                {
                    foreach (var col in currentColliders)
                        if (col != null) col.enabled = true;
                }

                if (currentRaycasters != null)
                {
                    foreach (var ray in currentRaycasters)
                        if (ray != null && ray.GetType().Name == "TrackedDeviceGraphicRaycaster")
                            ray.enabled = true;
                }

                if (debug) Debug.Log($"[Spawn] Pinch Release → 固定 {currentTextObject.name}，恢复脚本、Collider、Raycaster");
            }

            currentTextObject = null;
            hasSpawnedThisPinch = false;
            currentColliders = null;
            currentRaycasters = null;

            return;
        }


        if (pinchPressed)
        {
            pinchTimer += Time.deltaTime;

            if (!hasSpawnedThisPinch && pinchTimer >= pinchHoldTime)
            {
                currentTextObject = SpawnText();
                hasSpawnedThisPinch = true;

                if (currentTextObject != null)
                {
                    var applier = currentTextObject.GetComponent<GazePinchPromptApplierOnDevice_Case2>();
                    if (applier != null) applier.enabled = false;

                    currentColliders = currentTextObject.GetComponentsInChildren<Collider>();
                    foreach (var col in currentColliders)
                        if (col != null) col.enabled = false;

                    currentRaycasters = currentTextObject.GetComponentsInChildren<Behaviour>();
                    foreach (var b in currentRaycasters)
                        if (b != null && b.GetType().Name == "TrackedDeviceGraphicRaycaster")
                            b.enabled = false;
                }

                if (debug) Debug.Log("[Spawn] 已生成 TextObject（跟随阶段），禁用脚本、Collider、Raycaster");
            }

            if (currentTextObject != null)
                FollowHand(currentTextObject);
        }
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

    void FollowHand(GameObject obj)
    {
        Camera cam = Camera.main;
        if (cam == null || obj == null) return;

        float gazeY = cam.transform.position.y;
        if (gazeInteractor != null &&
            gazeInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            gazeY = hit.point.y;
        }

        Vector3 targetPos = cam.transform.position + cam.transform.forward * spawnDistance;
        targetPos.y = gazeY;

        obj.transform.position = targetPos;
        obj.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
    }
}