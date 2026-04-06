using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoxCubeGenerator : MonoBehaviour
{
    [Header("Trigger Input (Controller Index Trigger)")]
    public InputActionProperty leftTriggerAction;
    public InputActionProperty rightTriggerAction;
    public float triggerDownThreshold = 0.8f;
    public float triggerUpThreshold = 0.2f;

    [Header("Controller Transforms")]
    public Transform leftController;
    public Transform rightController;

    [Header("Cube Object (Define Mode)")]
    public GameObject cubeObject;
    public float minSize = 0.05f;

    [Header("Managed Cubes")]
    public List<GameObject> cubeObjects = new List<GameObject>();
    public float transparentAlpha = 0;

    [Header("Extra Grab Object (Disable Grab Only)")]
    public GameObject extraGrabObject;

    [Header("Debug")]
    public bool debug = true;

    private bool defining = false;
    private bool leftWasDown = false;

    private ARAnchorManager anchorManager;

    private void Awake()
    {
        anchorManager = FindAnyObjectByType<ARAnchorManager>();
        if (anchorManager == null)
            Debug.LogWarning("[BoxCubeGenerator] 场景中没有找到 ARAnchorManager");
    }

    private void Update()
    {
        float leftValue = leftTriggerAction.action.ReadValue<float>();
        float rightValue = rightTriggerAction.action.ReadValue<float>();

        bool leftDown = leftValue > triggerDownThreshold;
        bool rightDown = rightValue > triggerDownThreshold;
        bool leftUp = leftValue < triggerUpThreshold;
        bool rightUp = rightValue < triggerUpThreshold;

        // 左扳机单独按下（不含右扳机）→ 透明化所有 cube 并禁用抓取
        bool leftJustDown = leftDown && !leftWasDown;
        if (leftJustDown && !rightDown)
        {
            SetCubesTransparent();
        }
        leftWasDown = leftDown;

        if (cubeObject == null)
            return;

        if (!defining)
        {
            if (leftDown && rightDown)
            {
                defining = true;
                DetachFromAnchor();

                if (debug)
                    Debug.Log("[BoxCubeGenerator] 双手 Trigger → 开始定义 Cube");
            }
        }
        else
        {
            if (!leftUp && !rightUp)
            {
                UpdateCubeFromCorners(leftController.position, rightController.position);

                if (debug && Time.frameCount % 30 == 0)
                    Debug.Log($"[BoxCubeGenerator] 预览中: L={leftController.position}, R={rightController.position}, scale={cubeObject.transform.localScale}");
            }

            if (leftUp && rightUp)
            {
                defining = false;
                UpdateCubeCollider();
                ReattachToAnchor();

                if (debug)
                    Debug.Log($"[BoxCubeGenerator] 松开 → Cube 确定: pos={cubeObject.transform.position}, scale={cubeObject.transform.localScale}");
            }
        }
    }

    private void SetCubesTransparent()
    {
        foreach (var cube in cubeObjects)
        {
            if (cube == null) continue;

            var rend = cube.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = rend.material;
                mat.SetFloat("_Mode", 3);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
                Color c = mat.color;
                mat.color = new Color(c.r, c.g, c.b, transparentAlpha);
            }

            var grab = cube.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = false;

            if (debug)
                Debug.Log($"[BoxCubeGenerator] {cube.name} 已设为透明并禁用 XRGrabInteractable");
        }

        if (extraGrabObject != null)
        {
            if (extraGrabObject.TryGetComponent<XRGrabInteractable>(out var grab))
                grab.enabled = false;

            if (debug)
                Debug.Log($"[BoxCubeGenerator] {extraGrabObject.name} 已禁用 XRGrabInteractable");
        }

        // 为所有 cube 和 extraGrabObject 添加 spatial anchor
        foreach (var cube in cubeObjects)
        {
            if (cube != null)
                AttachAnchorAsync(cube);
        }

        if (extraGrabObject != null)
            AttachAnchorAsync(extraGrabObject);
    }

    private void UpdateCubeFromCorners(Vector3 a, Vector3 b)
    {
        Vector3 center = (a + b) * 0.5f;
        Vector3 size = new Vector3(
            Mathf.Max(Mathf.Abs(a.x - b.x), minSize),
            Mathf.Max(Mathf.Abs(a.y - b.y), minSize),
            Mathf.Max(Mathf.Abs(a.z - b.z), minSize)
        );

        cubeObject.transform.position = center;
        cubeObject.transform.localScale = size;

        Vector3 headForward = Camera.main.transform.forward;
        headForward.y = 0f;
        if (headForward.sqrMagnitude > 0.001f)
            cubeObject.transform.rotation = Quaternion.LookRotation(headForward);
    }

    private async void AttachAnchorAsync(GameObject obj)
    {
        if (anchorManager == null)
        {
            Debug.LogWarning("[BoxCubeGenerator] ARAnchorManager 不存在，跳过 anchor 注册");
            return;
        }

        // 若已有 anchor 父节点，先脱离
        var existing = obj.GetComponentInParent<ARAnchor>();
        if (existing != null)
        {
            obj.transform.SetParent(null, true);
            Destroy(existing.gameObject);
        }

        var pose = new Pose(obj.transform.position, obj.transform.rotation);
        var result = await anchorManager.TryAddAnchorAsync(pose);

        if (result.status.IsSuccess())
        {
            var anchor = result.value;
            obj.transform.SetParent(anchor.transform, true);
            obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (debug)
                Debug.Log($"[BoxCubeGenerator] {obj.name} anchor 注册成功: pos={obj.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[BoxCubeGenerator] {obj.name} anchor 注册失败: {result.status}");
        }
    }

    private void DetachFromAnchor()
    {
        var anchor = cubeObject.GetComponentInParent<ARAnchor>();
        if (anchor != null)
        {
            cubeObject.transform.SetParent(null, true);

            if (debug)
                Debug.Log($"[BoxCubeGenerator] 从 Anchor 脱离: {anchor.name}");

            Destroy(anchor.gameObject);
        }
    }

    private async void ReattachToAnchor()
    {
        if (anchorManager == null)
        {
            Debug.LogWarning("[BoxCubeGenerator] ARAnchorManager 不存在，跳过 anchor 注册");
            return;
        }

        var pose = new Pose(cubeObject.transform.position, cubeObject.transform.rotation);
        var result = await anchorManager.TryAddAnchorAsync(pose);

        if (result.status.IsSuccess())
        {
            var anchor = result.value;
            cubeObject.transform.SetParent(anchor.transform, true);
            cubeObject.transform.localPosition = Vector3.zero;
            cubeObject.transform.localRotation = Quaternion.identity;

            if (debug)
                Debug.Log($"[BoxCubeGenerator] 重新注册 Anchor 成功: pos={cubeObject.transform.position}");
        }
        else
        {
            Debug.LogWarning($"[BoxCubeGenerator] 重新注册 Anchor 失败: {result.status}");
        }
    }

    private void UpdateCubeCollider()
    {
        var boxCol = cubeObject.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.center = Vector3.zero;
            boxCol.size = Vector3.one;
        }
    }
}
