using NUnit.Framework;
using UnityEngine;

public class SpaceRaceCopyFixture
{
    [Test]
    public void HandIconsMatchTheEquippedWeapon()
    {
        Assert.AreEqual("HeavyBlaster", HandIcons.CatalogKey("HeavyBlaster(Clone)"));
        Assert.AreEqual("PathogenBlaster", HandIcons.CatalogKey("PathogenBlasterLeft"));
        Assert.AreEqual("EnergyRecharger", HandIcons.CatalogKey("EnergyBlaster"));
        Assert.AreEqual("MediRay", HandIcons.CatalogKey("HealBlaster"));
        Assert.AreEqual("LightBlaster", HandIcons.CatalogKey("LightDroidBlaster"));
    }

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
    public void RespawnCountdownKeepsAFixedWidth()
    {
        Assert.AreEqual("RESPAWNING:", SpaceRaceCopy.RespawnLabel);
        Assert.AreEqual("04.00", SpaceRaceCopy.FormatRespawnCountdown(4f));
        Assert.AreEqual("10.00", SpaceRaceCopy.FormatRespawnCountdown(10f));
        Assert.AreEqual("00.00", SpaceRaceCopy.FormatRespawnCountdown(-2f));
        Assert.AreEqual(5, SpaceRaceCopy.FormatRespawnCountdown(4f).Length);
        Assert.AreEqual(SpaceRaceCopy.FormatRespawnCountdown(4f).Length, SpaceRaceCopy.FormatRespawnCountdown(30f).Length);

        float labelRight;
        float digitsLeft;
        SpaceRaceCopy.CenterRespawnReadout(900f, 390f, 16f, out labelRight, out digitsLeft);
        var leftEdge = labelRight - 900f;
        var rightEdge = digitsLeft + 390f;
        Assert.AreEqual(0f, leftEdge + rightEdge, 0.01f);
        Assert.Less(System.Math.Abs(leftEdge), 800f);
        Assert.AreEqual(110, SpaceRaceCopy.FitRespawnFontSize(110, 1300f, 1920f, 48f));
        Assert.Less(SpaceRaceCopy.FitRespawnFontSize(110, 1400f, 1280f, 48f), 110);
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

    [Test]
    public void RespawnRestoresEmptyEnergyAndHidesTheStartupVeil()
    {
        Assert.AreEqual(100f, SpawnVitals.RestoredEnergy(0f, 100f));
        Assert.AreEqual(100f, SpawnVitals.RestoredEnergy(100f, 100f));
        Assert.AreEqual(40f, SpawnVitals.RestoredEnergy(40f, 100f));
        Assert.AreEqual(100f, SpawnVitals.RestoredEnergy(140f, 100f));
        Assert.IsTrue(SpawnVitals.ShouldHideOverlay(0f, 0f, 0f, 1f, 1f));
        Assert.IsFalse(SpawnVitals.ShouldHideOverlay(0.92f, 0f, 0f, 1f, 1f));
        Assert.IsFalse(SpawnVitals.ShouldHideOverlay(0f, 0.2f, 0.2f, 0.4f, 0.5f));
    }

    [Test]
    public void HowToCardHugsTheInstructions()
    {
        var fitted = SpaceRaceWidgets.HowToCardSize(594f, 400f, 1920f);
        Assert.Greater(fitted.x, 600f);
        Assert.Less(fitted.x, 760f);
        Assert.AreEqual(474f, fitted.y, 0.1f);

        var capped = SpaceRaceWidgets.HowToCardSize(1400f, 400f, 1920f);
        Assert.AreEqual(760f, capped.x, 0.1f);
    }

    [Test]
    public void ConsoleTipKeepsTheLockedMessageOnOneLine()
    {
        Assert.AreEqual(196f, ConsoleTipLayout.TextWidth(172f, "!! CONSOLE LOCKED !!".Length));
        Assert.AreEqual(224f, ConsoleTipLayout.TextWidth(90f, "!! CONSOLE LOCKED !!".Length));
        Assert.AreEqual(224f, ConsoleTipLayout.TextWidth(0f, "!! CONSOLE LOCKED !!".Length));
        Assert.AreEqual(214f, ConsoleTipLayout.TextWidth(float.NaN, "!! ACCESS DENIED !!".Length));
        Assert.AreEqual(224f, ConsoleTipLayout.TextWidth(800f, "!! CONSOLE LOCKED !!".Length));

        var widened = ConsoleTipLayout.CanvasWidth(196f, 0.05f, 2f, 10f);
        Assert.AreEqual(19.6f, widened, 0.001f);
        Assert.AreEqual(10f, ConsoleTipLayout.CanvasWidth(40f, 0.05f, 2f, 10f));
    }

    [Test]
    public void ShotsConvergeOnTheCursorDistance()
    {
        Vector3 point;
        Assert.IsTrue(ShotAim.TryPlanePoint(new Vector3(0f, 5f, 2f), new Vector3(0f, -1f, 0f), 0f, out point));
        Assert.AreEqual(0f, point.x, 0.001f);
        Assert.AreEqual(0f, point.y, 0.001f);
        Assert.AreEqual(2f, point.z, 0.001f);
        Assert.IsFalse(ShotAim.TryPlanePoint(new Vector3(0f, 5f, 0f), new Vector3(0f, 1f, 0f), 0f, out point));
        Assert.IsFalse(ShotAim.TryPlanePoint(new Vector3(0f, 5f, 0f), new Vector3(1f, 0f, 0f), 0f, out point));

        var muzzle = new Vector3(0f, 1.2f, 0f);
        var cursor = new Vector3(3f, 4f, 4f);
        var direction = ShotAim.Direction(muzzle, cursor, Vector3.forward);
        var distance = new Vector3(cursor.x - muzzle.x, 0f, cursor.z - muzzle.z).magnitude;
        var arrival = muzzle + direction * distance;
        Assert.AreEqual(cursor.x, arrival.x, 0.001f);
        Assert.AreEqual(muzzle.y, arrival.y, 0.001f);
        Assert.AreEqual(cursor.z, arrival.z, 0.001f);
        Assert.AreEqual(90f, ShotAim.YawOffset(Vector3.forward, Vector3.right), 0.05f);

        var close = ShotAim.Direction(Vector3.zero, new Vector3(0.1f, 2f, 0f), Vector3.forward);
        Assert.AreEqual(0f, close.x, 0.001f);
        Assert.AreEqual(1f, close.z, 0.001f);
        Assert.AreEqual(0.25f, ShotEffects.ColorFor("EnergyBlaster").r, 0.001f);
        Assert.AreEqual(1f, ShotEffects.ColorFor("HeavyBlaster").r, 0.001f);
    }
}
