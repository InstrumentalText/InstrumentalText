using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextNotificationHandler : MonoBehaviour
{
    [Header("Dot")]
    public Transform dot;

    [Header("UI")]
    public GameObject textNotificationPrefab;

    [Header("Layout")]
    public Vector3 firstOffset = new Vector3(0.1f, 0f, 0f);
    public Vector3 stackOffset = new Vector3(0f, 0.05f, 0f);

    [Header("Apply Animation")]
    public float scaleFactor = 1.2f;
    public int bounceTimes = 2;
    public float bounceDuration = 0.15f;
    public float lifeTime = 1.0f;

    private List<GameObject> spawnedTexts = new List<GameObject>();


    public void RefreshNotifications()
    {
        var applier = GazePinchPromptApplierOnDevice_Case2.ActiveInstance;
        if (applier == null)
        {
            Debug.Log("[Notification] No active Applier → cannot show Apply notification");
            return;
        }

  
        if (!applier.IsApplyMode())
        {
            Debug.Log("[Notification] Not in Apply Mode → skipping Apply notification");
            return;
        }

        Debug.Log("[Notification] Apply Mode → Showing latest notification");

        if (dot == null)
        {
            Debug.LogWarning("[Notification] Dot 未绑定！");
            return;
        }

        if (!dot.gameObject.activeSelf)
            dot.gameObject.SetActive(true);

        var textObjs = TextObjectManager.Instance.GetTextObjectsByTarget(gameObject);
        if (textObjs.Count == 0) return;

        GameObject latestTextObj = textObjs[textObjs.Count - 1];
        var store = latestTextObj.GetComponent<CurrentTextStore>();
        if (store == null || string.IsNullOrEmpty(store.CurrentText))
        {
            Debug.Log("[Notification] Latest TextObject has no CurrentText");
            return;
        }

        GameObject ui = Instantiate(textNotificationPrefab);
        ui.transform.position = GetWorldPositionFromDot(firstOffset);
        FaceCamera(ui);

        TMP_Text t = ui.GetComponentInChildren<TMP_Text>();
        if (t != null)
            t.text = store.CurrentText;

        StartCoroutine(PlayApplyAnimationAndDestroy(ui));
    }



    public void ShowAllNotifications()
    {
        var applier = GazePinchPromptApplierOnDevice_Case2.ActiveInstance;
        if (applier != null && applier.IsApplyMode())
        {
            Debug.Log("[Notification] Hover ignored → currently in Apply Mode");
            return;
        }

        Debug.Log("[Notification] Hover Mode → Showing all notifications");

        ClearAll();

        if (dot == null)
        {
            Debug.LogWarning("[Notification] Dot 未绑定！");
            return;
        }

        var textObjs = TextObjectManager.Instance.GetTextObjectsByTarget(gameObject);

        List<string> validTexts = new List<string>();

        foreach (var textObj in textObjs)
        {
            if (textObj == null) continue;

            var store = textObj.GetComponent<CurrentTextStore>();
            if (store == null || string.IsNullOrEmpty(store.CurrentText)) continue;

            validTexts.Add(store.CurrentText);
        }

        if (validTexts.Count == 0)
        {
            Debug.Log("[Notification] No valid text → Dot hidden");
            if (dot.gameObject.activeSelf)
                dot.gameObject.SetActive(false);
            return;
        }

        if (!dot.gameObject.activeSelf)
            dot.gameObject.SetActive(true);

        Vector3 currentOffset = firstOffset;

        foreach (var text in validTexts)
        {
            GameObject ui = Instantiate(textNotificationPrefab);
            ui.transform.position = GetWorldPositionFromDot(currentOffset);
            FaceCamera(ui);

            TMP_Text t = ui.GetComponentInChildren<TMP_Text>();
            if (t != null)
                t.text = text;

            spawnedTexts.Add(ui);

            currentOffset += stackOffset;
        }
    }
    public void HideAllNotifications()
    {
        Debug.Log("[Notification] HideAllNotifications called");
        ClearAll();
    }


    Vector3 GetWorldPositionFromDot(Vector3 offset)
    {
        Transform cam = Camera.main.transform;
        return dot.position
            + cam.right * offset.x
            + cam.up * offset.y
            + cam.forward * offset.z;
    }

    void FaceCamera(GameObject ui)
    {
        ui.transform.rotation = Quaternion.LookRotation(
            ui.transform.position - Camera.main.transform.position
        );
    }

    void ClearAll()
    {
        foreach (var t in spawnedTexts)
        {
            if (t != null) Destroy(t);
        }
        spawnedTexts.Clear();
    }

    IEnumerator PlayApplyAnimationAndDestroy(GameObject ui)
    {
        Vector3 originalScale = ui.transform.localScale;

        for (int i = 0; i < bounceTimes; i++)
        {
            float timer = 0f;
            while (timer < bounceDuration)
            {
                ui.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, timer / bounceDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
            while (timer < bounceDuration)
            {
                ui.transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, timer / bounceDuration);
                timer += Time.deltaTime;
                yield return null;
            }
        }

        ui.transform.localScale = originalScale;

        yield return new WaitForSeconds(lifeTime);

        Destroy(ui);
        Debug.Log("[Notification] Apply notification destroyed → Hover can now show");
    }
}