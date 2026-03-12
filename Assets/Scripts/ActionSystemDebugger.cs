using UnityEngine;
using System.Text;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(Camera))]
public class ActionSystemDebugger : MonoBehaviour
{
    public Material selectedStateMat;

    [TextArea(3, 10)]
    public string prompt = "";

    [SerializeField]
    private GameObject targetInSelection;
    
    private LLMProcessor llmProcessor;
    private Material originalMat;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        llmProcessor = FindFirstObjectByType<LLMProcessor>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                Select(hit.collider.gameObject);
            else
                Deselect();
        }

        if (Input.GetKeyDown(KeyCode.Return) && targetInSelection != null)
        {
            llmProcessor.ProcessPrompt(targetInSelection, prompt);
        }
    }

    private void Select(GameObject obj)
    {
        if (obj == targetInSelection) return;

        Deselect();

        targetInSelection = obj;

        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMat = renderer.material;
            renderer.material = selectedStateMat;
        }
    }

    private void Deselect()
    {
        if (targetInSelection == null) return;

        var renderer = targetInSelection.GetComponent<Renderer>();
        if (renderer != null && originalMat != null)
        {
            renderer.material = originalMat;
        }

        targetInSelection = null;
        originalMat = null;
    }
}
