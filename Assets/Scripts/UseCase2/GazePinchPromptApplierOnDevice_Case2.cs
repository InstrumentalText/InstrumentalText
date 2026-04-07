using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
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

    [Header("Apply Position")]
    public float aboveTargetOffset = 0.3f;
    public float verticalSpacing = 0.2f;

    [Header("Dot Connection")]
    public string dotName = "Dot";

    public Material lineMaterial;

    [Header("Multi-Target Connection")]
    public Color connectionLineColor = Color.white;
    public float connectionLineWidth = 0.02f;


    [Header("Apply Visual (Per TextObject)")]

    public Color appliedCavans = Color.red;
    public Color appliedUIColor = Color.green; // UI Image 颜色
    public Color appliedTextColor = Color.black; // TMP 文本颜色

    private bool applyMode = false;
    private bool pinchActive = false;
    private float pinchTimer = 0f;
    private Vector3 originalTextRootScale;

    private LLMProcessorOnDevice_Case2 llmProcessor;
    private Camera mainCamera;

    private GameObject currentTarget = null;

    private List<GameObject> appliedTargets = new List<GameObject>();
    private List<GameObject> connectionLineObjects = new List<GameObject>();
    private bool hasBeenApplied = false;

    private List<GazePinchPromptApplierOnDevice_Case2> disabledAppliers = new List<GazePinchPromptApplierOnDevice_Case2>();

    private List<Behaviour> disabledRaycasters = new List<Behaviour>();

    private ARAnchorManager anchorManager;

    void Start()
    {
        llmProcessor = FindObjectOfType<LLMProcessorOnDevice_Case2>();
        anchorManager = FindAnyObjectByType<ARAnchorManager>();
        mainCamera = Camera.main;

        if (textRoot == null)
            textRoot = transform.root.gameObject;

        Debug.Log($"[Applier] Start: TextRoot = {textRoot.name}");
    }

    void Update()
    {
        float pinchValue = pinchAction.action.ReadValue<float>();

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

        DisableOtherTextObjectAppliers();

        DisableAllRaycasters();

        if (textRoot != null)
        {
            originalTextRootScale = textRoot.transform.localScale;
            textRoot.transform.localScale = originalTextRootScale * scaleFactor;
        }
    }

    void ExitApplyModeWithoutApply()
    {
        ExitApplyModeCommon();
        Debug.Log("[Applier] Exit Apply Mode");
    }

    public void ApplyPromptToTarget(GameObject target)
    {
        if (target == null || textRoot == null)
        {
            Debug.LogWarning("[Applier] target or textRoot null");
            return;
        }

        CurrentTextStore textStore = textRoot.GetComponent<CurrentTextStore>();
        if (textStore == null || string.IsNullOrEmpty(textStore.CurrentText))
        {
            Debug.LogWarning("[Applier] CurrentText empty");
            return;
        }

        bool isTextObjectPlane = target.CompareTag("TextObjectPlane");

        if (!isTextObjectPlane)
        {
            if (llmProcessor == null)
            {
                Debug.LogWarning("[Applier] LLMProcessor not found");
                return;
            }

            string prompt = textStore.CurrentText;
            Debug.Log($"[Applier] Applying '{prompt}' to {target.name} (normal object)");

            llmProcessor.ProcessPrompt(target, prompt);

            if (!appliedTargets.Contains(target))
            {
                appliedTargets.Add(target);
                TextObjectManager.Instance.AddApplyTarget(textRoot, target);
            }

            if (!hasBeenApplied)
            {
                hasBeenApplied = true;
                ApplyVisualState(textRoot);
            }

            PositionTextAtCentroid();
        }
        else
        {
            if (llmProcessor == null)
            {
                Debug.LogWarning("[Applier] LLMProcessor not found");
                return;
            }

            string finalAction = "turn off";
            string finalText = "[LLM] " + finalAction;

            Debug.Log($"[Applier] [TextObjectPlane] Using action: {finalAction}");

            GameObject originalRoot = target.transform.parent != null ? target.transform.parent.gameObject : null;

            if (originalRoot == null || !originalRoot.CompareTag("TextObject"))
            {
                Debug.LogWarning("[Applier] Cannot find TextObject root for target: " + target.name);
                return;
            }

            List<GameObject> appliedObjects = TextObjectManager.Instance.GetApplyTargets(originalRoot);

            if (appliedObjects == null || appliedObjects.Count == 0)
            {
                Debug.Log("[Applier] No previously registered apply objects for this TextObject root: " + originalRoot.name);
            }

            GameObject newTextObject = Instantiate(textRoot);

            newTextObject.transform.position = new Vector3(-0.98f, -0.02f, -0.057f);
            newTextObject.SetActive(false); 


            CurrentTextStore newStore = newTextObject.GetComponent<CurrentTextStore>();
            if (newStore != null)
            {
                newStore.CurrentText = finalText;
            }
            else
            {
                Debug.LogWarning("[Applier] New TextObject missing CurrentTextStore");
            }


            Transform inputFieldTransform = newTextObject.transform.Find("Plane/Input Field World Keyboard/InputField (TMP)");
            if (inputFieldTransform != null)
            {
                var tmpInput = inputFieldTransform.GetComponent<TMPro.TMP_InputField>();
                if (tmpInput != null)
                {
                    tmpInput.text = finalText;
                }
                else
                {
                    Debug.LogWarning("[Applier] TMP_InputField component not found");
                }
            }
            else
            {
                Debug.LogWarning("[Applier] InputField (TMP) not found under canvas");
            }


            TextObjectManager.Instance.RegisterTextObject(newTextObject);


            foreach (var appliedTarget in appliedObjects)
            {
                if (appliedTarget == null) continue;

                Debug.Log($"[Applier] Executing hardcoded turn off on: {appliedTarget.name}");

                ExecuteHardcodedTurnOff(appliedTarget);

                TextObjectManager.Instance.AddApplyTarget(newTextObject, appliedTarget);

                var handler = appliedTarget.GetComponent<TextNotificationHandler>();
                if (handler != null)
                    handler.RefreshNotifications();
            }

        }

        AttachToSpatialAnchor(textRoot);
        ExitApplyModeCommon();

        Debug.Log("[Applier] Apply Completed");
    }



    void ExitApplyModeCommon()
    {
        applyMode = false;
        pinchActive = false;

        if (ActiveInstance == this)
            ActiveInstance = null;

        currentTarget = null;

        if (textRoot != null)
            textRoot.transform.localScale = originalTextRootScale;

        RestoreOtherTextObjectAppliers();
        RestoreAllRaycasters();
    }

    public void SetCurrentTarget(GameObject target) => currentTarget = target;
    public GameObject GetCurrentTarget() => currentTarget;
    public bool IsApplyMode() => applyMode;

    public void SetConnectionLinesVisible(bool visible)
    {
        foreach (var obj in connectionLineObjects)
            if (obj != null) obj.SetActive(visible);
    }

    public void ForceEnterApplyMode()
    {
        EnterApplyMode();
    }

    public void ForceExitApplyMode()
    {
        ExitApplyModeCommon();
    }

    void DisableOtherTextObjectAppliers()
    {
        disabledAppliers.Clear();

        if (TextObjectManager.Instance == null) return;

        var allTexts = TextObjectManager.Instance.GetAllTextObjects();

        foreach (var obj in allTexts)
        {
            if (obj == null) continue;
            if (obj == this.textRoot) continue;

            var applier = obj.GetComponentInChildren<GazePinchPromptApplierOnDevice_Case2>();
            if (applier != null && applier.enabled)
            {
                applier.enabled = false;
                disabledAppliers.Add(applier);
            }
        }
    }

    void RestoreOtherTextObjectAppliers()
    {
        foreach (var applier in disabledAppliers)
        {
            if (applier != null)
                applier.enabled = true;
        }

        disabledAppliers.Clear();
    }

    void DisableAllRaycasters()
    {
        disabledRaycasters.Clear();

        if (TextObjectManager.Instance == null) return;

        var allTexts = TextObjectManager.Instance.GetAllTextObjects();

        foreach (var obj in allTexts)
        {
            if (obj == null) continue;

            var behaviours = obj.GetComponentsInChildren<Behaviour>();

            foreach (var b in behaviours)
            {
                if (b != null && b.enabled && b.GetType().Name == "TrackedDeviceGraphicRaycaster")
                {
                    b.enabled = false;
                    disabledRaycasters.Add(b);
                }
            }
        }
    }

    void RestoreAllRaycasters()
    {
        foreach (var b in disabledRaycasters)
        {
            if (b != null)
                b.enabled = true;
        }

        disabledRaycasters.Clear();
    }

    void PositionTextAtCentroid()
    {
        if (appliedTargets.Count == 0) return;

        Vector3 centroid = Vector3.zero;
        float maxTopY = float.MinValue;

        foreach (var t in appliedTargets)
        {
            if (t == null) continue;
            centroid += t.transform.position;
            Collider col = t.GetComponentInChildren<Collider>();
            float topY = col != null ? col.bounds.max.y : t.transform.position.y;
            if (topY > maxTopY) maxTopY = topY;
        }

        centroid /= appliedTargets.Count;

        Vector3 toCamera = mainCamera.transform.position - centroid;
        toCamera.y = 0f;
        Quaternion rot = toCamera.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(toCamera.normalized) * Quaternion.Euler(0, 180, 0)
            : textRoot.transform.rotation;

        // Stack all texts that share the exact same target set vertically
        List<GameObject> colocated = GetColocatedTexts();
        for (int i = 0; i < colocated.Count; i++)
        {
            if (colocated[i] == null) continue;
            colocated[i].transform.SetPositionAndRotation(new Vector3(centroid.x, maxTopY + aboveTargetOffset + i * verticalSpacing, centroid.z), rot);
        }

        RebuildConnectionLines();
    }

    List<GameObject> GetColocatedTexts()
    {
        var result = new List<GameObject>();
        if (TextObjectManager.Instance == null) return result;
        var allTexts = TextObjectManager.Instance.GetAllTextObjects();

        foreach (var textObj in allTexts)
        {
            if (textObj == null) continue;
            var targets = TextObjectManager.Instance.GetApplyTargets(textObj);
            if (targets == null || targets.Count != appliedTargets.Count) continue;

            bool match = true;
            foreach (var t in appliedTargets)
            {
                if (!targets.Contains(t)) { match = false; break; }
            }
            if (match) result.Add(textObj);
        }

        return result;
    }

    void RebuildConnectionLines()
    {
        foreach (var obj in connectionLineObjects)
            if (obj != null) Destroy(obj);
        connectionLineObjects.Clear();

        if (appliedTargets.Count < 2) return;

        foreach (var target in appliedTargets)
        {
            if (target == null) continue;

            GameObject lineObj = new GameObject("ConnectionLine");
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();

            Material mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = connectionLineColor;
            lr.material = mat;

            lr.startColor = connectionLineColor;
            lr.endColor = connectionLineColor;
            lr.startWidth = connectionLineWidth;
            lr.endWidth = connectionLineWidth;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.SetPosition(0, textRoot.transform.position);
            lr.SetPosition(1, target.transform.position);

            connectionLineObjects.Add(lineObj);
        }
    }

    void ApplyCanvasColor(GameObject textObj)
    {
        // 找到 Plane
        Transform plane = textObj.transform.Find("Plane");
        if (plane == null)
        {
            Debug.LogWarning("[Applier] Plane not found under textObj");
            return;
        }

        Transform canvas = plane.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogWarning("[Applier] Canvas not found under Plane");
            return;
        }

        UnityEngine.UI.Image[] images = canvas.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        foreach (var img in images)
        {
            img.color = appliedCavans;
        }

        Debug.Log("[Applier] Canvas Image color applied!");
    }

    void ApplyUIColor(GameObject textObj)
    {
        Transform plane = textObj.transform.Find("Plane");
        if (plane == null) return;

        Transform canvas = plane.Find("Input Field World Keyboard");
        if (canvas == null)
        {
            Debug.LogWarning("[Applier] Canvas not found");
            return;
        }

        var images = canvas.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        foreach (var img in images)
        {
            img.color = appliedUIColor;
        }
    }


    void ApplyTMPColor(GameObject textObj)
    {
        Transform plane = textObj.transform.Find("Plane");
        if (plane == null) return;

        Transform inputField = plane.Find("Input Field World Keyboard/InputField (TMP)");
        if (inputField == null)
        {
            Debug.LogWarning("[Applier] TMP InputField not found");
            return;
        }

        var input = inputField.GetComponent<TMPro.TMP_InputField>();
        if (input != null)
        {
            if (input.textComponent != null)
                input.textComponent.color = appliedTextColor;

            if (input.placeholder is TMPro.TMP_Text placeholder)
                placeholder.color = appliedTextColor * 0.5f;
        }

        var texts = inputField.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (var t in texts)
        {
            t.color = appliedTextColor;
        }
    }

    void ApplyVisualState(GameObject textObj)
    {
        ApplyCanvasColor(textObj);
        ApplyUIColor(textObj);
        ApplyTMPColor(textObj);
    }

    private void ExecuteHardcodedTurnOff(GameObject target)
    {
        var handlers = target.GetComponents<IActionHandler>();
        var context = new ExecutionContext(target);

        foreach (var handler in handlers)
        {
            string actionType = null;

            if (handler is LightHandler)
                actionType = "light.off";
            else if (handler is MusicPlayerHandler)
                actionType = "switch.off";

            if (actionType == null) continue;

            var result = handler.Execute(actionType, "{}", context);
            Debug.Log($"[Applier] [T2T] {actionType} on '{target.name}' → success={result.success}");
        }
    }

    async void AttachToSpatialAnchor(GameObject obj)
    {
        if (anchorManager == null)
        {
            Debug.LogWarning($"[Applier] ARAnchorManager not found, cannot anchor {obj.name}");
            return;
        }

        var pose = new Pose(obj.transform.position, obj.transform.rotation);
        var result = await anchorManager.TryAddAnchorAsync(pose);

        if (result.status.IsSuccess())
        {
            var anchor = result.value;
            obj.transform.SetParent(anchor.transform, true);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            Debug.Log($"[Applier] {obj.name} anchored at {anchor.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[Applier] Failed to create anchor for {obj.name}: {result.status}");
        }
    }
}