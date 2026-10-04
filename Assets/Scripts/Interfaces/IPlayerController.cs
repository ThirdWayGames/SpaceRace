using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IPlayerController : IBaseController
    {
        /// <summary>
        /// Sets the maximum running speed.
        /// </summary>
        /// <param name="maxRunningSpeeed">The maximum running speeed.</param>
        void SetMaxRunningSpeed(float maxRunningSpeeed);

        /// <summary>
        /// Initialises the client spawn.
        /// </summary>
        void InitialiseClientSpawn();

        void SetDisableActions(bool disabled);

        void SetDisableMovement(bool disabled);

        bool IsActionsDisabled();

        int GetTeamId();

        void SetPathogenLoadout(ScriptableObject pathogenLoadout);
    }
}