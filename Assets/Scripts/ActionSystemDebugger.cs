using UnityEngine;
using System.Text;

[RequireComponent(typeof(Camera))]
public class ActionSystemDebugger : MonoBehaviour
{
    [SerializeField] Color emissionColor = new Color(0.15f, 0.15f, 0.15f);

    [TextArea(3, 10)]
    public string prompt = "";

    [SerializeField]
    private GameObject targetInSelection;

    private LLMProcessor llmProcessor;
    private InstrumentRegistry instrumentRegistry;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        llmProcessor = FindFirstObjectByType<LLMProcessor>();
        instrumentRegistry = FindFirstObjectByType<InstrumentRegistry>();
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
            Debug.Log($"Apply text {prompt} to object {targetInSelection}");
            llmProcessor.ProcessPrompt(targetInSelection, prompt);
        }
    }

    private void Select(GameObject obj)
    {
        if (obj == targetInSelection) return;

        Deselect();

        targetInSelection = obj;

        var objectController = obj.GetComponent<ObjectController>();
        if (objectController != null)
        {
            objectController.Select(emissionColor);
        }
        else
        {
            var canvasController = obj.GetComponent<CanvasController>();
            if (canvasController != null)
                canvasController.Select();
        }

        PrintHandlerSpecs(obj);
        PrintInstruments(obj);
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

    private void PrintInstruments(GameObject obj)
    {
        if (instrumentRegistry == null) return;

        var instruments = instrumentRegistry.GetInstrumentsByTarget(obj);
        if (instruments.Count == 0)
        {
            Debug.Log($"[ActionDebugger] {obj.name}: No instruments mounted.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[ActionDebugger] {obj.name} — {instruments.Count} instrument(s):");

        foreach (var inst in instruments)
        {
            sb.AppendLine($"  Instrument #{inst.id}: \"{inst.userIntent}\"");
            sb.AppendLine($"    Active triggers: {inst.triggerIds.Count} ({string.Join(", ", inst.triggerIds)})");
        }

        Debug.Log(sb.ToString());
    }

    private void Deselect()
    {
        if (targetInSelection == null) return;

        var objectController = targetInSelection.GetComponent<ObjectController>();
        if (objectController != null)
        {
            objectController.Deselect();
        }
        else
        {
            var canvasController = targetInSelection.GetComponent<CanvasController>();
            if (canvasController != null)
                canvasController.Deselect();
        }

        targetInSelection = null;
    }
}
