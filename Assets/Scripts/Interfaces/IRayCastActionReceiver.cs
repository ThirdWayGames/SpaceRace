using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IRayCastActionReceiver
    {
        void ExecuteActionBroadcast(RaycastHit hit, GameObject broadcaster);

        void ExecuteActionBroadcast2D(RaycastHit2D hit, GameObject broadcaster);
    }
}