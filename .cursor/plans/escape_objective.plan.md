---
name: Escape objective
overview: End the co-op session at the wormhole or when the storm consumes the ship, and add a lobby entry that loads the escape greybox without touching the PvP match end.
todos:
  - id: end-latch
    content: Master writes EscapePhase Escaped or Consumed once and shows a result panel
    status: pending
  - id: lobby-entry
    content: Add an escape scene choice that loads a new escape scene and does not reuse WinningTeam
    status: pending
  - id: tests
    content: Cover the wormhole equipment check and the one-shot end latch
    status: pending
isProject: false
---

# Escape objective

Depends on [galaxy_simulation.plan.md](galaxy_simulation.plan.md) and [additive_missions.plan.md](additive_missions.plan.md). The wormhole check and hull/storm consume already live on `GalaxySim`. This plan surfaces them and adds the lobby door. Do not edit hazard files.

## End conditions

Already specified, restated so this plan does not fork them:

- **Escaped.** Phase is `Docked`, current poi is `eye`, roster is all aboard, equipment contains `coil`. `GalaxySim.TryWormhole()` is the only check. Call it from the host when the pose is inside the wormhole, not from a second copy of the rule.
- **Consumed.** Hull is 0 or storm radius is 0. `GalaxySim.Tick` already calls `NotifyConsumed`.

`EscapeSessionHost` writes `EscapeSessionKeys.EscapePhase` when the sim phase becomes `Escaped` or `Consumed`, once. A `bool endLatched` on the host ignores later writes. Do not set `GameManager3D.MatchPhaseKey` or `WinningTeamKey`. Do not call `RequestMatchEnd`.

Clients show a world-space or screen panel: "Escaped" or "Consumed", and the master starts a return to the lobby after 10 seconds using the same master-only pattern as `TimedReturnToLobby` (read it, do not duplicate a second scene-sync mechanism). Non-masters follow `automaticallySyncScene`. If the escape scene has no `GameManager3D`, put the panel and the timer on `EscapeSessionHost` instead of editing `GameStateManager`.

`GameStateManager.Update` must keep watching red and blue cores only. If the escape scene does not contain those cores, do not add `GameStateManager` to it.

## Lobby

In `LobbyManager.SceneToLoadEnum` add `Escape`. `GetSceneName` returns `"Escape"`. Add `Assets/_Scenes/Escape/Escape.unity` to `ProjectSettings/EditorBuildSettings.asset` as an enabled scene.

The scene contains:

- the existing networked player spawn path the other mission scenes use (`PlayerManager3D`, marine prefabs, one `TeleportController`), copied by reference to the same prefabs, not forked
- one `ShipGreybox`
- one `EscapeSessionHost` on the `EscapeSession` prefab instance the master creates at runtime, not a second simulation
- no `GameStateManager`, no red/blue core

If copying a full mission scene would drag in a core and `GameStateManager`, build the smallest scene that can spawn one marine and then add `ShipGreybox`. Look at how `SpaceStationPve` spawns before inventing a new spawner. Reuse `PlayerManager3D.SpawnPlayer`.

`ArenaName` dropdown in the lobby must gain an Escape option wired to the enum. Inspect the lobby scene wiring. If the dropdown is populated in the inspector, update that serialized list. If you cannot safely edit the scene YAML, populate the extra option from `LobbyManager.Start` in code when the option is missing.

Launch still goes through `ShowLoadingScreen` and `PhotonNetwork.LoadLevel` **once**, to enter Escape from the lobby. That is the only legal `LoadLevel` in this mode. View changes and deployments after that must not call it.

## Tests

Add to `GalaxySimFixture` only if the wormhole cases from the galaxy plan are missing. Add `EscapeEndLatchFixture` if the latch lives in a pure helper:

- first terminal phase is published
- a later phase change is ignored
- `Consumed` does not become `Escaped` afterwards

Prefer a tiny `EndLatch` class over testing `Photon.PunBehaviour`.

## Done when

From the lobby, Escape loads, the crew can fly the sample map, pick up `coil` on the greybox, return aboard, fly to `eye`, and see Escaped. Dying to the storm shows Consumed. A red/blue mission launched from the lobby still ends on core destruction and still does not read `EscapePhase`.

## After this series

Content work is a separate pass: replace `ShipGreybox` and `MissionGreybox` with real interiors, point `ContactEffectTable` and `EquipmentPickup` at authored pathogens and equipment, and assign `BoarderPrefabName`. Do not start that pass inside this plan.
