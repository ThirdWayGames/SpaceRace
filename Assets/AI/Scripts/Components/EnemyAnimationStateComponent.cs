using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationStateComponent : MonoBehaviour {

    public enum AnimationState
    {
        Idle,
        Walking,
        IdleAiming,
        WalkingAiming,
        CrouchCoverIdle,
        CrouchCoverAiming
    }

    public AnimationState AnimState;

    private EnemyActions _actions;

    public EnemyActions Animator
    {
        get
        {
            if (_actions != null)
            {
                return _actions;
            }
            _actions = GetComponent<EnemyActions>();

            return _actions;
        }
        set { _actions = value; }
    }

}
