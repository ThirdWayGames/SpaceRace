using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IConsole
    {
        bool IsActive();

        bool IsLocked();

        int GetFailureCount();

        void Activate(IPlayerController player);

        void Deactivate(IPlayerController player);

        IPlayerController GetActivePlayer();

        /// <summary>
        /// Sends a result to the currently active console.
        /// </summary>
        /// <param name="passMark">The result of the console</param>
        /// <param name="deactivateOverride">If true then the SetConsoleResult will override the AccessConsoles DeactivateAfterResult property.</param>
        void SetConsoleResult(float passMark, bool deactivateOverride = false);

        PhotonView GetPhotonView();

        string GetAssociatedScene();

        ScriptableObject GetDifficultySettings();
    }
}