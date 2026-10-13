---
name: Additive missions
overview: Deploy some of the crew onto a greybox away site while the rest stay aboard, without PhotonNetwork.LoadLevel and without scene PhotonViews.
todos:
  - id: site-prefab
    content: Add a mission greybox prefab with spawn and recall markers and no PhotonViews
    status: pending
  - id: session-sync
    content: Master writes ActiveMissionId; every client including late joiners instantiates or destroys the prefab
    status: pending
  - id: deploy-recall
    content: Airlock deploys and recalls through the phase machine and CrewLocation player property
    status: pending
  - id: tests
    content: Test deploy gating and the mission-id lifecycle in pure code
    status: pending
isProject: false
---

# Additive missions

Depends on [escape_session_contract.plan.md](escape_session_contract.plan.md) and the airlock in [crew_stations_and_views.plan.md](crew_stations_and_views.plan.md). Uses `CurrentPoiId`, `GalaxyMap` mission ids, and `EscapePhaseMachine` from the galaxy plan.

Do not call `PhotonNetwork.LoadLevel` or `SceneManager.LoadScene`. Do not additive-load `SpaceStationPve` or `ShipBoardingPvp`. Do not put a `PhotonView` on the away-site prefab.

## Files

Create:

- `Assets/Scripts/Session/Missions/MissionCatalog.cs`
- `Assets/Scripts/Session/Missions/MissionSession.cs`
- `Assets/Scripts/Session/Missions/MissionSite.cs`
- `Assets/Resources/Escape/MissionGreybox.prefab`
- `Assets/Tests/MissionSessionFixture.cs`

`MissionCatalog` maps mission id `greybox` to resource path `Escape/MissionGreybox`. Pure dictionary. The sample POIs in `GalaxyMap.CreateSample` already use that id.

## MissionSession

Pure logic the host calls. Holds the phase machine and roster passed in, plus `string ActiveMissionId`.

- `DeployResult TryDeploy(int playerId, string currentPoiMissionId)`
  - reject if `currentPoiMissionId` is empty (in transit, or a wormhole)
  - reject if the phase machine rejects deploy
  - set that player `Deployed`
  - set `ActiveMissionId` to the poi mission id
  - accept
- `DeployResult TryRecall(int playerId)`
  - set that player `Aboard`
  - if nobody remains deployed, clear `ActiveMissionId` and let the phase machine return to `Docked`
- `string MissionToLoad()` returns the active id, or empty

`DeployResult` is a class with `bool Accepted`, `string MissionId`, `EscapePhase Phase`.

Tests:

- deploy in transit is rejected and the mission id stays empty
- deploy while docked at a poi with mission `greybox` accepts, phase `MissionActive`, id `greybox`
- a second player can deploy onto the same id
- recalling one of two leaves the id set and the phase `MissionActive`
- recalling the last clears the id and returns `Docked`
- deploy at the wormhole (empty mission id) is rejected

## Site prefab

`MissionSite` on the prefab root:

- `Transform DeploySpawn`
- `Transform RecallVolume` (a trigger)
- children built in the prefab or, if prefab YAML is not safe to author, built in `MissionSite.BuildGreybox()` from primitives: a floor, a light, a spawn pad, a recall pad, and a pickup marker named `EquipmentPickup`

No `GameManager3D`, no `PlayerManager3D`, no `ShipManager`, no `PhotonView`, no audio listener.

The pickup marker has a component `EquipmentPickup` with `string EquipmentId = "coil"`. On local-player trigger it asks the master, via `EventTarget`, to `GalaxySim.SetEquipment`. The master checks the player is `Deployed` before accepting. This is the one equipment grant in the slice. The objective plan reads the flag; do not also check the wormhole here.

## Who instantiates

`EscapeSessionHost`, or a `MissionPresenter` beside it, watches `EscapeSessionKeys.ActiveMissionId`.

- On change to a non-empty id, `Resources.Load` the catalog path and `Instantiate` locally under a `MissionRoot` at `(0, 0, 200)` so it does not overlap the ship greybox. Same instance on every client.
- On change to empty, `Destroy` that instance.
- `Start` applies the property if it is already set, which covers late joiners.
- Do not `PhotonNetwork.Instantiate` the site. Geometry is cosmetic and identical. The equipment grant is an RPC, so it does not need a scene view.

## Deploy and recall flow

Airlock, when `DeployRequested` and the local player is aboard:

1. Owning client sends `EventTarget` `RequestDeploy` to the master.
2. Master runs `MissionSession.TryDeploy` with the current poi's `MissionId` and the actor id.
3. On accept, master writes player custom property `EscapeSessionKeys.CrewLocation` = `Deployed`, writes `ActiveMissionId` if it changed, and writes the phase property if it changed.
4. The owning client, when it sees its own property become `Deployed` and the site exists, teleports its body to `DeploySpawn`. It owns the player view, so this is a normal transform write. Disable the station camera first (`TryStand`).

Recall: the deployed owner enters `RecallVolume` and presses `E`, or uses a recall control that sends `RequestRecall`. Master sets `CrewLocation` to `Aboard`. Owner teleports to the ship spawn marker on `ShipGreybox`. If that was the last deployed player, master clears `ActiveMissionId`.

Reject deploy when the site is already active for a **different** mission id. The sample map only has `greybox`, so this is a guard.

Players in `Deployed` are not all-aboard, so `GalaxySim` already refuses to leave the poi radius. Do not add a second gate.

## Done when

`MissionSessionFixture` passes. A manual script covers: helm parks inside `wreck`, one player deploys and appears on the pad, the other stays on the ship and can still sit the scanner, the deployed player takes the coil and recalls, the site instance is destroyed, and the ship can then undock. State that you could not run two editors if that is true, and list the RPCs you verified by reading the host.
