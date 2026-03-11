using UnityEngine;
using System.Text;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(Camera))]
public class ActionSystemDebugger : MonoBehaviour
{
    public Material selectedStateMat;

    [TextArea(3, 10)]
    public string testJson = "{\"type\":\"transform.modify\",\"args\":{\"mode\":\"relative\",\"space\":\"world\",\"position\":{\"x\":0,\"y\":0.5,\"z\":0}}}";

    [SerializeField]
    private GameObject targetInSelection;
    
    private Material originalMat;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
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
            ExecuteJson(targetInSelection, testJson);
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

        PrintActionSpecs(obj);
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

    private void ExecuteJson(GameObject obj, string json)
    {
        JObject root;
        try { root = JObject.Parse(json); }
        catch (System.Exception e)
        {
            Debug.LogError($"[ActionDebugger] Invalid JSON: {e.Message}");
            return;
        }

        string actionType = root["type"]?.ToString();
        if (string.IsNullOrEmpty(actionType))
        {
            Debug.LogError("[ActionDebugger] JSON missing 'type' field.");
            return;
        }

        string argsJson = root["args"]?.ToString() ?? "{}";
        var context = new ExecutionContext(obj);
        var handlers = obj.GetComponents<IActionHandler>();

        foreach (var handler in handlers)
        {
            if (!handler.CanHandle(actionType)) continue;

            var result = handler.Execute(actionType, argsJson, context);
            if (result.success)
                Debug.Log($"[ActionDebugger] '{obj.name}' -> {actionType} executed successfully.");
            else
                Debug.LogWarning($"[ActionDebugger] '{obj.name}' -> {actionType} failed: [{result.errorCode}] {result.message}");
            return;
        }

        Debug.LogWarning($"[ActionDebugger] '{obj.name}' has no handler for '{actionType}'.");
    }

    private void PrintActionSpecs(GameObject obj)
    {
        var handlers = obj.GetComponents<IActionHandler>();
        if (handlers.Length == 0)
        {
            Debug.Log($"[ActionDebugger] '{obj.name}' has no IActionHandler.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"[ActionDebugger] '{obj.name}' — {handlers.Length} handler(s):");

        foreach (var handler in handlers)
        {
            var specs = handler.GetActionSpecs();
            sb.AppendLine($"  Handler: {handler.GetType().Name} ({specs.Count} action(s))");

            foreach (var spec in specs)
            {
                sb.AppendLine($"    [{spec.type}] {spec.summary}");
                sb.AppendLine($"      Description: {spec.description}");

                foreach (var arg in spec.args)
                {
                    string req = arg.required ? "required" : "optional";
                    string def = arg.defaultValue != null ? $", default={arg.defaultValue}" : "";
                    sb.AppendLine($"      - {arg.name} ({arg.argType}, {req}{def}): {arg.description}");
                }

                if (spec.examples.Count > 0)
                {
                    sb.AppendLine($"      Examples:");
                    foreach (var ex in spec.examples)
                        sb.AppendLine($"        {ex}");
                }
            }
        }

        Debug.Log(sb.ToString());
    }
}
