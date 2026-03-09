using UnityEngine;

public class GazeHighlighter : MonoBehaviour
{
    public float gazeThreshold = 0.3f; // 注视阈值时间
    public Color highlightColor = Color.yellow;

    private Transform currentGazedObject;
    private float gazeTimer = 0f;
    private Outline currentOutline;

    public void UpdateGaze(Transform gazedObject)
    {
        if (gazedObject == currentGazedObject)
        {
            // 持续注视同一物体
            gazeTimer += Time.deltaTime;

            if (gazeTimer >= gazeThreshold && currentOutline == null)
            {
                AddOutline(gazedObject);
            }
        }
        else
        {
            // 注视切换或离开
            RemoveOutline();
            currentGazedObject = gazedObject;
            gazeTimer = 0f;
        }
    }

    void AddOutline(Transform obj)
    {
        currentOutline = obj.gameObject.AddComponent<Outline>();
        currentOutline.OutlineMode = Outline.Mode.OutlineAll;
        currentOutline.OutlineColor = highlightColor;
        currentOutline.OutlineWidth = 5f;
    }

    void RemoveOutline()
    {
        if (currentOutline != null)
        {
            Destroy(currentOutline);
            currentOutline = null;
        }
    }
}
