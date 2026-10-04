using System.Collections.Generic;
using UnityEngine;

public class ShipManager : Photon.MonoBehaviour 
{
    /// <summary>
    /// The power level - Float between 0 and 1 that indicates the percentage of power the ship has.
    /// </summary>
    public float PowerLevel = 1.0f;

    public bool AlarmStateActive = false;

    public List<LightController> EmergencyLighting;

    public List<LightController> Lighting;

    public List<AlarmController3D> Alarms;

    public List<EmergencyLightController3D> AlarmLighting;

    /// <summary>
    /// The instance
    /// </summary>
    public static ShipManager instance = null;

    // Use this for initialization
    public void Start ()
    {
        //Check if instance already exists
        if (instance == null)
        {
            //if not, set instance to this
            instance = this;

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
        }
        //If instance already exists and it's not this:
        else if (instance != this)
        {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the power level.
    /// </summary>
    /// <param name="powerLevel">The power level.</param>
    public void SetPowerLevel(float powerLevel)
    {
        Debug.Log(string.Format("Setting power level to {0}", powerLevel));
        instance.PowerLevel = Mathf.Clamp(powerLevel, 0f, 1f);
    }

    [PunRPC]
    public void RaiseAlarm()
    {
        AlarmStateActive = true;
    }

    [PunRPC]
    public void CancelAlarm()
    {
        AlarmStateActive = false;
    }

    // Update is called once per frame
    void Update ()
    {
        foreach (var lightController in EmergencyLighting)
        {
            if (lightController != null)
            {
                if (instance.PowerLevel < .3f)
                {
                    lightController.TurnLightOn();
                }
                else
                {
                    lightController.TurnLightOff();
                }
            }
        }

        foreach (var lightController in Lighting)
        {
            if (lightController != null)
            {
                if (instance.PowerLevel < .3f)
                {
                    lightController.TurnLightOff();
                }
                else
                {
                    lightController.TurnLightOn();
                }
            }
        }

        foreach (var emergencyLightController3D in AlarmLighting)
        {
            if (emergencyLightController3D != null)
            {
                emergencyLightController3D.SetAlertState(AlarmStateActive);
            }
        }

        foreach (var alarm in Alarms)
        {
            if (alarm != null)
            {
                alarm.SetState(AlarmStateActive);
            }
        }
    }
}
