namespace Assets.Scripts.Interfaces
{
    public interface ISpawnData
    {
        /// <summary>
        /// Turns the spawn data oject into an object array that contains the serialised spawn data for transmission over the network.
        /// </summary>
        /// <returns></returns>
        object[] ToOjectArray();
    }
}