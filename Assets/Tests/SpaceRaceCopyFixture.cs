using NUnit.Framework;
using UnityEngine;

public class SpaceRaceCopyFixture
{
    [Test]
    public void HandIconsMatchTheEquippedWeapon()
    {
        Assert.AreEqual("HeavyBlaster", HandIcons.CatalogKey("HeavyBlaster(Clone)"));
        Assert.AreEqual("Grenade", HandIcons.CatalogKey("Grenade"));
        Assert.AreEqual("Flare", HandIcons.CatalogKey("Throwable"));
        Assert.AreEqual("sniper-rifle", HandIcons.CatalogKey("SniperRifle"));
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
    public void SniperClaimsBothHandsAndGrenadesFitEitherHand()
    {
        string left = "HeavyBlaster";
        string right = "Throwable";
        LoadoutHands.Assign(ref left, ref right, true, "SniperRifle");
        Assert.AreEqual("SniperRifle", left);
        Assert.IsNull(right);

        LoadoutHands.Assign(ref left, ref right, false, "Grenade");
        Assert.IsNull(left);
        Assert.AreEqual("Grenade", right);

        left = "SniperRifle";
        right = null;
        LoadoutHands.Assign(ref left, ref right, false, "Throwable");
        Assert.IsNull(left);
        Assert.AreEqual("Throwable", right);
        Assert.IsTrue(GrenadeBlast.Inside(Vector3.zero, new Vector3(1f, 0f, 0f), 3.4f));
        Assert.IsFalse(GrenadeBlast.Inside(Vector3.zero, new Vector3(4f, 0f, 0f), 3.4f));
        Assert.AreEqual(19f, CameraFollow3D.ScopedDistance(10f, 3.5f, 3f, 19f), 0.001f);
        Assert.AreEqual(19f, CameraFollow3D.ScopedDistance(18f, 3.5f, 3f, 19f), 0.001f);
        Assert.AreEqual(12f, CameraFollow3D.ScopedDistance(10f, -2f, 3f, 12f), 0.001f);
        Assert.Greater(SniperRifle.LaserCore.r, 0.8f);
        Assert.Less(SniperRifle.LaserCore.a, 0.75f);
        Assert.Less(SniperRifle.LaserCore.b, 0.35f);
        Assert.AreEqual(2, SniperRifle.StepScopeMagnification(1, 0.2f));
        Assert.AreEqual(0, SniperRifle.StepScopeMagnification(1, -0.2f));
        Assert.AreEqual(2, SniperRifle.StepScopeMagnification(2, 0.2f));
        Assert.AreEqual(0, SniperRifle.StepScopeMagnification(0, -0.2f));
        Assert.AreEqual(8f, SniperRifle.ScopeMagnificationAt(0), 0.001f);
        Assert.AreEqual(10f, SniperRifle.ScopeMagnificationAt(1), 0.001f);
        Assert.AreEqual(12f, SniperRifle.ScopeMagnificationAt(2), 0.001f);
        Assert.AreEqual("2x", SniperRifle.ScopeMagnificationName(0));
        Assert.AreEqual("4x", SniperRifle.ScopeMagnificationName(1));
        Assert.AreEqual("6x", SniperRifle.ScopeMagnificationName(2));
        Assert.AreEqual(1, SniperRifle.NearestScopeMagnification(10f));
        Assert.AreEqual(2, SniperRifle.NearestScopeMagnification(12f));
        Assert.AreEqual(0, SniperRifle.NearestScopeMagnification(8f));
        var leftEdge = CameraFollow3D.EdgePush(new Vector2(10f, 300f), 800f, 600f, 150f);
        Assert.AreEqual(-1f, leftEdge.x, 0.001f);
        Assert.AreEqual(0f, leftEdge.y, 0.001f);
        var topRight = CameraFollow3D.EdgePush(new Vector2(700f, 500f), 800f, 600f, 150f);
        Assert.AreEqual(1f, topRight.x, 0.001f);
        Assert.AreEqual(1f, topRight.y, 0.001f);
        Assert.AreEqual(0f, CameraFollow3D.EdgePush(new Vector2(400f, 300f), 800f, 600f, 150f).sqrMagnitude, 0.001f);
        Assert.IsTrue(CameraFollow3D.PlayerInsideFrame(new Vector2(200f, 200f), 800f, 600f, 150f));
        Assert.IsFalse(CameraFollow3D.PlayerInsideFrame(new Vector2(40f, 300f), 800f, 600f, 150f));
        Assert.AreEqual(150f, CameraFollow3D.ScopeEdgeMargin(1920f, 1080f, 150f), 0.001f);
        Assert.AreEqual(80f, CameraFollow3D.ScopeEdgeMargin(200f, 200f, 150f), 0.001f);
        var groundPan = CameraFollow3D.ScopePanDelta(new Vector2(0f, 1f), Vector3.right, new Vector3(0f, 0.7f, 0.7f), 3f);
        Assert.AreEqual(0f, groundPan.y, 0.001f);
        Assert.AreEqual(3f, groundPan.z, 0.001f);
        var aim = new Vector3(2f, 1.2f, 4f);
        var offset = new Vector3(0f, 12f, -8f);
        var scopePosition = SniperScopeView.BirdseyePosition(aim, offset);
        Assert.AreEqual(aim.x, scopePosition.x - offset.x, 0.001f);
        Assert.Greater(scopePosition.y, aim.y + 10f);
        var look = SniperScopeView.ScopeLookDirection(offset);
        Assert.Less(look.y, -0.5f);
        Assert.IsTrue(SniperRifle.IsShotBlocked(8f, 3f, 0.2f));
        Assert.IsFalse(SniperRifle.IsShotBlocked(8f, 7.95f, 0.2f));
        Assert.AreEqual(1f, ScopeSwayMath.Multiplier(0.9f, true, false, 0f, 1.1f, 0f), 0.001f);
        Assert.AreEqual(0.55f, ScopeSwayMath.Multiplier(0.9f, true, false, 0f, 1.1f, 0.5f), 0.001f);
        Assert.AreEqual(0.1f, ScopeSwayMath.Multiplier(0.9f, true, false, 0f, 1.1f, 1f), 0.001f);
        Assert.AreEqual(1.1f, ScopeSwayMath.Multiplier(0.9f, false, true, 0f, 1.1f, 1f), 0.001f);
        Assert.AreEqual(1f, ScopeSwayMath.Multiplier(0.9f, false, true, 1f, 1.1f, 0f), 0.001f);
        Assert.IsFalse(CameraFollow3D.AllowSprint(true, true));
        Assert.IsTrue(CameraFollow3D.AllowSprint(false, true));
        Assert.IsFalse(CameraFollow3D.AllowSprint(true, false));
        Assert.IsTrue(ScopeSwayMath.HoldingBreath(true, true, false));
        Assert.IsFalse(ScopeSwayMath.HoldingBreath(true, true, true));
        Assert.IsFalse(ScopeSwayMath.HoldingBreath(false, true, false));
        Assert.IsTrue(SniperRifle.BlocksScopeLos(false, false));
        Assert.IsFalse(SniperRifle.BlocksScopeLos(true, false));
        Assert.IsFalse(SniperRifle.BlocksScopeLos(false, true));
        Assert.IsFalse(SniperRifle.BlocksScopeLos(false, false, true));
        var mapDiameter = SniperScopeView.MinimapCircleDiameter(SniperScopeView.MinimapWidgetWidth, SniperScopeView.MinimapWidgetHeight, SniperScopeView.MinimapScaleX, SniperScopeView.MinimapScaleY);
        Assert.AreEqual(133.219f, mapDiameter, 0.05f);
        Assert.AreEqual(mapDiameter * 0.5f * SniperScopeView.ScopeDiameterScale, SniperScopeView.LensRadius, 0.001f);
        Assert.AreEqual(1.05f * 1.05f, SniperScopeView.ScopeDiameterScale, 0.001f);
        Assert.IsTrue(SniperScopeView.BorderThickness >= 5f);
        Assert.AreEqual((SniperScopeView.LensRadius + SniperScopeView.BorderThickness) * 2f, SniperScopeView.BorderDiameter(SniperScopeView.LensRadius, SniperScopeView.BorderThickness), 0.001f);
        var breathArc = SniperScopeView.BreathArcPosition(new Vector3(400f, 300f, 0f), SniperScopeView.BreathArcOffsetY);
        Assert.AreEqual(400f, breathArc.x, 0.001f);
        Assert.AreEqual(295f, breathArc.y, 0.001f);
        var arcRadius = SniperScopeView.LabelArcRadius(SniperScopeView.LensRadius, SniperScopeView.BottomBandDepth);
        var leftAngle = SniperScopeView.CurvedGlyphAngle(0, 3, SniperScopeView.LabelArcCenter, SniperScopeView.LabelGlyphSpacing);
        var rightAngle = SniperScopeView.CurvedGlyphAngle(2, 3, SniperScopeView.LabelArcCenter, SniperScopeView.LabelGlyphSpacing);
        Assert.Less(leftAngle, SniperScopeView.LabelArcCenter);
        Assert.Greater(rightAngle, SniperScopeView.LabelArcCenter);
        Assert.AreNotEqual(SniperScopeView.CurvedGlyphRotation(leftAngle), SniperScopeView.CurvedGlyphRotation(rightAngle));
        Assert.AreEqual(0f, SniperScopeView.CurvedGlyphRotation(-90f), 0.001f);
        var magLabel = SniperScopeView.CurvedGlyphPosition(new Vector2(400f, 300f), arcRadius, SniperScopeView.LabelArcCenter);
        var fromCenter = magLabel - new Vector2(400f, 300f);
        Assert.AreEqual(arcRadius, fromCenter.magnitude, 0.05f);
        Assert.Greater(fromCenter.magnitude, SniperScopeView.LensRadius);
        Assert.Less(fromCenter.magnitude, SniperScopeView.LensRadius + SniperScopeView.BottomBandDepth);
        Assert.AreEqual(0.18f, SniperScopeView.BreathArcFill(1f, SniperScopeView.BreathArcSpan), 0.001f);
        Assert.AreEqual(0f, SniperScopeView.BreathArcFill(0f, SniperScopeView.BreathArcSpan), 0.001f);
        var easing = ScopeSwayMath.Step(new ScopeSwayState(), true, 0.25f, 4f, 1.5f, 0.5f);
        Assert.AreEqual(0.5f, easing.Settle, 0.001f);
        Assert.IsFalse(easing.Recovering);
        var released = ScopeSwayMath.Step(easing, false, 0.1f, 4f, 1.5f, 0.5f);
        Assert.AreEqual(0f, released.Breath, 0.001f);
        Assert.IsFalse(released.Recovering);
        var breath = ScopeSwayMath.Step(new ScopeSwayState(), true, 4f, 4f, 1.5f, 0.5f);
        Assert.IsTrue(breath.Recovering);
        Assert.AreEqual(1f, breath.Breath, 0.001f);
        breath = ScopeSwayMath.Step(breath, true, 0.75f, 4f, 1.5f, 0.5f);
        Assert.IsTrue(breath.Recovering);
        Assert.AreEqual(0.5f, breath.Breath, 0.001f);
        breath = ScopeSwayMath.Step(breath, true, 0.75f, 4f, 1.5f, 0.5f);
        Assert.IsFalse(breath.Recovering);
        Assert.AreEqual(0f, breath.Breath, 0.001f);
        var drift = ScopeSwayMath.Offset(1.2f, 20f, 1.4f);
        Assert.LessOrEqual(drift.magnitude, 20f);
        Assert.Greater(drift.sqrMagnitude, 0.01f);
        var sight = SniperRifle.LaserEnd(new Vector3(0f, 1.2f, 0f), new Vector3(0f, 0.2f, 8f), -1f);
        Assert.AreEqual(8f, sight.z, 0.001f);
        Assert.AreEqual(1.26f, sight.y, 0.001f);
        var blocked = SniperRifle.LaserEnd(new Vector3(0f, 1.2f, 0f), new Vector3(0f, 1.2f, 8f), 3f);
        Assert.AreEqual(3f, blocked.z, 0.001f);
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
