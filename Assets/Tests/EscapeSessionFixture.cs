using NUnit.Framework;

[TestFixture]
public class EscapeSessionFixture
{
    CrewRoster roster;

    EscapePhaseMachine phase;

    [SetUp]
    public void SetUp()
    {
        roster = new CrewRoster();
        phase = new EscapePhaseMachine();
    }

    [Test]
    public void NewRosterIsAllAboard()
    {
        Assert.IsTrue(roster.AllAboard());
        Assert.IsFalse(roster.AnyDeployed());
    }

    [Test]
    public void DeployedPlayerIsNotAllAboard()
    {
        roster.Set(1, CrewLocation.Deployed);

        Assert.IsFalse(roster.AllAboard());
        Assert.IsTrue(roster.AnyDeployed());
        Assert.AreEqual(1, roster.DeployedCount());
    }

    [Test]
    public void RemovingDeployedPlayerRestoresAllAboard()
    {
        roster.Set(1, CrewLocation.Deployed);
        roster.Remove(1);

        Assert.IsTrue(roster.AllAboard());
        Assert.IsFalse(roster.AnyDeployed());
    }

    [Test]
    public void UndockWhileAllAboardEntersTransit()
    {
        var change = phase.TryUndock(roster.AllAboard());

        Assert.IsTrue(change.Accepted);
        Assert.AreEqual(EscapePhase.Transiting, change.Phase);
    }

    [Test]
    public void UndockWhileDeployedIsRejected()
    {
        roster.Set(1, CrewLocation.Deployed);

        var change = phase.TryUndock(roster.AllAboard());

        Assert.IsFalse(change.Accepted);
        Assert.AreEqual(EscapePhase.Docked, phase.Current);
    }

    [Test]
    public void DeployFromTransitIsRejected()
    {
        phase.TryUndock(true);

        var change = phase.TryDeploy();

        Assert.IsFalse(change.Accepted);
        Assert.AreEqual(EscapePhase.Transiting, phase.Current);
    }

    [Test]
    public void DeployFromDockedBecomesMissionActive()
    {
        var change = phase.TryDeploy();

        Assert.IsTrue(change.Accepted);
        Assert.AreEqual(EscapePhase.MissionActive, change.Phase);
    }

    [Test]
    public void RecallOfLastDeployedPlayerReturnsDocked()
    {
        phase.TryDeploy();
        roster.Set(1, CrewLocation.Aboard);

        var change = phase.TryRecall(roster.AnyDeployed());

        Assert.IsTrue(change.Accepted);
        Assert.AreEqual(EscapePhase.Docked, change.Phase);
    }

    [Test]
    public void RecallWhileSomeoneRemainsDeployedStaysMissionActive()
    {
        phase.TryDeploy();
        roster.Set(1, CrewLocation.Aboard);
        roster.Set(2, CrewLocation.Deployed);

        var change = phase.TryRecall(roster.AnyDeployed());

        Assert.IsTrue(change.Accepted);
        Assert.AreEqual(EscapePhase.MissionActive, change.Phase);
    }

    [Test]
    public void EscapeWithoutEquipmentIsRejected()
    {
        var change = phase.TryEscape(true, false);

        Assert.IsFalse(change.Accepted);
        Assert.AreEqual(EscapePhase.Docked, phase.Current);
    }

    [Test]
    public void EscapeWhileDeployedIsRejected()
    {
        roster.Set(1, CrewLocation.Deployed);

        var change = phase.TryEscape(roster.AllAboard(), true);

        Assert.IsFalse(change.Accepted);
        Assert.AreEqual(EscapePhase.Docked, phase.Current);
    }

    [Test]
    public void EscapeWhileDockedAboardAndEquipped()
    {
        var change = phase.TryEscape(roster.AllAboard(), true);

        Assert.IsTrue(change.Accepted);
        Assert.AreEqual(EscapePhase.Escaped, change.Phase);
    }

    [Test]
    public void ConsumeFromTransitBecomesConsumed()
    {
        phase.TryUndock(true);

        var change = phase.NotifyConsumed();

        Assert.IsTrue(change.Accepted);
        Assert.AreEqual(EscapePhase.Consumed, change.Phase);
    }

    [Test]
    public void TerminalPhasesIgnoreLaterTransitions()
    {
        phase.TryEscape(true, true);
        Assert.IsFalse(phase.TryDeploy().Accepted);
        Assert.IsFalse(phase.NotifyConsumed().Accepted);
        Assert.AreEqual(EscapePhase.Escaped, phase.Current);

        var consumed = new EscapePhaseMachine();
        consumed.NotifyConsumed();
        Assert.IsFalse(consumed.TryEscape(true, true).Accepted);
        Assert.AreEqual(EscapePhase.Consumed, consumed.Current);
    }
}
