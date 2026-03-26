using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class GazePinchPromptApplierOnDevice_Case2 : MonoBehaviour
{
    public static GazePinchPromptApplierOnDevice_Case2 ActiveInstance;

    [Header("Pinch Input")]
    public InputActionProperty pinchAction;
    public float pinchHoldTime = 1.0f;
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;

    [Header("Text Object")]
    public GameObject textRoot;

    [Header("ApplyMode Animation")]
    public float scaleFactor = 1.2f;
    public int bounceTimes = 3;
    public float bounceDuration = 0.2f;

    [Header("Dot Connection")]
    public string dotName = "Dot";

 
    public Material lineMaterial;

    private bool applyMode = false;
    private bool pinchActive = false;
    private float pinchTimer = 0f;

    private LLMProcessorOnDevice_Case2 llmProcessor;
    private Camera mainCamera;

    private GameObject currentTarget = null;

    private List<GameObject> spawnedLines = new List<GameObject>();

    void Start()
    {
        llmProcessor = FindObjectOfType<LLMProcessorOnDevice_Case2>();
        mainCamera = Camera.main;

        if (textRoot == null)
            textRoot = transform.root.gameObject;

        Debug.Log($"[Applier] Start: TextRoot = {textRoot.name}");
    }

    void Update()
    {
        float pinchValue = pinchAction.action.ReadValue<float>();

        // Phase 1: 进入 ApplyMode
        if (!applyMode)
        {
            if (IsGazingThisObject() && pinchValue > pinchDownThreshold)
            {
                pinchTimer += Time.deltaTime;

                if (pinchTimer >= pinchHoldTime)
                {
                    EnterApplyMode();
                    pinchActive = true;
                }
            }
            else
            {
                pinchTimer = 0f;
            }

            return;
        }

        // Phase 2: Pinch 释放 → Apply
        if (pinchActive && pinchValue < pinchUpThreshold)
        {
            if (currentTarget != null)
            {
                Debug.Log($"[Applier] Pinch released → apply to {currentTarget.name}");
                ApplyPromptToTarget(currentTarget);
            }
            else
            {
                Debug.Log("[Applier] No target");
                ExitApplyModeWithoutApply();
            }
        }
    }

    bool IsGazingThisObject()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        return GetComponent<Collider>().Raycast(ray, out RaycastHit hitInfo, 100f);
    }

    void EnterApplyMode()
    {
        applyMode = true;
        pinchTimer = 0f;

        ActiveInstance = this;

        Debug.Log($"[Applier] Apply Mode Entered: {gameObject.name}");

        if (textRoot != null)
            StartCoroutine(BounceTextObject(textRoot, scaleFactor, bounceTimes, bounceDuration));
    }

    void ExitApplyModeWithoutApply()
    {
        applyMode = false;
        pinchActive = false;

        if (ActiveInstance == this)
            ActiveInstance = null;

        currentTarget = null;

        Debug.Log("[Applier] Exit Apply Mode");
    }


    public void ApplyPromptToTarget(GameObject target)
    {
        if (target == null || textRoot == null)
        {
            Debug.LogWarning("[Applier] target or textRoot null");
            return;
        }

        if (llmProcessor == null)
        {
            Debug.LogWarning("[Applier] LLMProcessor not found");
            return;
        }

        CurrentTextStore textStore = textRoot.GetComponent<CurrentTextStore>();
        if (textStore == null || string.IsNullOrEmpty(textStore.CurrentText))
        {
            Debug.LogWarning("[Applier] CurrentText empty");
            return;
        }

        string prompt = textStore.CurrentText;
        Debug.Log($"[Applier] Applying '{prompt}' to {target.name}");

        llmProcessor.ProcessPrompt(target, prompt);

        if (TextObjectManager.Instance != null)
        {
            TextObjectManager.Instance.RegisterTextObject(textRoot);
            TextObjectManager.Instance.AddApplyTarget(textRoot, target);
        }


        var handler = target.GetComponent<TextNotificationHandler>();
        if (handler != null)
        {
            handler.RefreshNotifications();
        }
        else
        {
            Debug.LogWarning("[Applier] Target missing TextNotificationHandler");
        }

        applyMode = false;
        pinchActive = false;

        if (ActiveInstance == this)
            ActiveInstance = null;

        currentTarget = null;

        Debug.Log("[Applier] Apply Completed");
    }
    
    public void SetCurrentTarget(GameObject target) => currentTarget = target;
    public GameObject GetCurrentTarget() => currentTarget;
    public bool IsApplyMode() => applyMode;

    private IEnumerator BounceTextObject(GameObject obj, float scaleFactor, int times, float duration)
    {
        Vector3 originalScale = obj.transform.localScale;

        for (int i = 0; i < times; i++)
        {
            float timer = 0f;
            while (timer < duration)
            {
                obj.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
            while (timer < duration)
            {
                obj.transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        obj.transform.localScale = originalScale;
    }
}