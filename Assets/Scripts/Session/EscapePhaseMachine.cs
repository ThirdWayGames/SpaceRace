public class EscapePhaseMachine
{
    public EscapePhase Current;

    public EscapePhaseMachine()
    {
        Current = EscapePhase.Docked;
    }

    public bool IsTerminal()
    {
        return Current == EscapePhase.Escaped || Current == EscapePhase.Consumed;
    }

    public PhaseChange TryDeploy()
    {
        if (IsTerminal())
        {
            return Reject();
        }

        if (Current == EscapePhase.Docked || Current == EscapePhase.MissionActive)
        {
            Current = EscapePhase.MissionActive;
            return Accept();
        }

        return Reject();
    }

    public PhaseChange TryRecall(bool anyoneStillDeployed)
    {
        if (IsTerminal() || Current != EscapePhase.MissionActive)
        {
            return Reject();
        }

        if (!anyoneStillDeployed)
        {
            Current = EscapePhase.Docked;
        }

        return Accept();
    }

    public PhaseChange TryUndock(bool allAboard)
    {
        if (IsTerminal() || Current != EscapePhase.Docked)
        {
            return Reject();
        }

        if (!allAboard)
        {
            return Reject();
        }

        Current = EscapePhase.Transiting;
        return Accept();
    }

    public PhaseChange NotifyArrived()
    {
        if (IsTerminal() || Current != EscapePhase.Transiting)
        {
            return Reject();
        }

        Current = EscapePhase.Docked;
        return Accept();
    }

    public PhaseChange TryEscape(bool allAboard, bool hasEquipment)
    {
        if (IsTerminal() || Current != EscapePhase.Docked)
        {
            return Reject();
        }

        if (!allAboard || !hasEquipment)
        {
            return Reject();
        }

        Current = EscapePhase.Escaped;
        return Accept();
    }

    public PhaseChange NotifyConsumed()
    {
        if (IsTerminal())
        {
            return Reject();
        }

        Current = EscapePhase.Consumed;
        return Accept();
    }

    PhaseChange Accept()
    {
        return new PhaseChange(true, Current);
    }

    PhaseChange Reject()
    {
        return new PhaseChange(false, Current);
    }
}
