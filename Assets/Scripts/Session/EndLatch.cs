public class EndLatch
{
    public bool Latched;

    public EscapePhase Phase;

    public bool TryPublish(EscapePhase phase)
    {
        if (Latched)
        {
            return false;
        }

        if (phase != EscapePhase.Escaped && phase != EscapePhase.Consumed)
        {
            return false;
        }

        Latched = true;
        Phase = phase;
        return true;
    }
}
