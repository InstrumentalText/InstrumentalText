using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using TMPro;

public class MusicPlayerHandler : MonoBehaviour, IActionHandler
{
    [SerializeField] TMP_Text m_infoLabel;
    [SerializeField] AudioSource m_audioSource;
    [SerializeField] AudioClip m_calmClip;
    [SerializeField] AudioClip m_energiseClip;

    private static readonly List<ActionSpec> actionSpecs = new()
    {
        new ActionSpec
        {
            type = "switch.on",
            summary = "Turn on the music player",
            description = "Start playing calm background music and display the current track name.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"switch.on\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "switch.off",
            summary = "Turn off the music player",
            description = "Stop playing music and clear the track display.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"switch.off\",\"args\":{}}"
            }
        },
        new ActionSpec
        {
            type = "music.energise",
            summary = "Switch to energetic music",
            description = "Replace the current track with a more energetic song.",
            args = new List<ArgSpec>(),
            examples = new List<string>
            {
                "{\"type\":\"music.energise\",\"args\":{}}"
            }
        }
    };

    public IReadOnlyList<ActionSpec> GetActionSpecs()
    {
        return actionSpecs;
    }

    public bool CanHandle(string actionType)
    {
        return actionType == "switch.on" || actionType == "switch.off" || actionType == "music.energise";
    }

    public ActionResult Execute(string actionType, string argsJson, ExecutionContext target)
    {
        var spec = actionSpecs.Find(s => s.type == actionType);
        if (spec == null) return new ActionResult{success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}"};

        if(target == null || target.Target == null) return new ActionResult{success = false, errorCode = "INVALID_TARGET", message = $"Target is null"};

        JObject argsObj;
        try { argsObj = JObject.Parse(argsJson ?? "{}");}
        catch (Exception e) {return new ActionResult{success = false, errorCode = "INVALID_JSON", message = e.Message};}

        switch(spec.type)
        {
            case "switch.on":
                return HandleSwitchOn(spec, argsObj, target);
            case "switch.off":
                return HandleSwitchOff(spec, argsObj, target);
            case "music.energise":
                return HandleEnergise(spec, argsObj, target);
            default:
                return new ActionResult{success = false, errorCode = "UNKNOWN_ACTION", message = $"Unsupported action: {actionType}"};
        }
    }

    private ActionResult HandleSwitchOn(ActionSpec spec, JObject argsObj, ExecutionContext target)
    {
        if (m_audioSource == null)
            return new ActionResult{success = false, errorCode = "NO_AUDIO_SOURCE", message = "AudioSource is not assigned."};

        m_audioSource.clip = m_calmClip;
        m_audioSource.loop = true;
        m_audioSource.Play();

        UpdateLabel(m_calmClip);

        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private ActionResult HandleSwitchOff(ActionSpec spec, JObject argsObj, ExecutionContext target)
    {
        if (m_audioSource == null)
            return new ActionResult{success = false, errorCode = "NO_AUDIO_SOURCE", message = "AudioSource is not assigned."};

        m_audioSource.Stop();
        m_audioSource.clip = null;

        if (m_infoLabel != null)
            m_infoLabel.text = "";

        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private ActionResult HandleEnergise(ActionSpec spec, JObject argsObj, ExecutionContext target)
    {
        if (m_audioSource == null)
            return new ActionResult{success = false, errorCode = "NO_AUDIO_SOURCE", message = "AudioSource is not assigned."};

        if (!m_audioSource.isPlaying)
            return new ActionResult{success = false, errorCode = "NOT_PLAYING", message = "Music player is not on. Use switch.on first."};

        m_audioSource.clip = m_energiseClip;
        m_audioSource.Play();

        UpdateLabel(m_energiseClip);

        return new ActionResult{success = true, errorCode = "", message = ""};
    }

    private void UpdateLabel(AudioClip clip)
    {
        if (m_infoLabel != null)
        {
            m_infoLabel.text = "Playing music: ";
            m_infoLabel.text += clip != null ? clip.name : "";
        }
    }
}
