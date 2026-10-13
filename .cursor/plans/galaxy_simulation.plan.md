---
name: Galaxy simulation
overview: Master-authoritative flat flight, a shrinking storm, and authored locations and contacts. Pure classes plus a thin Photon host. No station cameras and no mission loading.
todos:
  - id: map-data
    content: Add PointOfInterest, GalaxyContact, StormFront, and a hand-authored GalaxyMap sample
    status: pending
  - id: sim
    content: Add ShipFlight and GalaxySim.Tick with hull drain, reveal, and arrival
    status: pending
  - id: tests
    content: Add GalaxySimFixture for flight, storm, arrival, and blocked departure
    status: pending
  - id: host
    content: Add EscapeSessionHost that instantiates one master-owned view and serializes the snapshot
    status: pending
isProject: false
---

# Galaxy simulation

Depends on [escape_session_contract.plan.md](escape_session_contract.plan.md). Read [escape_views_index.plan.md](escape_views_index.plan.md). Do not edit station, mission, or lobby files.

## Files

Create:

- `Assets/Scripts/Session/Galaxy/PointOfInterest.cs`
- `Assets/Scripts/Session/Galaxy/GalaxyContact.cs`
- `Assets/Scripts/Session/Galaxy/ContactKind.cs`
- `Assets/Scripts/Session/Galaxy/StormFront.cs`
- `Assets/Scripts/Session/Galaxy/GalaxyMap.cs`
- `Assets/Scripts/Session/Galaxy/ShipFlight.cs`
- `Assets/Scripts/Session/Galaxy/GalaxySim.cs`
- `Assets/Scripts/Session/Galaxy/EscapeSessionHost.cs`
- `Assets/Tests/GalaxySimFixture.cs`

`EscapeSessionHost` is the only MonoBehaviour. Everything it decides lives in `GalaxySim`, which the fixture tests without Photon.

## Data

`PointOfInterest`: `string Id`, `float X`, `float Z`, `float Radius`, `string MissionId` (empty for the wormhole), `string EquipmentRewardId` (empty if none), `bool IsWormhole`.

`ContactKind`: `Hazard`, `Hostile`.

`GalaxyContact`: `string Id`, `float X`, `float Z`, `float Radius`, `ContactKind Kind`, `bool Revealed`, `float Health`. Hostiles start with health above zero. Hazards use health 0 and are not shot down.

`StormFront`: `float Radius`, `float ShrinkPerSecond`, `float CenterX`, `float CenterZ` (both 0).

`GalaxyMap.CreateSample()` returns a map the host uses until a designer replaces it:

- storm radius 400, shrink 1 unit per second
- ship pose at x = 0, z = 300, heading 180 (toward the origin), speed 0
- location `wreck` at (0, 200), radius 25, mission `greybox`, reward `coil`
- location `planet` at (80, 80), radius 25, mission `greybox`, reward empty
- location `hostile-ship` at (-80, 120), radius 25, mission `greybox`, reward empty
- wormhole `eye` at (0, 0), radius 20, no mission
- hazard contact `radiation` at (40, 160), radius 18, unrevealed
- hostile contact `raider` at (-40, 220), radius 16, health 30, unrevealed

Hull starts at 100. Outside the storm, hull falls by 10 per second. Hostile overlap (a later plan spawns boarders) also deals 10 per second in this plan so the sim is testable before that plan lands. Document the constant on `GalaxySim` as `OutsideStormDrainPerSecond` and `HostileOverlapDrainPerSecond`.

## ShipFlight

`ShipPose Integrate(ShipPose pose, PilotCommand command, float powerLevel, float dt)`.

- power level is 0..1 and comes from `ShipManager.PowerLevel` on the host; tests pass it in
- acceleration is `8 * powerLevel * throttle` units per second squared
- speed clamps to `0..12`
- turn rate is `45 * powerLevel * turn` degrees per second
- move along the heading on the x/z plane
- throttle 0 does not brake in this slice (coasting). Negative throttle decelerates

No rigidbody. No transform.

## GalaxySim

Holds pose, hull, storm, map, phase machine, roster, latest `PilotCommand`, `CurrentPoiId`, equipment id list.

`void Tick(float dt, float powerLevel)`:

1. Integrate flight only when phase is `Docked` or `Transiting`. `MissionActive` holds the pose (the ship waits). Terminal phases hold everything.
2. Shrink the storm only while the phase is not terminal. Radius floors at 0.
3. If radius is 0 or hull is 0, `NotifyConsumed`.
4. If pose is outside the storm, drain hull.
5. If pose overlaps a living hostile, drain hull.
6. Arrival: if phase is `Transiting` and pose is inside a POI radius, set `CurrentPoiId` and `NotifyArrived`.
7. Departure: if phase is `Docked`, pose is outside the current POI, and `TryUndock` succeeds, clear `CurrentPoiId`. If undock is rejected, do not integrate further this tick; snap the pose back to the last pose that was inside the radius. Store that last-inside pose whenever the ship is inside.

`bool TryReveal(float arcDegrees, float range)` marks unrevealed contacts whose center is within `range` and within `arcDegrees` of the ship heading. Returns true if any flag flipped. Scanner plan calls this. Default arc 80, range 80.

`void DamageContact(string id, float amount)` reduces hostile health. At 0 the contact is dead and stops draining hull.

`void SetEquipment(string id)` adds an id once.

`bool HasEquipment(string id)`.

`bool TryWormhole()` calls `TryEscape` when `CurrentPoiId` is the wormhole and `HasEquipment("coil")`.

## Tests

`GalaxySimFixture`, no Photon, no scene.

- throttle forward from rest increases speed and moves along the heading
- power 0 does not accelerate or turn
- speed does not exceed 12
- storm radius decreases and floors at 0, which consumes the ship
- outside the storm, hull decreases
- arriving inside `wreck` from transit sets phase `Docked` and `CurrentPoiId`
- with a deployed crew member, leaving the radius is rejected and the pose stays inside
- with everyone aboard, leaving becomes `Transiting` and clears the poi
- `TryReveal` marks a contact in front and leaves one behind unmarked
- damaging `raider` to 0 stops the overlap drain
- wormhole without `coil` is rejected
- wormhole with `coil`, docked, all aboard, becomes `Escaped`

## Host

`EscapeSessionHost : Photon.PunBehaviour`.

- Prefab path the master instantiates: `Resources/Escape/EscapeSession.prefab`. The agent creates the prefab YAML only if a Unity editor is unavailable; a minimal prefab with this component and one `PhotonView` observing it is enough. If writing prefab YAML is unreliable, document the inspector steps in the plan's PR and still ship the component.
- `Start`: if `PhotonNetwork.inRoom && isMasterClient && photonView.isMine` is false on a duplicate, destroy the duplicate. The master instantiates when the escape level loads. Non-masters only observe.
- `Update` on the master calls `GalaxySim.Tick(Time.deltaTime, ShipManager.instance != null ? ShipManager.instance.PowerLevel : 1f)`.
- `OnPhotonSerializeView`: master writes pose x, z, heading, speed, storm radius, hull. Others read into a public `Snapshot` the view plan can poll. Keep this stream unreliable (the default observe option).
- Discrete changes (phase, current poi, revealed ids, equipment ids) are written by the master as room properties through `EscapeSessionKeys`, and only when the value changed. Join a revealed-id list with commas. Parse on `OnPhotonCustomRoomPropertiesChanged` and in `Start`.
- `SubmitPilotCommand(float throttle, float turn)` is a `[PunRPC]` sent with `PhotonNetworkSettings.EventTarget` by the helm occupant. The master stores it as the latest command. Ignore the RPC on non-masters. Do not buffer it.
- Apply the phase machine on the sim; push the phase property when `PhaseChange.Accepted` is true.

If `ShipManager.instance` is null, power is 1. Do not create a new ship singleton.

## Done when

`GalaxySimFixture` covers the cases above, and the host compiles. Do not build station cameras, the storm mesh, or the lobby entry in this plan. A debug `OnGUI` line with phase, hull, radius, and poi id is acceptable on the host and should be behind a public `bool ShowDebug` defaulting to false.
