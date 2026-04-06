using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
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

    [Header("Spawn")]
    public GameObject colorMirrorPrefab;
    public Material lineMaterial;
    public float lineWidth = 0.005f;

    private TextColorController textColorController;

    private int ballSpawnCount = 0;

    private bool applyMode = false;
    private bool pinchActive = false;
    private float pinchTimer = 0f;

    private Camera mainCamera;
    private ARAnchorManager anchorManager;
    private GameObject heldBall;

    private GameObject currentTarget = null;

    private List<GazePinchPromptApplierOnDevice_Case2> disabledAppliers = new List<GazePinchPromptApplierOnDevice_Case2>();
    private List<Behaviour> disabledRaycasters = new List<Behaviour>();

    void Start()
    {
        mainCamera = Camera.main;
        anchorManager = FindAnyObjectByType<ARAnchorManager>();

        textColorController = FindAnyObjectByType<TextColorController>();
        if (textColorController == null)
            Debug.LogWarning("[Applier] TextColorController not found in scene");

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
            ReleaseHeldBall();
            ExitApplyModeCommon();
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

    void ReleaseHeldBall()
    {
        if (colorMirrorPrefab == null)
        {
            Debug.LogWarning("[Applier] colorMirrorPrefab not assigned");
            return;
        }

        GameObject eyeCursor = GameObject.Find("EyeCursor(Clone)");
        Vector3 spawnPos = eyeCursor != null ? eyeCursor.transform.position : mainCamera.transform.position + mainCamera.transform.forward * 0.5f;
        Quaternion spawnRot = eyeCursor != null ? eyeCursor.transform.rotation : Quaternion.identity;

        if (eyeCursor == null)
            Debug.LogWarning("[Applier] EyeCursor(Clone) not found, using fallback position");

        heldBall = Instantiate(colorMirrorPrefab, spawnPos, spawnRot);
        Debug.Log($"[Applier] Spawned ColorMirror at EyeCursor position {spawnPos}");

        ConnectLineToTextRoot(heldBall);
        AttachToSpatialAnchor(heldBall);
        heldBall = null;

        ballSpawnCount++;
        if (ballSpawnCount == 2 && textColorController != null)
            textColorController.ActivateText1();
        else if (ballSpawnCount == 4 && textColorController != null)
            textColorController.ActivateText2();
    }

    void ConnectLineToTextRoot(GameObject ball)
    {
        if (textRoot == null) return;

        var lr = ball.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;

        if (lineMaterial != null)
            lr.material = lineMaterial;
        else
            lr.material = new Material(Shader.Find("Sprites/Default"));

        Transform plane = textRoot.transform.Find("Plane");
        Transform lineTarget = plane != null ? plane : textRoot.transform;

        var updater = ball.AddComponent<BallLineUpdater>();
        updater.Init(lr, lineTarget);
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
