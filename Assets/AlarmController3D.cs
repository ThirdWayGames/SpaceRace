using System;
using System.Collections;
using UnityEngine;

public class AlarmController3D : Photon.MonoBehaviour
{
    protected AudioSource Audio;

    protected bool CurrentAlarmState = false;

    protected bool crRunning = false;

    public float AlarmDelay = 10f;

	// Use this for initialization
	void Start ()
	{
	    Audio = GetComponentInChildren<AudioSource>();
	    if (Audio == null)
	    {
            throw new Exception("No audio source found in child obejcts.");
	    }
	}

    public void Update()
    {
        // If we are in an alarm state and the CR isn't running.
        if (CurrentAlarmState && !crRunning)
        {
            // Start it.
            StartCoroutine("SoundAlarm");
        }

        // If we are not in an alarm state and the CR is running.
        if (!CurrentAlarmState && crRunning)
        {
            // Stop it.
            StopCoroutine("SoundAlarm");
            crRunning = false;
        }
    }

    public IEnumerator SoundAlarm()
    {
        crRunning = true;
        if (!Audio.isPlaying)
        {
            Audio.PlayOneShot(Audio.clip);
        }

        yield return new WaitForSeconds(AlarmDelay);
        crRunning = false;
    }

    public void SetState(bool alarmState)
    {
        CurrentAlarmState = alarmState;
    }
}
