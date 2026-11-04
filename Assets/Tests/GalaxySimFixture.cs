using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class GalaxySimFixture
{
    [Test]
    public void ThrottleForwardMovesAlongHeading()
    {
        var pose = new ShipPose(0f, 0f, 0f, 0f);
        var next = ShipFlight.Integrate(pose, new PilotCommand(1f, 0f), 1f, 1f);

        Assert.Greater(next.Speed, 0f);
        Assert.AreEqual(0f, next.X, 0.001f);
        Assert.Greater(next.Z, 0f);
    }

    [Test]
    public void PowerZeroDoesNotAccelerateOrTurn()
    {
        var pose = new ShipPose(2f, 3f, 15f, 4f);
        var next = ShipFlight.Integrate(pose, new PilotCommand(1f, 1f), 0f, 1f);

        Assert.AreEqual(4f, next.Speed, 0.001f);
        Assert.AreEqual(15f, next.HeadingDegrees, 0.001f);
    }

    [Test]
    public void SpeedDoesNotExceedCap()
    {
        var pose = new ShipPose(0f, 0f, 0f, 0f);
        var next = ShipFlight.Integrate(pose, new PilotCommand(1f, 0f), 1f, 10f);

        Assert.AreEqual(ShipFlight.MaxSpeed, next.Speed, 0.001f);
    }

    [Test]
    public void StormReachingZeroConsumesTheShip()
    {
        var sim = GalaxySim.CreateSample();
        sim.Storm.Radius = 1f;
        sim.Storm.ShrinkPerSecond = 5f;

        sim.Tick(1f, 1f);

        Assert.AreEqual(0f, sim.Storm.Radius, 0.001f);
        Assert.AreEqual(EscapePhase.Consumed, sim.Phase.Current);
    }

    [Test]
    public void OutsideTheStormHullDecreases()
    {
        var sim = GalaxySim.CreateSample();
        sim.Storm.Radius = 10f;
        sim.Storm.ShrinkPerSecond = 0f;
        sim.Pose = new ShipPose(0f, 100f, 0f, 0f);
        sim.Command = new PilotCommand();

        sim.Tick(1f, 1f);

        Assert.AreEqual(100f - GalaxySim.OutsideStormDrainPerSecond, sim.Hull, 0.001f);
    }

    [Test]
    public void ArrivingAtWreckDocks()
    {
        var sim = GalaxySim.CreateSample();
        sim.Phase.Current = EscapePhase.Transiting;
        sim.Pose = new ShipPose(0f, 200f, 0f, 0f);
        sim.Command = new PilotCommand();

        sim.Tick(0.1f, 1f);

        Assert.AreEqual(EscapePhase.Docked, sim.Phase.Current);
        Assert.AreEqual("wreck", sim.CurrentPoiId);
    }

    [Test]
    public void DeployedCrewCannotLeaveTheWreck()
    {
        var sim = DockAtWreck();
        sim.Roster.Set(1, CrewLocation.Deployed);
        sim.Command = new PilotCommand(1f, 0f);

        sim.Tick(3f, 1f);

        Assert.AreEqual(EscapePhase.Docked, sim.Phase.Current);
        Assert.AreEqual("wreck", sim.CurrentPoiId);
        Assert.LessOrEqual(Distance(sim.Pose.X, sim.Pose.Z, 0f, 200f), 25f);
    }

    [Test]
    public void AboardCrewCanLeaveTheWreck()
    {
        var sim = DockAtWreck();
        sim.Command = new PilotCommand(1f, 0f);

        sim.Tick(3f, 1f);

        Assert.AreEqual(EscapePhase.Transiting, sim.Phase.Current);
        Assert.AreEqual(string.Empty, sim.CurrentPoiId);
    }

    [Test]
    public void RevealMarksTheContactInFront()
    {
        var sim = GalaxySim.CreateSample();
        sim.Pose = new ShipPose(0f, 160f, 0f, 0f);

        Assert.IsTrue(sim.TryReveal(80f, 80f));
        Assert.IsTrue(sim.Map.FindContact("raider").Revealed);
        Assert.IsFalse(sim.Map.FindContact("radiation").Revealed);
    }

    [Test]
    public void DeadRaiderStopsOverlapDrain()
    {
        var sim = GalaxySim.CreateSample();
        sim.Storm.ShrinkPerSecond = 0f;
        sim.Pose = new ShipPose(-40f, 220f, 0f, 0f);
        sim.Command = new PilotCommand();
        sim.Phase.Current = EscapePhase.Transiting;

        sim.Tick(1f, 1f);
        var afterHit = sim.Hull;
        Assert.Less(afterHit, 100f);

        sim.DamageContact("raider", 30f);
        sim.Tick(1f, 1f);

        Assert.AreEqual(afterHit, sim.Hull, 0.001f);
        Assert.AreEqual(0f, sim.Map.FindContact("raider").Health, 0.001f);
    }

    [Test]
    public void WormholeWithoutCoilIsRejected()
    {
        var sim = DockAt("eye");

        Assert.IsFalse(sim.TryWormhole());
        Assert.AreEqual(EscapePhase.Docked, sim.Phase.Current);
    }

    [Test]
    public void WormholeWithCoilEscapes()
    {
        var sim = DockAt("eye");
        sim.SetEquipment("coil");

        Assert.IsTrue(sim.TryWormhole());
        Assert.AreEqual(EscapePhase.Escaped, sim.Phase.Current);
    }

    static GalaxySim DockAtWreck()
    {
        return DockAt("wreck");
    }

    static GalaxySim DockAt(string poiId)
    {
        var sim = GalaxySim.CreateSample();
        var point = sim.Map.FindPoint(poiId);
        sim.Pose = new ShipPose(point.X, point.Z, 0f, 0f);
        sim.CurrentPoiId = poiId;
        sim.Phase.Current = EscapePhase.Docked;
        sim.Command = new PilotCommand();
        sim.Tick(0.01f, 0f);
        sim.Command = new PilotCommand();
        return sim;
    }

    static float Distance(float x1, float z1, float x2, float z2)
    {
        var dx = x1 - x2;
        var dz = z1 - z2;
        return Mathf.Sqrt((dx * dx) + (dz * dz));
    }
}
