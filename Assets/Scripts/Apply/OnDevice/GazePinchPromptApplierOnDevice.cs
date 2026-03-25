using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class GazePinchPromptApplierOnDevice : MonoBehaviour
{
    [Header("Pinch Input")]
    public InputActionProperty pinchAction;
    public float pinchHoldTime = 1.0f;
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;

    [Header("Text Object")]
    public GameObject textRoot;

    private bool applyMode = false;
    private bool pinchActive = false;
    private float pinchTimer = 0f;

    private LLMProcessorOnDevice llmProcessor;
    private Camera mainCamera;

    private GameObject currentTarget = null;

    void Start()
    {
        llmProcessor = FindObjectOfType<LLMProcessorOnDevice>();
        mainCamera = Camera.main;

        if (textRoot == null)
            textRoot = transform.root.gameObject;

        Debug.Log($"[GazePinchPromptApplierOnDevice] Start: TextRoot = {textRoot.name}");
    }

    void Update()
    {
        float pinchValue = pinchAction.action.ReadValue<float>();

        // Phase 1: Enter ApplyMode
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

        // Phase 2: Wait for pinch release
        if (pinchActive)
        {
            if (pinchValue < pinchUpThreshold)
            {
                if (currentTarget != null)
                {
                    Debug.Log($"[Applier] Pinch released → apply to {currentTarget.name}");
                    ApplyPromptToTarget(currentTarget);
                }
                else
                {
                    Debug.Log("[Applier] Pinch released but no target");
                    ExitApplyModeWithoutApply();
                }
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

        ApplyTargetHighlighter.Instance?.SetApplyMode(true);

        Debug.Log("[Applier] Apply Mode Entered");
    }

    void ExitApplyModeWithoutApply()
    {
        applyMode = false;
        pinchActive = false;

        ApplyTargetHighlighter.Instance?.SetApplyMode(false);

        currentTarget = null;

        Debug.Log("[Applier] Apply Mode Exited Without Apply");
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

        applyMode = false;
        pinchActive = false;
        currentTarget = null;

        ApplyTargetHighlighter.Instance?.SetApplyMode(false);

        Debug.Log("[Applier] Apply Completed");
    }

    public void SetCurrentTarget(GameObject target)
    {
        currentTarget = target;

        Debug.Log($"[Applier] CurrentTarget = {(target != null ? target.name : "null")}");
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    public bool IsApplyMode()
    {
        return applyMode;
    }
}