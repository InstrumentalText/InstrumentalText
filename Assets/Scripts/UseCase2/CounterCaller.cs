using UnityEngine;

public class CounterCaller : MonoBehaviour
{
    private TextColorController textColorController;

    void Awake()
    {
        textColorController = FindAnyObjectByType<TextColorController>();
        if (textColorController == null)
            Debug.LogWarning("[CounterCaller] TextColorController not found in scene");
    }

    public void CallCounter()
    {
        if (textColorController != null)
            textColorController.Counter();
    }
}
