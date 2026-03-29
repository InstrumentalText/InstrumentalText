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


    [Header("Apply Visual (Per TextObject)")]

    public Color appliedCavans = Color.red; 
    public Color appliedUIColor = Color.green; // UI Image 颜色
    public Color appliedTextColor = Color.black; // TMP 文本颜色


    private bool applyMode = false;
    private bool pinchActive = false;
    private float pinchTimer = 0f;

    private LLMProcessorOnDevice_Case2 llmProcessor;
    private Camera mainCamera;

    private GameObject currentTarget = null;

    private List<GameObject> spawnedLines = new List<GameObject>();

    private List<GazePinchPromptApplierOnDevice_Case2> disabledAppliers = new List<GazePinchPromptApplierOnDevice_Case2>();

    private List<Behaviour> disabledRaycasters = new List<Behaviour>();

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
            StartCoroutine(BounceTextObject(textRoot, scaleFactor, bounceTimes, bounceDuration));
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
            TextObjectManager.Instance.AddApplyTarget(textRoot, target);
            //添加apply后的UI调整
            ApplyVisualState(textRoot);
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


            Transform inputFieldTransform = newTextObject.transform.Find("canvas/InputField (TMP)");
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

                Debug.Log($"[Applier] Applying action '{finalAction}' to applied target: {appliedTarget.name}");

                llmProcessor.ProcessPrompt(appliedTarget, finalAction);


                TextObjectManager.Instance.AddApplyTarget(newTextObject, appliedTarget);
            }
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

        RestoreOtherTextObjectAppliers();
        RestoreAllRaycasters();
    }

    public void SetCurrentTarget(GameObject target) => currentTarget = target;
    public GameObject GetCurrentTarget() => currentTarget;
    public bool IsApplyMode() => applyMode;

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


}