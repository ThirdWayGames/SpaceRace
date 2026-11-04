---
name: Escape session contract
overview: Add the pure phase machine, crew roster, and room-property keys the galactic, ship, and mission plans share. No Photon calls, no scenes, no movement.
todos:
  - id: types
    content: Add EscapePhase, CrewLocation, StationRole, ShipPose, PilotCommand, and EscapeSessionKeys
    status: pending
  - id: roster-phase
    content: Add CrewRoster and EscapePhaseMachine with the transition rules from the index
    status: pending
  - id: tests
    content: Add EscapeSessionFixture covering aboard, deploy, undock, and terminal phases
    status: pending
isProject: false
---

# Escape session contract

Read [escape_views_index.plan.md](escape_views_index.plan.md) first. This plan lands the shared types. Later plans must not redefine them.

## Files

Create:

- `Assets/Scripts/Session/EscapePhase.cs`
- `Assets/Scripts/Session/CrewLocation.cs`
- `Assets/Scripts/Session/StationRole.cs`
- `Assets/Scripts/Session/ShipPose.cs`
- `Assets/Scripts/Session/PilotCommand.cs`
- `Assets/Scripts/Session/EscapeSessionKeys.cs`
- `Assets/Scripts/Session/CrewRoster.cs`
- `Assets/Scripts/Session/EscapePhaseMachine.cs`
- `Assets/Tests/EscapeSessionFixture.cs`

No MonoBehaviours. No `using` of Photon. No edits to `GameManager3D`, `GameStateManager`, `ShipManager`, or `LobbyManager`.

## Types

```csharp
public enum EscapePhase
{
    Docked,
    Transiting,
    MissionActive,
    Escaped,
    Consumed
}

public enum CrewLocation
{
    Aboard,
    Deployed
}

public enum StationRole
{
    Helm,
    Scanner,
    Turret,
    Airlock
}
```

`ShipPose` is a serializable class (not a tuple) with `float X`, `float Z`, `float HeadingDegrees`, `float Speed`.

`PilotCommand` is a serializable class with `float Throttle` and `float Turn`, both clamped to -1..1 by the constructor or a `Clamp` method.

`EscapeSessionKeys` holds public const strings:

- `EscapePhase` = `"EscapePhase"`
- `ActivePoiId` = `"ActivePoiId"`
- `ActiveMissionId` = `"ActiveMissionId"`
- `RevealedContactIds` = `"RevealedContactIds"`
- `EquipmentIds` = `"EquipmentIds"`
- `CrewLocation` = `"CrewLocation"`
- `Hull` = `"Hull"`

Do not reuse `GameManager3D.MatchPhaseKey` or `WinningTeamKey`.

## CrewRoster

Plain class. Stores player id (`int`, Photon actor id later) to `CrewLocation`.

- `Set(int playerId, CrewLocation location)`
- `Remove(int playerId)`
- `bool AllAboard()` — true when the roster is empty or every entry is `Aboard`. Empty is aboard so a fresh session can undock in tests.
- `bool AnyDeployed()`
- `int DeployedCount()`

Dead players are not a roster concern. Callers remove them or set them `Aboard`. Document that on `AllAboard`.

## EscapePhaseMachine

Pure. Methods return the next phase and do not mutate a hidden field. Keep a `Current` property the tests can set.

Rules:

- `TryDeploy()` from `Docked` or `MissionActive` returns `MissionActive`. From `Transiting`, `Escaped`, or `Consumed` it returns the current phase and `false`.
- `TryRecallAll()` and a single recall that leaves `DeployedCount == 0` return `Docked` from `MissionActive`. Recalling while someone remains deployed stays `MissionActive`.
- `TryUndock()` from `Docked` when `AllAboard()` returns `Transiting`. If anyone is deployed, it returns `Docked` and `false`.
- `NotifyArrived()` from `Transiting` returns `Docked`.
- `TryEscape()` from `Docked` when `AllAboard()` and a `bool hasEquipment` argument is true returns `Escaped`. Otherwise false and the current phase.
- `NotifyConsumed()` from any non-terminal phase returns `Consumed`.
- `Escaped` and `Consumed` ignore every later call and return false.

Use a small result class `PhaseChange` with `bool Accepted` and `EscapePhase Phase`. No tuples.

## Tests

`Assets/Tests/EscapeSessionFixture.cs`, `[TestFixture]`, `[Test]`, `NUnit.Framework`. No scene, no prefabs.

Cover:

- new roster is all-aboard
- one deployed player makes `AllAboard` false and `AnyDeployed` true
- removing that player restores all-aboard
- undock from `Docked` while all aboard becomes `Transiting`
- undock while deployed is rejected
- deploy from `Transiting` is rejected
- deploy from `Docked` becomes `MissionActive`
- recall of the last deployed player returns `Docked`
- escape without equipment is rejected
- escape while deployed is rejected
- escape while docked, aboard, and equipped becomes `Escaped`
- consume from `Transiting` becomes `Consumed`
- a second transition after `Escaped` or `Consumed` is rejected

## Done when

The fixture compiles against the new types and the assertions above hold. Do not add a scene, a prefab, or a Photon adapter in this plan.
