using System;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IRayCastActionTrigger
    {
        void RayCastHitAction(RaycastHit hit);

        void RayCastHitAction2D(RaycastHit2D hit);
    }
}