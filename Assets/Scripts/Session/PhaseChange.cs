public class PhaseChange
{
    public bool Accepted;

    public EscapePhase Phase;

    public PhaseChange(bool accepted, EscapePhase phase)
    {
        Accepted = accepted;
        Phase = phase;
    }
}
