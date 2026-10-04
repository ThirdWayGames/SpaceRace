using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts
{
    public class RayCastActionTrigger : MonoBehaviour, IRayCastActionTrigger
    {
        protected List<IRayCastActionReceiver> Receivers;

        public virtual void Awake()
        {
            Receivers = GetComponentsInParent<IRayCastActionReceiver>().ToList();
        }

        public virtual void RayCastHitAction(RaycastHit hit)
        {
            if (Receivers.Any())
            {
                Receivers.ForEach(x => x.ExecuteActionBroadcast(hit, gameObject));
            }
        }

        public virtual void RayCastHitAction2D(RaycastHit2D hit)
        {
            if (Receivers.Any())
            {
                Receivers.ForEach(x => x.ExecuteActionBroadcast2D(hit, gameObject));
            }
        }
    }
}