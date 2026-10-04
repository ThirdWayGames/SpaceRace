
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    public RuntimeAnimatorController RedTeamController;

    public RuntimeAnimatorController BlueTeamController;

    public Animator PlayerAnimator;

    public void SetTeam(int teamId)
    {
        PlayerAnimator.runtimeAnimatorController = teamId == 1 ? BlueTeamController : RedTeamController;
    }
}
