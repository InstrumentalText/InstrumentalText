using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SearchModeHandler : MonoBehaviour
{
    [Header("Mode Reference")]
    public ModeButton targetButton;

    [Header("References")]
    public GameObject searchBar;
    public float searchBarDistance = 1f;

    [Header("SearchBar Logic")]
    public SearchBarHandler searchBarHandler; 
    public CurrentTextStore currentTextStore; 

    [Header("Pinch Settings")]
    public InputActionProperty pinchAction;
    public float pinchDownThreshold = 0.8f;

    [Header("Debug")]
    public bool debug = true;

    private bool isModeActive = false;
    private bool searchBarShown = false;

    public bool IsSearchActive()
    {
        return isModeActive;
    }

    private void Awake()
    {
        if (targetButton == null)
        {
            Debug.LogError("[SearchModeHandler] 请拖入 ModeButton！");
            return;
        }

        if (searchBar == null)
        {
            Debug.LogError("[SearchModeHandler] 请拖入 SearchBar！");
        }

        targetButton.OnModeStateChanged += HandleModeChanged;
    }

    private void OnDestroy()
    {
        if (targetButton != null)
            targetButton.OnModeStateChanged -= HandleModeChanged;
    }

    private void HandleModeChanged(bool active)
    {
        isModeActive = active;

        if (debug)
            Debug.Log($"[SearchModeHandler] Mode 状态 → {(active ? "激活" : "关闭")}");

        if (TextObjectManager.Instance != null)
        {
            if (active)
            {
                TextObjectManager.Instance.ShowAll();

                // 搜索模式 → 启用 DotFollower
                SetDotFollowersActive(true);

                if (debug)
                    Debug.Log("[SearchModeHandler] 显示 TextObject + 启用 DotFollower");
            }
            else
            {
                TextObjectManager.Instance.HideAll();

                // 退出搜索模式 → 禁用 DotFollower
                SetDotFollowersActive(false);

                ClearSearchInput();

                if (searchBarHandler != null)
                {
                    searchBarHandler.ClearAllHighlights();
                    if (debug)
                        Debug.Log("[SearchModeHandler] 清除所有高亮");
                }

                if (searchBarShown && searchBar != null)
                {
                    searchBar.SetActive(false);
                    searchBarShown = false;
                }
            }
        }
    }

    private void SetDotFollowersActive(bool active)
    {
        if (TextObjectManager.Instance == null) return;

        foreach (var textObj in TextObjectManager.Instance.GetAllTextObjects())
        {
            if (textObj == null) continue;

            // 获取 TextObject 下所有 DotFollower 并激活/禁用
            var dotFollowers = textObj.GetComponentsInChildren<DotFollower>(true);
            foreach (var df in dotFollowers)
            {
                if (df != null)
                    df.enabled = active;
            }
        }
    }

    private void Update()
    {
        if (!isModeActive || pinchAction == null)
            return;

        if (!searchBarShown)
        {
            float pinchValue = pinchAction.action.ReadValue<float>();

            if (pinchValue >= pinchDownThreshold)
            {
                ShowSearchBar();
                searchBarShown = true;

                if (debug)
                    Debug.Log("[SearchModeHandler] Pinch → 弹出 SearchBar");
            }
        }
    }

    void ShowSearchBar()
    {
        if (searchBar == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        searchBar.transform.position =
            cam.transform.position + cam.transform.forward * searchBarDistance;

        searchBar.transform.rotation =
            Quaternion.LookRotation(cam.transform.forward);

        searchBar.SetActive(true);
    }

    void ClearSearchInput()
    {
        if (currentTextStore != null)
        {
            currentTextStore.CurrentText = "";
        }

        if (searchBar != null)
        {
            Transform inputFieldTransform = searchBar.transform.Find("canvas/InputField (TMP)");
            if (inputFieldTransform != null)
            {
                TMP_InputField inputField = inputFieldTransform.GetComponent<TMP_InputField>();
                if (inputField != null)
                {
                    inputField.text = "";
                }
            }
        }

        if (debug)
            Debug.Log("[SearchModeHandler] 清空 SearchBar 输入内容");
    }
}