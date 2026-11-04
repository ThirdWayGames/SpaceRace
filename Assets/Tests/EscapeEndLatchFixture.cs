using NUnit.Framework;

[TestFixture]
public class EscapeEndLatchFixture
{
    [Test]
    public void FirstTerminalPhaseIsPublished()
    {
        var latch = new EndLatch();

        Assert.IsTrue(latch.TryPublish(EscapePhase.Escaped));
        Assert.IsTrue(latch.Latched);
        Assert.AreEqual(EscapePhase.Escaped, latch.Phase);
    }

    [Test]
    public void LaterPhaseChangeIsIgnored()
    {
        var latch = new EndLatch();
        latch.TryPublish(EscapePhase.Escaped);

        Assert.IsFalse(latch.TryPublish(EscapePhase.Consumed));
        Assert.AreEqual(EscapePhase.Escaped, latch.Phase);
    }

    [Test]
    public void ConsumedDoesNotBecomeEscaped()
    {
        var latch = new EndLatch();
        latch.TryPublish(EscapePhase.Consumed);

        Assert.IsFalse(latch.TryPublish(EscapePhase.Escaped));
        Assert.AreEqual(EscapePhase.Consumed, latch.Phase);
    }

    [Test]
    public void NonTerminalPhaseIsNotPublished()
    {
        var latch = new EndLatch();

        Assert.IsFalse(latch.TryPublish(EscapePhase.Transiting));
        Assert.IsFalse(latch.Latched);
    }
}
