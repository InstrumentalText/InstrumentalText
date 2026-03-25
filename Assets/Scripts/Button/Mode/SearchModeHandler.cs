using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class SearchModeHandler : MonoBehaviour
{
    [Header("Mode Reference")]
    public ModeButton targetButton;

    [Header("References")]
    public GameObject searchBar;
    public float searchBarDistance = 1f;

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

        if (isModeActive)
        {
            if (TextObjectManager.Instance != null)
            {
                TextObjectManager.Instance.ShowAll();

                if (debug)
                    Debug.Log("[SearchModeHandler] 显示 TextObject + 恢复所有连线");
            }
        }
        else
        {
            if (TextObjectManager.Instance != null)
            {
                TextObjectManager.Instance.HideAll();

                if (debug)
                    Debug.Log("[SearchModeHandler] 隐藏所有 TextObject");
            }

            if (searchBarShown && searchBar != null)
            {
                searchBar.SetActive(false);
                searchBarShown = false;
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
}