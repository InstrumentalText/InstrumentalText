using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
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

    [Header("Tag")]
    public string textTag = "ItemText";

    [Header("Debug")]
    public bool debug = true;

    private bool pinchPressed = false;
    private float pinchTimer = 0f;
    private bool hasSpawned = false;

    private bool interactionActive = false;

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
        if (isOn)
        {
            interactionActive = true;

            pinchPressed = false;
            pinchTimer = 0f;
            hasSpawned = false;

            if (debug)
                Debug.Log("[LibrarySpawn] Toggle ON → 开启 Pinch 检测");
        }
        else
        {
            interactionActive = false;

            if (debug)
                Debug.Log("[LibrarySpawn] Toggle OFF → 停止检测");
        }
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
        }


        if (pinchPressed && pinchValue < pinchUpThreshold)
        {
            pinchPressed = false;
            pinchTimer = 0f;
        }


        if (pinchPressed && !hasSpawned)
        {
            pinchTimer += Time.deltaTime;

            if (pinchTimer >= pinchHoldTime)
            {
                string itemText = GetItemText();

                GameObject newObj = SpawnFromExistingTextObject(itemText);

                if (newObj != null)
                {
                    if (TextObjectManager.Instance != null)
                        TextObjectManager.Instance.RegisterTextObject(newObj);
                }

                hasSpawned = true;

                EndInteraction();

                if (debug)
                    Debug.Log("[LibrarySpawn] 完成交互 → 已复制 + 已注册");
            }
        }
    }


    void EndInteraction()
    {
        interactionActive = false;

        if (toggle != null)
            toggle.isOn = false;

        pinchPressed = false;
        pinchTimer = 0f;
    }

    string GetItemText()
    {
        var txt = GetComponentsInChildren<UnityEngine.UI.Text>(true)
                    .FirstOrDefault(t => t.CompareTag(textTag));

        if (txt != null)
            return txt.text.Trim();

        return "";
    }

    GameObject SpawnFromExistingTextObject(string itemText)
    {
        if (string.IsNullOrEmpty(itemText))
            return null;

        if (TextObjectManager.Instance == null)
            return null;

        var all = TextObjectManager.Instance.GetAllTextObjects();

        GameObject source = null;

        foreach (var obj in all)
        {
            if (obj == null) continue;

            var store = obj.GetComponent<CurrentTextStore>();
            if (store == null) continue;

            if (!string.IsNullOrEmpty(store.CurrentText) &&
                store.CurrentText.Trim() == itemText)
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

        Vector3 spawnPos =
            cam.transform.position +
            cam.transform.right * 0.5f +
            cam.transform.forward * spawnDistance;

        Quaternion rot = Quaternion.LookRotation(cam.transform.forward);

        GameObject newObj = Instantiate(source, spawnPos, rot);
        newObj.SetActive(true);

        return newObj;
    }
}