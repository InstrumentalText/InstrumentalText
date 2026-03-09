using UnityEngine;
using UnityEngine.InputSystem;

public class PinchHoldSpawnText_XRI : MonoBehaviour
{
    [Header("Pinch Input")]
    public InputActionProperty pinchAction;

    [Header("Pinch Threshold")]
    public float pinchDownThreshold = 0.8f;
    public float pinchUpThreshold = 0.2f;

    [Header("Hold Time")]
    public float pinchHoldTime = 1.0f;

    [Header("Spawn")]
    public GameObject textPrefab;
    public float spawnDistance = 0.5f;

    [Header("Check Existing Text Objects")]
    public string textObjectTag = "TextObject"; //Tag: TextObject

    float pinchTimer = 0f;
    bool pinchPressed = false;
    bool spawnedThisPinch = false;

    void Update()
    {
        float pinchValue = pinchAction.action.ReadValue<float>();

        // Pinch start
        if (!pinchPressed && pinchValue > pinchDownThreshold)
        {
            pinchPressed = true;
            pinchTimer = 0f;
        }

        // Pinch release
        if (pinchPressed && pinchValue < pinchUpThreshold)
        {
            pinchPressed = false;
            pinchTimer = 0f;
            spawnedThisPinch = false;
        }

        // Pinch holding
        if (pinchPressed)
        {
            pinchTimer += Time.deltaTime;

            if (pinchTimer >= pinchHoldTime && !spawnedThisPinch)
            {
                SpawnText();
                spawnedThisPinch = true;
            }
        }
    }

    void SpawnText()
    {
        if (textPrefab == null) return;

        //当前设置，场景中只允许一个TextObject存在
        if (GameObject.FindGameObjectWithTag(textObjectTag) != null)
        {
            Debug.Log("Text Object already exists. Pinch detected but no spawn.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
        Quaternion rot = Quaternion.LookRotation(spawnPos - cam.transform.position);

        GameObject newText = Instantiate(textPrefab, spawnPos, rot);

        //生成的TextObject加上Tag
        newText.tag = textObjectTag;

        Debug.Log("Text object spawned");
    }
}