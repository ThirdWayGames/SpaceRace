using UnityEngine;

public class EnemyStateComponent : MonoBehaviour
{
    public State CurrentState;

    public enum State
    {
        Patrol,
        Attack,
        Guard,
        Wave
    }

}
