using UnityEngine;
using System.Collections.Generic;

// Handler Interface for all handler components
public interface IActionHandler
{
    IReadOnlyList<ActionSpec> GetActionSpecs();
    bool CanHandle(string actionType);
    ActionResult Execute(string actionType, string argsJson, ExecutionContext target);
}
