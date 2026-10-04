namespace Assets.Scripts.Interfaces
{
    public interface IBullet
    {
        /// <summary>
        /// Applies force to the bullet
        /// </summary>
        /// <param name="bulletVelocity">Velocity to apply</param>
        /// <param name="bulletLifetime">The lifetime of the bullet</param>
        void ApplyForce(ISpawnData bulletData);

        /// <summary>
        /// Used to identify the team id of the person who shot the bullet.
        /// </summary>
        /// <param name="teamId"></param>
        void SetShooterTeamId(int teamId);

        /// <summary>
        /// Gets the current energy consumption from the bullet being fired.
        /// </summary>
        /// <returns>The amount of energy to consume.</returns>
        float GetEnergyConsumption();

        /// <summary>
        /// Used to set the spawn data for local spawns.
        /// </summary>
        /// <param name=""></param>
        void SetSpawnData(ISpawnData spawnData);
    }
}