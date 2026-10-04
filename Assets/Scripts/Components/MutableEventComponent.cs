using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.Components
{
    [RequireComponent(typeof(GameObjectEntity))]
    public abstract class MutableEventComponent : MonoBehaviour
    {
        protected float PrevMutatableRunningValue;

        public MonoBehaviour MutatableComponentToObserve;

        public virtual void Start()
        {
            PrevMutatableRunningValue = GetObservedValue();
        }

        public void Update()
        {
            if (PrevMutatableRunningValue != GetObservedValue())
            {
                PrevMutatableRunningValue = GetObservedValue();
                TriggerEvent();
            }
        }

        protected float GetObservedValue()
        {
            float result = 0f;
            if (MutatableComponentToObserve != null)
            {
                MutatableComponent mutatableComponent = MutatableComponentToObserve as MutatableComponent;
                if (mutatableComponent != null)
                {
                    result = mutatableComponent.CurrentValue;
                }
            }

            return result;
        }

        protected abstract void TriggerEvent();
    }
}