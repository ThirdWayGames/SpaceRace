using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.ScriptableObjects
{
    public class FloatTrigger
    {
        public float value;
        public GameObject triggeredBy;
    }

    [Serializable]
    public class FloatTriggerEvent : UnityEvent<FloatTrigger>
    {
    }

    [CreateAssetMenu]
    [Serializable]
    public class EventTriggerVariable : ScriptableObject
    {
        public UnityEvent<FloatTrigger> TriggerEvent = new FloatTriggerEvent();
    }
}