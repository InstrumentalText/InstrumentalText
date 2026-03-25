using System.Collections.Generic;
using UnityEngine;

public class ModeButtonGroup : MonoBehaviour
{
    [SerializeField] private List<ModeButton> buttons;

    private ModeButton current;

    private void Awake()
    {
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                btn.group = this;
            }
        }
    }

    public void OnButtonClicked(ModeButton btn)
    {
        if (current == btn)
        {
            btn.SetActive(false);
            current = null;
            return;
        }

        if (current != null)
        {
            current.SetActive(false);
        }

        current = btn;
        current.SetActive(true);
    }
}


