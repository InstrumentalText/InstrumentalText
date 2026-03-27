using UnityEngine;


public class TextObjectHandleTrigger : MonoBehaviour
{
    [Header("颜色反馈")]
    public Color highlightColor = Color.green;
    public Color defaultColor = Color.white;

    [Header("组件手动拖拽")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;      // handle 上的 XRGrabInteractable
    public Renderer rend;                            // handle 或其子物体的 Renderer
    public CurrentTextStore currentTextStore;        // handle 所属 TextObject 的 CurrentTextStore

    [Header("Debug")]
    public bool debug = true;

    private void Awake()
    {
        if (grabInteractable == null) Debug.LogError($"{gameObject.name} 缺少 XRGrabInteractable，请在 Inspector 拖入");
        if (rend == null) Debug.LogError($"{gameObject.name} 缺少 Renderer，请在 Inspector 拖入");
        if (currentTextStore == null) Debug.LogError($"{gameObject.name} 缺少 CurrentTextStore，请在 Inspector 拖入");

        if (debug)
        {
            Debug.Log($"[HandleTrigger][Awake] {gameObject.name} 初始化完成");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (!other.CompareTag("TextObject") && other.name != "handle")
        {
            if (debug) Debug.Log($"[TriggerEnter] {other.name} 不是 handle 或 TextObject → 忽略");
            return;
        }

        // 对方的 XRGrabInteractable 和 CurrentTextStore 必须手动拖拽
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable otherGrab = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        CurrentTextStore otherTextStore = other.GetComponentInParent<CurrentTextStore>();

        if (otherGrab == null || otherTextStore == null)
        {
            if (debug) Debug.Log("[TriggerEnter] 找不到对方 XRGrabInteractable 或 CurrentTextStore → 忽略");
            return;
        }

        bool selfIsMoving = grabInteractable != null && grabInteractable.isSelected;
        bool otherIsMoving = otherGrab.isSelected;

        if (debug)
        {
            Debug.Log($"[TriggerEnter] 状态检测 → self: {currentTextStore.CurrentText} (moving={selfIsMoving}) | " +
                      $"other: {otherTextStore.CurrentText} (moving={otherIsMoving})");
        }

        // 只有自己静止 + 对方在运动才触发
        if (!selfIsMoving && otherIsMoving)
        {
            Debug.Log($"[TextObject Interaction] {otherTextStore.CurrentText} → {currentTextStore.CurrentText} ✅触发成功");

            if (rend != null)
            {
                rend.material.color = highlightColor;
                if (debug) Debug.Log("[TriggerEnter] 颜色已变更（highlight）");
            }
        }
        else
        {
            if (debug)
                Debug.Log("[TriggerEnter] 条件不满足（必须：自己静止 + 对方在运动）");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        if (!other.CompareTag("TextObject") && other.name != "handle")
        {
            if (debug) Debug.Log($"[TriggerExit] {other.name} 不是 handle 或 TextObject → 忽略");
            return;
        }

        // 恢复颜色
        if (rend != null && rend.material != null)
        {
            rend.material.color = defaultColor;
            if (debug) Debug.Log("[TriggerExit] 颜色恢复（default）");
        }
    }
}