using UnityEngine;
using TMPro;

public class CurrentTextStore : MonoBehaviour
{
    [Header("Input Field")]
    public TMP_InputField inputField;

    [Header("Current Text")]
    [TextArea]
    public string CurrentText;

    void Start()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnTextChanged);
        }
    }

    void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnTextChanged);
        }
    }

    private void OnTextChanged(string newValue)
    {
        CurrentText = newValue;
    }
}