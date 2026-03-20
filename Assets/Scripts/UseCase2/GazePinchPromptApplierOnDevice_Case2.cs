// using UnityEngine;
// using UnityEngine.InputSystem;
// using System.Collections;

// [RequireComponent(typeof(Collider))]
// public class GazePinchPromptApplierOnDevice_Case2 : MonoBehaviour
// {
//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;
//     public float pinchHoldTime = 1.0f;
//     public float pinchDownThreshold = 0.8f;
//     public float pinchUpThreshold = 0.2f;

//     [Header("Text Object")]
//     public GameObject textRoot;

//     [Header("ApplyMode Animation")]
//     public float scaleFactor = 1.2f;      // 放大倍数
//     public int bounceTimes = 3;           // 弹跳次数
//     public float bounceDuration = 0.2f;   // 每次放大/缩小持续时间

//     private bool applyMode = false;
//     private bool pinchActive = false;
//     private float pinchTimer = 0f;

//     private LLMProcessorOnDevice_Case2 llmProcessor;
//     private Camera mainCamera;

//     private GameObject currentTarget = null;

//     void Start()
//     {
//         llmProcessor = FindObjectOfType<LLMProcessorOnDevice_Case2>();
//         mainCamera = Camera.main;

//         if (textRoot == null)
//             textRoot = transform.root.gameObject;

//         Debug.Log($"[GazePinchPromptApplierOnDevice_Case2] Start: TextRoot = {textRoot.name}");
//     }

//     void Update()
//     {
//         float pinchValue = pinchAction.action.ReadValue<float>();

//         // Phase 1: Enter ApplyMode
//         if (!applyMode)
//         {
//             if (IsGazingThisObject() && pinchValue > pinchDownThreshold)
//             {
//                 pinchTimer += Time.deltaTime;

//                 if (pinchTimer >= pinchHoldTime)
//                 {
//                     EnterApplyMode();
//                     pinchActive = true;
//                 }
//             }
//             else
//             {
//                 pinchTimer = 0f;
//             }

//             return;
//         }

//         // Phase 2: Wait for pinch release
//         if (pinchActive)
//         {
//             if (pinchValue < pinchUpThreshold)
//             {
//                 if (currentTarget != null)
//                 {
//                     Debug.Log($"[Applier] Pinch released → apply to {currentTarget.name}");
//                     ApplyPromptToTarget(currentTarget);
//                 }
//                 else
//                 {
//                     Debug.Log("[Applier] Pinch released but no target");
//                     ExitApplyModeWithoutApply();
//                 }
//             }
//         }
//     }

//     bool IsGazingThisObject()
//     {
//         Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
//         return GetComponent<Collider>().Raycast(ray, out RaycastHit hitInfo, 100f);
//     }

//     void EnterApplyMode()
//     {
//         applyMode = true;
//         pinchTimer = 0f;

//         Debug.Log("[Applier] Apply Mode Entered");

//         // 启动弹跳动画
//         if (textRoot != null)
//             StartCoroutine(BounceTextObject(textRoot, scaleFactor, bounceTimes, bounceDuration));
//     }

//     void ExitApplyModeWithoutApply()
//     {
//         applyMode = false;
//         pinchActive = false;
//         currentTarget = null;

//         Debug.Log("[Applier] Apply Mode Exited Without Apply");
//     }

//     public void ApplyPromptToTarget(GameObject target)
//     {
//         if (target == null || textRoot == null)
//         {
//             Debug.LogWarning("[Applier] target or textRoot null");
//             return;
//         }

//         if (llmProcessor == null)
//         {
//             Debug.LogWarning("[Applier] LLMProcessorOnDevice_Case2 not found");
//             return;
//         }

//         CurrentTextStore textStore = textRoot.GetComponent<CurrentTextStore>();

//         if (textStore == null || string.IsNullOrEmpty(textStore.CurrentText))
//         {
//             Debug.LogWarning("[Applier] CurrentText empty");
//             return;
//         }

//         string prompt = textStore.CurrentText;

//         Debug.Log($"[Applier] Applying '{prompt}' to {target.name}");

//         // 调用 Case2 的 LLM
//         llmProcessor.ProcessPrompt(target, prompt);

//         applyMode = false;
//         pinchActive = false;
//         currentTarget = null;

//         Debug.Log("[Applier] Apply Completed");
//     }

//     public void SetCurrentTarget(GameObject target)
//     {
//         currentTarget = target;
//         Debug.Log($"[Applier] CurrentTarget = {(target != null ? target.name : "null")}");
//     }

//     public GameObject GetCurrentTarget()
//     {
//         return currentTarget;
//     }

//     public bool IsApplyMode()
//     {
//         return applyMode;
//     }

//     // Coroutine: 弹跳动画
//     private IEnumerator BounceTextObject(GameObject obj, float scaleFactor, int times, float duration)
//     {
//         Vector3 originalScale = obj.transform.localScale;

//         for (int i = 0; i < times; i++)
//         {
//             // 放大
//             float timer = 0f;
//             while (timer < duration)
//             {
//                 obj.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, timer / duration);
//                 timer += Time.deltaTime;
//                 yield return null;
//             }
//             obj.transform.localScale = originalScale * scaleFactor;

//             // 缩小
//             timer = 0f;
//             while (timer < duration)
//             {
//                 obj.transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, timer / duration);
//                 timer += Time.deltaTime;
//                 yield return null;
//             }
//             obj.transform.localScale = originalScale;
//         }
//     }
// }


//带线条的单object交互版本
// using UnityEngine;
// using UnityEngine.InputSystem;
// using System.Collections;

// [RequireComponent(typeof(Collider))]
// public class GazePinchPromptApplierOnDevice_Case2 : MonoBehaviour
// {
//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;
//     public float pinchHoldTime = 1.0f;
//     public float pinchDownThreshold = 0.8f;
//     public float pinchUpThreshold = 0.2f;

//     [Header("Text Object")]
//     public GameObject textRoot;

//     [Header("ApplyMode Animation")]
//     public float scaleFactor = 1.2f;
//     public int bounceTimes = 3;
//     public float bounceDuration = 0.2f;

//     [Header("Dot Connection")]
//     public string dotName = "Dot";
//     public Color lineColor = Color.green;   // ✅ 可调颜色
//     public float lineWidth = 0.02f;         // ✅ 可调宽度

//     private bool applyMode = false;
//     private bool pinchActive = false;
//     private float pinchTimer = 0f;

//     private LLMProcessorOnDevice_Case2 llmProcessor;
//     private Camera mainCamera;

//     private GameObject currentTarget = null;

//     void Start()
//     {
//         llmProcessor = FindObjectOfType<LLMProcessorOnDevice_Case2>();
//         mainCamera = Camera.main;

//         if (textRoot == null)
//             textRoot = transform.root.gameObject;

//         Debug.Log($"[Applier] Start: TextRoot = {textRoot.name}");
//     }

//     void Update()
//     {
//         float pinchValue = pinchAction.action.ReadValue<float>();

//         // Phase 1
//         if (!applyMode)
//         {
//             if (IsGazingThisObject() && pinchValue > pinchDownThreshold)
//             {
//                 pinchTimer += Time.deltaTime;

//                 if (pinchTimer >= pinchHoldTime)
//                 {
//                     EnterApplyMode();
//                     pinchActive = true;
//                 }
//             }
//             else
//             {
//                 pinchTimer = 0f;
//             }

//             return;
//         }

//         // Phase 2
//         if (pinchActive)
//         {
//             if (pinchValue < pinchUpThreshold)
//             {
//                 if (currentTarget != null)
//                 {
//                     Debug.Log($"[Applier] Pinch released → apply to {currentTarget.name}");
//                     ApplyPromptToTarget(currentTarget);
//                 }
//                 else
//                 {
//                     Debug.Log("[Applier] No target");
//                     ExitApplyModeWithoutApply();
//                 }
//             }
//         }
//     }

//     bool IsGazingThisObject()
//     {
//         Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
//         return GetComponent<Collider>().Raycast(ray, out RaycastHit hitInfo, 100f);
//     }

//     void EnterApplyMode()
//     {
//         applyMode = true;
//         pinchTimer = 0f;

//         Debug.Log("[Applier] Apply Mode Entered");

//         if (textRoot != null)
//             StartCoroutine(BounceTextObject(textRoot, scaleFactor, bounceTimes, bounceDuration));
//     }

//     void ExitApplyModeWithoutApply()
//     {
//         applyMode = false;
//         pinchActive = false;
//         currentTarget = null;

//         Debug.Log("[Applier] Exit Apply Mode");
//     }

//     public void ApplyPromptToTarget(GameObject target)
//     {
//         if (target == null || textRoot == null)
//         {
//             Debug.LogWarning("[Applier] target or textRoot null");
//             return;
//         }

//         if (llmProcessor == null)
//         {
//             Debug.LogWarning("[Applier] LLMProcessor not found");
//             return;
//         }

//         CurrentTextStore textStore = textRoot.GetComponent<CurrentTextStore>();

//         if (textStore == null || string.IsNullOrEmpty(textStore.CurrentText))
//         {
//             Debug.LogWarning("[Applier] CurrentText empty");
//             return;
//         }

//         string prompt = textStore.CurrentText;

//         Debug.Log($"[Applier] Applying '{prompt}' to {target.name}");

//         llmProcessor.ProcessPrompt(target, prompt);

       
//         ConnectDots(textRoot, target);

//         applyMode = false;
//         pinchActive = false;
//         currentTarget = null;

//         Debug.Log("[Applier] Apply Completed");
//     }
    
//     void ConnectDots(GameObject a, GameObject b)
//     {
//         Transform dotA = a.transform.Find(dotName);
//         Transform dotB = b.transform.Find(dotName);

//         if (dotA == null || dotB == null)
//         {
//             Debug.LogWarning("[Dot] Dot not found");
//             return;
//         }

//         dotA.gameObject.SetActive(true);
//         dotB.gameObject.SetActive(true);

//         GameObject lineObj = new GameObject("DotConnection");
//         LineRenderer lr = lineObj.AddComponent<LineRenderer>();

//         // 材质（用默认 Unlit Color）
//         Material mat = new Material(Shader.Find("Unlit/Color"));
//         mat.color = lineColor;

//         lr.material = mat;
//         lr.startWidth = lineWidth;
//         lr.endWidth = lineWidth;
//         lr.positionCount = 2;
//         lr.useWorldSpace = true;

//         var updater = lineObj.AddComponent<LineUpdater>();
//         updater.Init(dotA, dotB, lr);
//     }

//     public void SetCurrentTarget(GameObject target)
//     {
//         currentTarget = target;
//     }

//     public GameObject GetCurrentTarget()
//     {
//         return currentTarget;
//     }

//     public bool IsApplyMode()
//     {
//         return applyMode;
//     }

//     // 动画
//     private IEnumerator BounceTextObject(GameObject obj, float scaleFactor, int times, float duration)
//     {
//         Vector3 originalScale = obj.transform.localScale;

//         for (int i = 0; i < times; i++)
//         {
//             float timer = 0f;
//             while (timer < duration)
//             {
//                 obj.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, timer / duration);
//                 timer += Time.deltaTime;
//                 yield return null;
//             }

//             timer = 0f;
//             while (timer < duration)
//             {
//                 obj.transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, timer / duration);
//                 timer += Time.deltaTime;
//                 yield return null;
//             }
//         }

//         obj.transform.localScale = originalScale;
//     }
// }



using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class GazePinchPromptApplierOnDevice_Case2 : MonoBehaviour
{
    // ✅ 全局当前激活 Applier
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
    public Color lineColor = Color.green;
    public float lineWidth = 0.02f;

    private bool applyMode = false;
    private bool pinchActive = false;
    private float pinchTimer = 0f;

    private LLMProcessorOnDevice_Case2 llmProcessor;
    private Camera mainCamera;

    private GameObject currentTarget = null;

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

        // Phase 1
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

        // Phase 2
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
                    Debug.Log("[Applier] No target");
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

        // ✅ 设置为当前激活
        ActiveInstance = this;

        Debug.Log($"[Applier] Apply Mode Entered: {gameObject.name}");

        if (textRoot != null)
            StartCoroutine(BounceTextObject(textRoot, scaleFactor, bounceTimes, bounceDuration));
    }

    void ExitApplyModeWithoutApply()
    {
        applyMode = false;
        pinchActive = false;

        // ✅ 只有自己是 active 才清除
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


        //添加全局的交互记录
        InteractionLibraryManager.Instance?.AddRecord(prompt, target.name, target);

        ConnectDots(textRoot, target);

        applyMode = false;
        pinchActive = false;

        // ✅ 清除 active
        if (ActiveInstance == this)
            ActiveInstance = null;

        currentTarget = null;

        Debug.Log("[Applier] Apply Completed");
    }

    void ConnectDots(GameObject a, GameObject b)
    {
        Transform dotA = a.transform.Find(dotName);
        Transform dotB = b.transform.Find(dotName);

        if (dotA == null || dotB == null)
        {
            Debug.LogWarning("[Dot] Dot not found");
            return;
        }

        dotA.gameObject.SetActive(true);
        dotB.gameObject.SetActive(true);

        GameObject lineObj = new GameObject("DotConnection");
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = lineColor;

        lr.material = mat;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = 2;
        lr.useWorldSpace = true;

        var updater = lineObj.AddComponent<LineUpdater>();
        updater.Init(dotA, dotB, lr);
    }

    public void SetCurrentTarget(GameObject target)
    {
        currentTarget = target;
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    public bool IsApplyMode()
    {
        return applyMode;
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
}

