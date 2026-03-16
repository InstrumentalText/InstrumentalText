using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(Camera))]
public class ActionSystemDebugger : MonoBehaviour
{
    [SerializeField] Color emissionColor = new Color(0.15f, 0.15f, 0.15f);

    [TextArea(3, 10)]
    public string prompt = "";

    [SerializeField]
    private GameObject targetInSelection;

    private LLMProcessor llmProcessor;
    private Camera cam;
    private readonly List<Material> modifiedMats = new();

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

        modifiedMats.Clear();
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat.IsKeywordEnabled("_EMISSION")) continue;

                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor);
                modifiedMats.Add(mat);
            }
        }

        PrintHandlerSpecs(obj);
    }

    private void PrintHandlerSpecs(GameObject obj)
    {
        var handlers = obj.GetComponents<IActionHandler>();
        if (handlers.Length == 0)
        {
            Debug.Log($"[ActionDebugger] {obj.name}: No handlers found.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[ActionDebugger] {obj.name} — {handlers.Length} handler(s):");

        foreach (var handler in handlers)
        {
            sb.AppendLine($"  Handler: {handler.GetType().Name}");
            foreach (var spec in handler.GetActionSpecs())
            {
                sb.AppendLine($"    [{spec.type}] {spec.summary}");
                sb.AppendLine($"      {spec.description}");
                foreach (var arg in spec.args)
                {
                    string req = arg.required ? "required" : "optional";
                    string def = arg.defaultValue != null ? $", default={arg.defaultValue}" : "";
                    sb.AppendLine($"      - {arg.name} ({arg.argType}, {req}{def}): {arg.description}");
                }
            }
        }

        Debug.Log(sb.ToString());
    }

    private void Deselect()
    {
        if (targetInSelection == null) return;

        foreach (var mat in modifiedMats)
        {
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
        }
        modifiedMats.Clear();

        targetInSelection = null;
    }
}
