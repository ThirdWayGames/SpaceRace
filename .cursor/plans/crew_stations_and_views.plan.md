---
name: Crew stations and views
overview: Per-player helm, scanner, turret, and airlock cameras inside the ship while a local galaxy proxy draws the master's pose. The player body never enters galaxy space.
todos:
  - id: stations
    content: Add CrewStation occupancy and ViewRouter camera swap using SetDisableMovement
    status: pending
  - id: proxy
    content: Spawn a local galaxy proxy at y=5000 from the session snapshot
    status: pending
  - id: input
    content: Helm sends PilotCommand at 10 Hz, scanner calls TryReveal, turret damages the nearest revealed hostile in arc
    status: pending
  - id: greybox-ship
    content: Add a runtime ship greybox with four stations so the mode can run without a hand-built Unity scene
    status: pending
isProject: false
---

# Crew stations and views

Depends on [escape_session_contract.plan.md](escape_session_contract.plan.md). Reads `EscapeSessionHost.Snapshot`, `SubmitPilotCommand`, `TryReveal`, and `DamageContact` from [galaxy_simulation.plan.md](galaxy_simulation.plan.md). If that host is not merged yet, call those methods through a small `IGalaxySession` in `Assets/Scripts/Session/Galaxy/IGalaxySession.cs` and adapt `EscapeSessionHost` to it without changing sim rules.

Do not load missions. Do not edit `LobbyManager`. The airlock station records an intent the mission plan will honor; in this plan the intent only logs.

## Files

Create:

- `Assets/Scripts/Session/Stations/IGalaxySession.cs`
- `Assets/Scripts/Session/Stations/CrewStation.cs`
- `Assets/Scripts/Session/Stations/StationOccupancy.cs`
- `Assets/Scripts/Session/Stations/ViewRouter.cs`
- `Assets/Scripts/Session/Stations/GalaxyProxy.cs`
- `Assets/Scripts/Session/Stations/ShipGreybox.cs`
- `Assets/Tests/StationOccupancyFixture.cs`

## Occupancy

`StationOccupancy` is pure: station id to player id, one player per station, one station per player.

- `bool TrySit(string stationId, int playerId)`
- `bool TryStand(int playerId)`
- `string StationFor(int playerId)` empty when standing

Reject a sit when the station or the player is taken. Tests cover both.

The master is the authority. `CrewStation` sends a sit/stand request with `PhotonNetworkSettings.EventTarget`. The master updates occupancy and writes room property `StationSeats` as `stationId:actorId` pairs joined by commas, using the same changed-only pattern as the galaxy host. Clients apply that property locally. Do not buffer sit RPCs.

## CrewStation

MonoBehaviour on an empty object in the interior.

- `StationRole Role`
- `Transform Seat`
- `Transform CameraAnchor`
- `string StationId`

Interaction uses the existing raycast path if it is easy (`IRayCastActionReceiver`); otherwise `OnTriggerStay` plus the `E` key is acceptable for the greybox. Say which one you used in the PR.

On sit, the occupying client:

- calls `BasePlayerController3D.SetDisableMovement(true)` on the local body
- moves the body to `Seat` (position and yaw only)
- `ViewRouter` disables the current player camera and enables a camera at `CameraAnchor`

On stand, reverse both. Only the local client swaps cameras. Remote bodies stay where the master seat transform says; the owning client sets the transform because it owns the player PhotonView.

## ViewRouter

Local component on the player, or a single object that finds the local `PlayerManager3D.LocalPlayerInstance`.

- `OnFoot` uses the camera already on the marine. Do not destroy it.
- `Station` enables one extra `Camera` parented to the anchor, with a higher depth, and disables the marine camera and its `AudioListener` duplicate. Only one listener stays active.
- Galactic anchors point at the proxy. The on-foot camera must not see the proxy: put proxy objects on a dedicated layer `GalaxyView` and remove that layer from the marine camera culling mask. Station cameras render `GalaxyView` plus nothing from the interior. If adding a layer in `TagManager` is too invasive, parent the proxy at `y = 5000` and set the marine camera far-clip so it cannot see that far, and the station camera far-clip so it can. Prefer the layer.

## GalaxyProxy

Local only. No `PhotonView`. `GalaxyProxy` creates primitives at runtime under one root at `(0, 5000, 0)`:

- a flat cylinder for the storm, scaled to the snapshot radius
- a cube for the ship, posed from snapshot x, z, heading
- a sphere per POI, tinted differently for the wormhole
- a sphere per contact, hidden until `Revealed`

It polls `IGalaxySession` each frame. Missing session means the proxy hides itself. Do not simulate flight here.

## Input per role

While the local player occupies the station, `Update` on that station:

- **Helm.** Read `Horizontal` as turn and `Vertical` as throttle. Send `SubmitPilotCommand` at most every 0.1 seconds, and when the values change. On stand, send throttle 0, turn 0 once.
- **Scanner.** Every 0.5 seconds, if this client is master, call `TryReveal(80, 80)`. If this client is not master, send an `EventTarget` RPC the master handles by calling `TryReveal`. A revealed contact stays revealed.
- **Turret.** Every 0.25 seconds while the fire button (`Fire1`) is held, ask the master to `DamageContact` the nearest revealed hostile within 60 units and 15 degrees of the ship heading, for 5 damage. Master ignores the call if this player does not occupy the turret. One `EventTarget` sound RPC per shot is enough; do not spawn a networked projectile in this slice.
- **Airlock.** Sets a public `bool DeployRequested` on the station for the mission plan. This plan does not teleport anyone.

Helm HUD, for the local helm occupant only: phase, hull, storm radius, current poi, and revealed contact ids. `OnGUI` is enough. Scanner occupant sees the same list plus unrevealed count (a number, not positions).

## Ship greybox

`ShipGreybox` builds a floor, four labeled station objects, and a player spawn marker at runtime when a scene contains it and no stations exist yet. Put one in a new scene only if you can do that without breaking `EditorBuildSettings`. Otherwise add the component to a prefab `Assets/Resources/Escape/ShipGreybox.prefab` and document that the lobby plan instantiates it. Do not modify `SpaceStationPve.unity` or `ShipBoardingPvp.unity`.

Stations sit a few metres apart so a player can walk between them with `PlayerController3D`.

## Done when

Occupancy tests pass. In a two-client play test (or a written manual script if a second client cannot be launched): one client sits helm and the proxy ship moves; the other walks between stations; standing restores the marine camera; a scanner sit reveals the forward contact; the marine camera does not show the proxy. Do not require the mission prefab.
