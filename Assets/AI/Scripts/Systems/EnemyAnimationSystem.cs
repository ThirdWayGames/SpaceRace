using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class EnemyAnimationSystem : ComponentSystem
{

    public struct Group
    {
        public EnemyAnimationStateComponent AnimationState;
        public EnemyMovementComponent Movement;
        public EnemyTargetComponent Target;
        public EnemyStateComponent State;
        public Transform transform;
    }

    protected override void OnUpdate()
    {
        foreach (var entity in GetEntities<Group>())
        {
            // Updating state
            //entity.AnimationState.AnimState = UpdateState(entity);
            ////entity.AnimationState.Animator.speed = entity.Movement.Agent.desiredVelocity.magnitude;
            //// Setting animation
            //SettingAnimation(entity.AnimationState);

            entity.transform.gameObject.GetComponent<Animator>();

            if (entity.Movement.IsMoving)
            {
                entity.transform.gameObject.GetComponent<Animator>().SetInteger("State", 1);
            }
            else
            {
                entity.transform.gameObject.GetComponent<Animator>().SetInteger("State", 0);
            }


        }
    }

    void SettingAnimation(EnemyAnimationStateComponent state)
    {

        if (state.AnimState == EnemyAnimationStateComponent.AnimationState.IdleAiming)
        {
            state.Animator.Aiming();
        }

        else if (state.AnimState == EnemyAnimationStateComponent.AnimationState.WalkingAiming)
        {
            state.Animator.Aiming();
        }
        else if (state.AnimState == EnemyAnimationStateComponent.AnimationState.CrouchCoverAiming)
        {
            state.Animator.CoverAiming();
        }


    }


    EnemyAnimationStateComponent.AnimationState UpdateState(Group entity)
    {
        if (entity.State.CurrentState == EnemyStateComponent.State.Patrol)
        {
            // Going to waypoints
            if (entity.Movement.IsMoving)
            {
                entity.AnimationState.AnimState = EnemyAnimationStateComponent.AnimationState.Walking;
                //Debug.Log("Walking not aiming");
            }
            else
            {
                entity.AnimationState.AnimState = EnemyAnimationStateComponent.AnimationState.Idle;
                // Debug.Log("Idle");
            }
        }
        else if (entity.State.CurrentState == EnemyStateComponent.State.Attack)
        {
            // Going to target
            if (entity.Movement.IsMoving)
            {
                entity.AnimationState.AnimState = EnemyAnimationStateComponent.AnimationState.WalkingAiming;
                //Debug.Log("Walking aiming");
            }
            else
            {
                if (!entity.Movement.InCover)
                {
                    entity.AnimationState.AnimState = EnemyAnimationStateComponent.AnimationState.IdleAiming;
                }
                else
                {
                    entity.AnimationState.AnimState = EnemyAnimationStateComponent.AnimationState.CrouchCoverAiming;
                }
                //Debug.Log("Idle aiming");
            }
        }
        else if (entity.State.CurrentState == EnemyStateComponent.State.Guard)
        {
            // Going to target
            if (entity.Movement.IsMoving)
            {
                entity.AnimationState.AnimState = EnemyAnimationStateComponent.AnimationState.Walking;
                //Debug.Log("Walking aiming");
            }
            else
            {
                entity.AnimationState.AnimState = EnemyAnimationStateComponent.AnimationState.Idle;
                // Debug.Log("Idle aiming");
            }
        }

        return entity.AnimationState.AnimState;
    }
}
