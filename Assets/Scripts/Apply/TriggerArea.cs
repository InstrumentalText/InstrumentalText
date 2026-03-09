// using UnityEngine;
// using UnityEngine.InputSystem;
// using TMPro;

// public class TriggerArea : MonoBehaviour
// {
//     [Header("颜色设置")]
//     public Color enterColor = Color.red;    
//     public Color exitColor = Color.white;   

//     [Header("Prompt")]
//     public string prompt;                   
//     public TMP_Text displayText;            

//     [Header("Pinch Input")]
//     public InputActionProperty pinchAction;
//     public float pinchDownThreshold = 0.8f;
//     public float pinchUpThreshold = 0.2f;

//     private bool isHandleInside = false;
//     private Collider currentHandle;

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.name == "handle")
//         {
//             Debug.Log("Handle 进入碰撞区域");

//             // 改变颜色
//             Renderer rend = other.GetComponent<Renderer>();
//             if (rend != null)
//             {
//                 rend.material.color = enterColor;
//             }

//             isHandleInside = true;
//             currentHandle = other;
//         }
//     }

//     private void OnTriggerStay(Collider other)
//     {
//         if (other.name == "handle")
//         {
//             // 这里保持停留逻辑
//             // Debug输出可以选择注释掉，避免每帧打印
//             // Debug.Log("Handle 停留在碰撞区域");

//             // 检测 pinch 是否释放
//             float pinchValue = pinchAction.action.ReadValue<float>();
//             if (pinchValue < pinchUpThreshold)
//             {
//                 // pinch 已释放，处理逻辑
//                 CurrentTextStore textStore = other.transform.parent.GetComponent<CurrentTextStore>();
//                 if (textStore != null)
//                 {
//                     prompt = textStore.CurrentText;
//                     Debug.Log("TriggerArea 的 prompt 获取为: " + prompt);

//                     // 显示 prompt
//                     if (displayText != null)
//                     {
//                         displayText.text = "Received prompt: "+prompt;
//                     }
//                 }

//                 // 删除 handle 整个物体（包括父物体）
//                 Destroy(other.transform.parent.gameObject);

//                 // 清除状态
//                 isHandleInside = false;
//                 currentHandle = null;
//             }
//         }
//     }

//     private void OnTriggerExit(Collider other)
//     {
//         if (other.name == "handle")
//         {
//             Debug.Log("Handle 离开碰撞区域");

//             // 恢复颜色
//             Renderer rend = other.GetComponent<Renderer>();
//             if (rend != null)
//             {
//                 rend.material.color = exitColor;
//             }

//             // 清空 prompt 显示
//             prompt = string.Empty;
//             if (displayText != null)
//             {
//                 displayText.text = "";
//             }

//             isHandleInside = false;
//             currentHandle = null;
//         }
//     }
// }



using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TriggerArea : MonoBehaviour
{
    [Header("颜色设置")]
    public Color enterColor = Color.red;    
    public Color exitColor = Color.white;   

    [Header("Prompt")]
    public string prompt;                   
    public TMP_Text displayText;            

    [Header("Pinch Input")]
    public InputActionProperty pinchAction;
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;

    private bool hasProcessed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "handle")
        {
            Debug.Log("Handle 进入碰撞区域");

            // handle 变色
            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = enterColor;
            }

            // 新 handle 出现 → 隐藏旧 prompt
            if (displayText != null)
            {
                displayText.text = "";
            }

            hasProcessed = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == "handle" && !hasProcessed)
        {
            float pinchValue = pinchAction.action.ReadValue<float>();

            // 检测 pinch release
            if (pinchValue < pinchUpThreshold)
            {
                hasProcessed = true;

                CurrentTextStore textStore = other.transform.parent.GetComponent<CurrentTextStore>();
                if (textStore != null)
                {
                    prompt = textStore.CurrentText;

                    Debug.Log("TriggerArea 获取 prompt: " + prompt);

                    if (displayText != null)
                    {
                        displayText.text = "Received prompt: " + prompt;
                    }
                }

                // 删除 handle 整个父物体
                Destroy(other.transform.parent.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "handle")
        {
            Debug.Log("Handle 离开碰撞区域");

            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = exitColor;
            }

            hasProcessed = false;
        }
    }
}