using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TriggerEvent : UnityEvent<GameObject>
{
    public TriggerEvent()
    {
    }
}

public class CollisionEventTrigger : MonoBehaviour
{
    public TriggerEvent OnEnterEvents;

    public TriggerEvent OnExitEvents;

    public void OnTriggerEnter(Collider collider)
    {
        if (OnEnterEvents != null)
        {
            OnEnterEvents.Invoke(this.gameObject);
        }
    }

    public void OnTriggerExit(Collider collider)
    {
        if (OnExitEvents != null)
        {
            OnExitEvents.Invoke(this.gameObject);
        }
    }
}
