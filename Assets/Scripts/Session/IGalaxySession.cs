public interface IGalaxySession
{
    GalaxySnapshot Snapshot { get; }

    void SubmitPilotCommand(float throttle, float turn);

    bool TryReveal(float arcDegrees, float range);

    void DamageContact(string id, float amount);
}
