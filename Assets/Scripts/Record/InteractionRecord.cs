using System;
using UnityEngine;

[Serializable]
public class InteractionRecord
{
    public string prompt;
    public string targetName;
    public float time;

    public string sceneName;
    public Vector3 targetPosition;

    public InteractionRecord(string prompt, string targetName, GameObject target = null)
    {
        this.prompt = prompt;
        this.targetName = targetName;
        this.time = Time.time;

        if (target != null)
        {
            this.targetPosition = target.transform.position;
        }

        this.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}