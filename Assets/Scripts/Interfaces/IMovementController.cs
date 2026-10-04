namespace Assets.Scripts.Interfaces
{
    public interface IMovementController : IBaseController
    {
        /// <summary>
        /// Sets the maximum speed.
        /// </summary>
        /// <param name="maxSpeed">The maximum speed.</param>
        void SetMaxSpeed(float maxSpeed);

        void SetDisableMovement(bool disabled);
    }
}