using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class StationOccupancyFixture
{
    StationOccupancy occupancy;

    [SetUp]
    public void SetUp()
    {
        occupancy = new StationOccupancy();
    }

    [Test]
    public void SitClaimsTheStation()
    {
        Assert.IsTrue(occupancy.TrySit("Helm", 1));
        Assert.AreEqual("Helm", occupancy.StationFor(1));
        Assert.AreEqual(1, occupancy.PlayerIn("Helm"));
    }

    [Test]
    public void SecondPlayerCannotTakeAFilledStation()
    {
        occupancy.TrySit("Helm", 1);

        Assert.IsFalse(occupancy.TrySit("Helm", 2));
        Assert.AreEqual(string.Empty, occupancy.StationFor(2));
    }

    [Test]
    public void SeatedPlayerCannotTakeAnotherStation()
    {
        occupancy.TrySit("Helm", 1);

        Assert.IsFalse(occupancy.TrySit("Scanner", 1));
        Assert.AreEqual("Helm", occupancy.StationFor(1));
    }

    [Test]
    public void StandingFreesTheSeat()
    {
        occupancy.TrySit("Helm", 1);
        Assert.IsTrue(occupancy.TryStand(1));

        Assert.AreEqual(string.Empty, occupancy.StationFor(1));
        Assert.IsTrue(occupancy.TrySit("Helm", 2));
    }

    [Test]
    public void RoundTripKeepsSeats()
    {
        occupancy.TrySit("Helm", 3);
        occupancy.TrySit("Turret", 4);
        var copy = new StationOccupancy();
        copy.Read(occupancy.Write());

        Assert.AreEqual(3, copy.PlayerIn("Helm"));
        Assert.AreEqual(4, copy.PlayerIn("Turret"));
    }

    [Test]
    public void TurretPicksTheNearestRevealedHostileInArc()
    {
        var summary = "raider,0,10,Hostile,1,30;lurker,0,40,Hostile,1,10;behind,0,-10,Hostile,1,10;hidden,0,8,Hostile,0,30";

        Assert.AreEqual("raider", TurretAim.Pick(0f, 0f, 0f, summary));
    }

    [Test]
    public void TurretIgnoresTargetsOutsideTheArc()
    {
        var summary = "wide,30,10,Hostile,1,30";

        Assert.AreEqual(string.Empty, TurretAim.Pick(0f, 0f, 0f, summary));
    }
}
