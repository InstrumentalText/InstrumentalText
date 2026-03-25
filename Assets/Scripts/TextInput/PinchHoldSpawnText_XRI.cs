// using UnityEngine;
// using UnityEngine.InputSystem;

// public class PinchHoldSpawnText_XRI : MonoBehaviour
// {
//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;

//     [Header("Pinch Threshold")]
//     public float pinchDownThreshold = 0.8f;
//     public float pinchUpThreshold = 0.2f;

//     [Header("Hold Time")]
//     public float pinchHoldTime = 1.0f;

//     [Header("Spawn")]
//     public GameObject textPrefab;
//     public float spawnDistance = 0.5f;

//     [Header("Check Existing Text Objects")]
//     public string textObjectTag = "TextObject"; //Tag: TextObject

//     float pinchTimer = 0f;
//     bool pinchPressed = false;
//     bool spawnedThisPinch = false;

//     void Update()
//     {
//         float pinchValue = pinchAction.action.ReadValue<float>();

//         // Pinch start
//         if (!pinchPressed && pinchValue > pinchDownThreshold)
//         {
//             pinchPressed = true;
//             pinchTimer = 0f;
//         }

//         // Pinch release
//         if (pinchPressed && pinchValue < pinchUpThreshold)
//         {
//             pinchPressed = false;
//             pinchTimer = 0f;
//             spawnedThisPinch = false;
//         }

//         // Pinch holding
//         if (pinchPressed)
//         {
//             pinchTimer += Time.deltaTime;

//             if (pinchTimer >= pinchHoldTime && !spawnedThisPinch)
//             {
//                 SpawnText();
//                 spawnedThisPinch = true;
//             }
//         }
//     }

//     void SpawnText()
//     {
//         if (textPrefab == null) return;

//         //当前设置，场景中只允许一个TextObject存在
//         if (GameObject.FindGameObjectWithTag(textObjectTag) != null)
//         {
//             Debug.Log("Text Object already exists. Pinch detected but no spawn.");
//             return;
//         }

//         Camera cam = Camera.main;
//         if (cam == null) return;

//         Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
//         Quaternion rot = Quaternion.LookRotation(spawnPos - cam.transform.position);

//         GameObject newText = Instantiate(textPrefab, spawnPos, rot);

//         //生成的TextObject加上Tag
//         newText.tag = textObjectTag;

//         Debug.Log("Text object spawned");
//     }
// }


// using UnityEngine;
// using UnityEngine.InputSystem;

// /// <summary>
// /// 修改版 Pinch 生成 TextObject
// /// - 可以生成任意多个
// /// - 名字自动命名 Text object 1, 2, 3 ...
// /// - 仅在 BuilderModeHandler 激活时才可生成
// /// </summary>
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

//     [Header("Mode Control")]
//     public BuilderModeHandler builderModeHandler; // Builder 模式脚本引用

//     [Header("Debug")]
//     public bool debug = true;

//     private float pinchTimer = 0f;
//     private bool pinchPressed = false;
//     private int textCounter = 0; // 自动编号

//     void Update()
//     {
//         if (builderModeHandler == null || !builderModeHandler.IsBuilderActive())
//             return; // 退出模式或未激活 → 禁用生成

//         float pinchValue = pinchAction.action.ReadValue<float>();

//         // Pinch 开始
//         if (!pinchPressed && pinchValue > pinchDownThreshold)
//         {
//             pinchPressed = true;
//             pinchTimer = 0f;
//         }

//         // Pinch 释放
//         if (pinchPressed && pinchValue < pinchUpThreshold)
//         {
//             pinchPressed = false;
//             pinchTimer = 0f;
//         }

//         // 持续 Pinch
//         if (pinchPressed)
//         {
//             pinchTimer += Time.deltaTime;
//             if (pinchTimer >= pinchHoldTime)
//             {
//                 SpawnText();
//                 pinchTimer = 0f; // 重置计时器，可持续生成
//             }
//         }
//     }

//     void SpawnText()
//     {
//         if (textPrefab == null)
//         {
//             if (debug) Debug.LogWarning("[PinchHoldSpawnText] textPrefab 未设置");
//             return;
//         }

//         Camera cam = Camera.main;
//         if (cam == null)
//             return;

//         Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
//         Quaternion rot = Quaternion.LookRotation(spawnPos - cam.transform.position);

//         GameObject newText = Instantiate(textPrefab, spawnPos, rot);

//         // 自动命名
//         textCounter++;
//         newText.name = $"Text object {textCounter}";

//         if (debug) Debug.Log($"[PinchHoldSpawnText] 生成 {newText.name}");
//     }
// }



using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 修改版 Pinch 生成 TextObject
/// - 一次 pinch + release 只生成一个
/// - hold 1s 后生成
/// - pinch 持续时 Text 跟随手（相机）
/// - release 时固定位置
/// </summary>
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

    [Header("Mode Control")]
    public BuilderModeHandler builderModeHandler;

    [Header("Debug")]
    public bool debug = true;

    private float pinchTimer = 0f;
    private bool pinchPressed = false;

    private bool hasSpawnedThisPinch = false; // ✅ 本次 pinch 是否已经生成
    private GameObject currentTextObject = null; // ✅ 当前跟随对象

    private int textCounter = 0;

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

            if (debug) Debug.Log("[Spawn] Pinch Start");
        }

        // -------------------------------
        // Pinch Release（结束本轮）
        // -------------------------------
        if (pinchPressed && pinchValue < pinchUpThreshold)
        {
            pinchPressed = false;
            pinchTimer = 0f;

            if (currentTextObject != null)
            {
                if (debug) Debug.Log($"[Spawn] Pinch Release → 固定 {currentTextObject.name}");
            }

            currentTextObject = null;
            hasSpawnedThisPinch = false;

            return;
        }

        // -------------------------------
        // Pinch Holding
        // -------------------------------
        if (pinchPressed)
        {
            pinchTimer += Time.deltaTime;

            // ✅ 只生成一次
            if (!hasSpawnedThisPinch && pinchTimer >= pinchHoldTime)
            {
                currentTextObject = SpawnText();
                hasSpawnedThisPinch = true;

                if (debug) Debug.Log("[Spawn] 已生成 TextObject（进入跟随阶段）");
            }

            // -------------------------------
            // 跟随手（相机）
            // -------------------------------
            if (currentTextObject != null)
            {
                FollowHand(currentTextObject);
            }
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
        if (cam == null)
            return null;

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
        Quaternion rot = Quaternion.LookRotation(cam.transform.forward);

        GameObject newText = Instantiate(textPrefab, spawnPos, rot);

        textCounter++;
        newText.name = $"Text object {textCounter}";

        if (debug) Debug.Log($"[Spawn] 创建 {newText.name}");

        return newText;
    }

    void FollowHand(GameObject obj)
    {
        Camera cam = Camera.main;
        if (cam == null || obj == null) return;

        Vector3 targetPos = cam.transform.position + cam.transform.forward * spawnDistance;
        obj.transform.position = targetPos;

        obj.transform.rotation = Quaternion.LookRotation(cam.transform.forward);
    }
}