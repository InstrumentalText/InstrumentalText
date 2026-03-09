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

            // handle变色
            Renderer rend = other.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = enterColor;
            }


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