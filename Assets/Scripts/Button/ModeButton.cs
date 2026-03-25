using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModeButton : Button
{
    [Header("Group")]
    public ModeButtonGroup group;

    [Header("Debug")]
    public bool debug = true;

    private bool isActive = false;

    public delegate void ModeStateChanged(bool active);
    public event ModeStateChanged OnModeStateChanged;

    public void SetActive(bool active)
    {
        if (isActive == active) return;

        isActive = active;

        if (debug)
            Debug.Log($"[ModeButton] {name} 状态切换 → {(isActive ? "激活" : "关闭")}");

        if (isActive)
            base.DoStateTransition(SelectionState.Selected, false);
        else
            base.DoStateTransition(SelectionState.Normal, false);

        OnModeStateChanged?.Invoke(isActive);
    }

    public bool IsActive()
    {
        return isActive;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        if (eventData.button == PointerEventData.InputButton.Left && group != null)
        {
            if (debug) Debug.Log($"[ModeButton] Click → {name}");
            group.OnButtonClicked(this);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (isActive)
        {
            base.DoStateTransition(SelectionState.Selected, false);
        }
        else
        {
            base.DoStateTransition(SelectionState.Normal, false);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (isActive)
        {
            base.DoStateTransition(SelectionState.Selected, false);
        }
        else
        {
            base.DoStateTransition(SelectionState.Normal, false);
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (isActive)
        {
            if (state != SelectionState.Disabled)
                base.DoStateTransition(SelectionState.Selected, instant);
            else
                base.DoStateTransition(state, instant);
        }
        else
        {
            if (state != SelectionState.Disabled)
                base.DoStateTransition(SelectionState.Normal, instant);
            else
                base.DoStateTransition(state, instant);
        }
    }
}