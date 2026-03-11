using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ActionSpec
{
    public string type; // Name amd index to locate function
    public string summary; // Short summary of a function
    public string description; // More detailed description
    public List<ArgSpec> args = new(); // Detailed information of parameters
    
    public List<string> examples = new(); // Optional, could be empty
}

[Serializable]
public class ArgSpec
{
    public string name;
    public string argType; // bool, int, float, string, enum, object
    public bool required;
    public string description;

    public ArgConstraints constraints = new();
    public string defaultValue;
}

[Serializable]
public class ArgConstraints
{
    public float? min;
    public float? max;
    public List<string> enumValues = new(); // Used when argType == "enum"
    public string unit;
}

// Will be returned when error happens, but probably not used
[Serializable]
public class ActionResult
{
    public bool success;
    public string errorCode;
    public string message;
}

public class ExecutionContext
{
    public GameObject Target { get; }

    public ExecutionContext(GameObject target)
    {
        Target = target;
    }
}