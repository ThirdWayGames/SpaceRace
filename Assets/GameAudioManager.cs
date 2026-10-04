using Assets.Scripts.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudioManager : MonoBehaviour
{
    public List<EventTriggerVariable> TriggerEvents;

    public AudioClip DefaultAudioClip;

    private AudioSource AudioSource;

    private void Awake()
    {
        AudioSource = GetComponent<AudioSource>();

        if (TriggerEvents.Any())
        {
            foreach (var trigger in TriggerEvents)
            {
                trigger.TriggerEvent.AddListener(ListenerTriggered);
            }
        }
    }

    public void ListenerTriggered(FloatTrigger triggerResult)
    {
        if (triggerResult.value > 0)
        {
            var accessConsoleScript = triggerResult.triggeredBy.GetComponent<AccessConsole3D>();

            if (accessConsoleScript != null)
            {
                if (accessConsoleScript.AlertClips.Any())
                {
                    StartCoroutine(PlayConsecutiveSounds(accessConsoleScript.AlertClips));
                }
                else
                {
                    PlayDefaultAudioClip();
                }
            }
            else
            {
                PlayDefaultAudioClip();
            }
        }
    }

    protected IEnumerator PlayConsecutiveSounds(List<AudioClip> clips)
    {
        foreach (var clip in clips)
        {
            AudioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    protected void PlayDefaultAudioClip()
    {
        if (DefaultAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(DefaultAudioClip, Vector3.zero);
        }
        else
        {
            Debug.LogWarning("Attempted to play the default sound on the GameAudioManger but it was null");
        }
    }
}
