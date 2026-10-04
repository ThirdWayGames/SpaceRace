using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class RayCastActionReceiver : MonoBehaviour, IRayCastActionReceiver
    {
        public virtual void ExecuteActionBroadcast(RaycastHit hit, GameObject broadcaster)
        {
            throw new System.NotImplementedException();
        }

        public virtual void ExecuteActionBroadcast2D(RaycastHit2D hit, GameObject broadcaster)
        {
            throw new System.NotImplementedException();
        }
    }
}