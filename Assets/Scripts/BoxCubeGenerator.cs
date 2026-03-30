using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

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

    [Header("Cube Object")]
    public GameObject cubeObject;
    public float minSize = 0.05f;

    [Header("Debug")]
    public bool debug = true;

    private bool defining = false;

    private ARAnchorManager anchorManager;

    private void Awake()
    {
        anchorManager = FindAnyObjectByType<ARAnchorManager>();
        if (anchorManager == null)
            Debug.LogWarning("[BoxCubeGenerator] 场景中没有找到 ARAnchorManager");
    }

    private void Update()
    {
        if (cubeObject == null)
            return;

        float leftValue = leftTriggerAction.action.ReadValue<float>();
        float rightValue = rightTriggerAction.action.ReadValue<float>();

        bool leftDown = leftValue > triggerDownThreshold;
        bool rightDown = rightValue > triggerDownThreshold;
        bool leftUp = leftValue < triggerUpThreshold;
        bool rightUp = rightValue < triggerUpThreshold;

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
