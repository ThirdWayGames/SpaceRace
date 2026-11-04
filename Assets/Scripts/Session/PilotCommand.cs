using System;
using UnityEngine;

[Serializable]
public class PilotCommand
{
    public float Throttle;

    public float Turn;

    public PilotCommand()
    {
    }

    public PilotCommand(float throttle, float turn)
    {
        Throttle = throttle;
        Turn = turn;
        Clamp();
    }

    public void Clamp()
    {
        Throttle = Mathf.Clamp(Throttle, -1f, 1f);
        Turn = Mathf.Clamp(Turn, -1f, 1f);
    }
}
