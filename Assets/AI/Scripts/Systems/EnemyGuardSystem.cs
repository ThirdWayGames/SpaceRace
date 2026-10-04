using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

//public class EnemyGuardSystem : ComponentSystem {

//    public struct Group
//    {
//        public EnemyGuardComponent Guard;
//        public Transform Transform;
//        public EnemyStateComponent State;
//    }

//    protected override void OnUpdate()
//    {
//        foreach (var entity in GetEntities<Group>())
//        {
//            if (entity.State.CurrentState == EnemyStateComponent.State.Guard)
//            {
//                entity.Guard.InPosition = IsInPosition(entity);

//                if (!entity.Guard.InPosition)
//                    return;

//                entity.Transform.LookAt(entity.Guard.GuardLocation.transform);
//            }
          
//        }
//    }


//    bool IsInPosition(Group entity)
//    {
//        return entity.Transform.position == entity.Guard.GuardLocation.transform.position;
//    }
//}
