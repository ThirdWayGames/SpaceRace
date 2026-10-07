using NUnit.Framework;

public class SpaceRaceCopyFixture
{
    [Test]
    public void SoloHostCanLaunchWithoutAReadyCrew()
    {
        Assert.IsTrue(LobbyManager.HostCanLaunch(true, 1, false));
        Assert.IsTrue(LobbyManager.HostCanLaunch(true, 0, false));
        Assert.IsFalse(LobbyManager.HostCanLaunch(true, 2, false));
        Assert.IsTrue(LobbyManager.HostCanLaunch(true, 2, true));
        Assert.IsFalse(LobbyManager.HostCanLaunch(false, 1, true));
    }

    [Test]
    public void TimeRemainingPadsMinutesAndSeconds()
    {
        Assert.AreEqual("TIME: 00:05", SpaceRaceCopy.FormatTimeRemaining(5));
        Assert.AreEqual("TIME: 02:05", SpaceRaceCopy.FormatTimeRemaining(125));
        Assert.AreEqual("TIME: 00:00", SpaceRaceCopy.FormatTimeRemaining(-4));
    }

    [Test]
    public void ConnectionStatusUsesPlayerLanguage()
    {
        Assert.AreEqual("Connecting", SpaceRaceCopy.ConnectionStatus("ConnectingToMasterserver"));
        Assert.AreEqual("Joining match", SpaceRaceCopy.ConnectionStatus("Joining"));
        Assert.AreEqual("Joining match", SpaceRaceCopy.ConnectionStatus("JoinedLobby"));
        Assert.AreEqual("Could not connect", SpaceRaceCopy.ConnectionStatus("Disconnected"));
        Assert.AreEqual("Connecting", SpaceRaceCopy.ConnectionStatus(null));
    }

    [Test]
    public void WinAnnouncementNamesTheCoreThatFell()
    {
        Assert.AreEqual("RED TEAM WINS\nThe blue core was destroyed.", SpaceRaceCopy.WinAnnouncement(1));
        Assert.AreEqual("BLUE TEAM WINS\nThe red core was destroyed.", SpaceRaceCopy.WinAnnouncement(2));
        Assert.AreEqual("MATCH OVER", SpaceRaceCopy.WinAnnouncement(0));
    }

    [Test]
    public void StationLabelsReplaceJokeAndMisspelledEquipment()
    {
        Assert.AreEqual("Auxiliary Power Bus", SpaceRaceCopy.Rewrite("Netflix Subscription"));
        Assert.AreEqual("Coolant Flange", SpaceRaceCopy.Rewrite("Flange Wangler"));
        Assert.AreEqual("Diametric Gyroscope", SpaceRaceCopy.Rewrite("Dyametric Gyroscope"));
        Assert.AreEqual("Plasma Lubricator", SpaceRaceCopy.Rewrite("Plasma Lubticator"));
        Assert.AreEqual("Parabolic Stabilisers", SpaceRaceCopy.Rewrite("Parabolic Stabalisers"));
        Assert.AreEqual("Energy Emitter", SpaceRaceCopy.Rewrite("Energy Emmiter"));
        Assert.AreEqual("Gamma Ray Emitter", SpaceRaceCopy.Rewrite("Gamma Ray Emmiter"));
    }

    [Test]
    public void LobbySentencesAreRewrittenInPlace()
    {
        var science = SpaceRaceCopy.Rewrite("Use your Science Console to sythesise pathogens.");
        Assert.IsTrue(science.Contains("synthesise"));
        Assert.IsFalse(science.Contains("sythesise"));

        var equipment = SpaceRaceCopy.Rewrite("change your carriables to adapt your teams strategy.");
        Assert.AreEqual("change your equipment to adapt your team's strategy.", equipment);
    }

    [Test]
    public void ConsoleInstructionsMatchTheOpenScreen()
    {
        Assert.IsTrue(SpaceRaceCopy.ConsoleInstruction("SynthLab").Contains("sockets"));
        Assert.AreEqual(SpaceRaceCopy.MildSevereLegend, SpaceRaceCopy.ConsoleInstruction("Database"));
        Assert.IsNull(SpaceRaceCopy.ConsoleInstruction("CircuitGame"));
        Assert.IsTrue(SpaceRaceCopy.HowToPlay.Contains("MOVE"));
        Assert.IsTrue(SpaceRaceCopy.HowToPlay.Contains("FIGHT"));
        Assert.IsTrue(SpaceRaceCopy.HowToPlay.Contains("CONSOLES"));
        Assert.IsTrue(SpaceRaceCopy.HowToPlay.Contains("MAP"));
    }
}
