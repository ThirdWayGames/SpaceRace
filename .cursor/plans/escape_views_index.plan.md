---
name: Escape views index
overview: How galactic view, ship interior, and away missions share one Photon room. Read this before any of the implementation plans. Defaults are locked so agents can start; the decisions most likely to change are marked.
todos:
  - id: read-constraints
    content: Read the constraints and locked defaults before editing gameplay code
    status: pending
  - id: implement-in-order
    content: Implement escape_session_contract, then galaxy_simulation and crew_stations_and_views, then additive_missions, then hazards_and_mutations and escape_objective
    status: pending
isProject: false
---

# Escape views

The crew is one ship in a shrinking tacheon storm. They fly between authored locations, man the ship while it moves, and deploy onto wrecks, hostile ships, and planets to collect what they need for a wormhole. Aliens and environmental effects are the existing mutation system, not a new status system.

This is a new co-op session beside the current red/blue core match. Do not replace `GameStateManager`, `ShipBoardingPvp`, or `SpaceStationPve`.

## Constraints the plans are not allowed to break

Unity is **2018.2.11f1**. Networking is **PUN Classic** (`Photon.PunBehaviour`, `PhotonTargets`, `PhotonNetwork.LoadLevel`). Do not upgrade either. Do not edit `Assets/Photon Unity Networking`.

`PhotonNetwork.automaticallySyncScene` is already set in `PunNetworkManager.Awake`. `PhotonNetwork.LoadLevel` replaces the scene for **every** client (`LoadSceneMode.Single` in `Assets/Photon Unity Networking/Plugins/PhotonNetwork/PhotonNetwork.cs`). A view change that calls `LoadLevel` unloads the ship and desyncs anyone who stayed aboard.

Scene-placed `PhotonView`s are registered by that synced load. A scene loaded with `SceneManager.LoadScene(..., Additive)` does not join that registry. Away sites must not depend on scene PhotonViews. Networked actors there are `PhotonNetwork.Instantiate`d by the master. Player bodies already work this way and can be teleported.

One-shot RPCs use `PhotonNetworkSettings.EventTarget` (`AllViaServer`). Current-value state uses room properties or one unreliable `OnPhotonSerializeView` stream. Do not add `AllBuffered` traffic. The failure mode is documented in [network_and_match_logic_e5983254.plan.md](network_and_match_logic_e5983254.plan.md).

New C# stays C# 6. No tuples, `out var`, discards, or nullable reference types. Match the style of `Assets/Scripts/GameManager3D.cs`.

Pure rules (phase, roster, flight, storm, reveal) do not call `PhotonNetwork`. NUnit fixtures live in `Assets/Tests` and use `[Test]`, same as `MutationControllerFixture`.

## Three layers, one room

```text
Photon room
  Session object (master-owned, DontDestroyOnLoad, one PhotonView)
    phase, ship pose, storm, contacts, roster, equipment
  Ship interior (the synced level)
    player bodies, stations, mutations, alarm
  Galaxy presentation (local only, no PhotonView, parked at y = 5000)
    proxy ship, storm disc, markers, driven by the replicated pose
  Away site (local prefab instance, spawned while a mission is active)
    geometry and volumes only; actors instantiated by the master
```

The interior is **not** parented to the galaxy proxy. Walking, rigidbodies, and Photon transforms stay in interior space. The galaxy is a 2D simulation (x, z, heading, speed). Each client draws a local proxy from the master's snapshot.

"Galactic view" and "ship view" are **per player**. A body stays in the interior. Sitting at a station calls the existing `BasePlayerController3D.SetDisableMovement(true)` and enables that station's camera. Standing up restores the player camera. One player can fly while another walks to a turret. A global camera swap cannot do that.

"All players aboard" gates **leaving a location** and **the wormhole**. It does not gate the camera. Dead players count as aboard; they already respawn on the ship via `PlayerManager3D`.

## Stations

| Station | Who may sit | What it writes |
| --- | --- | --- |
| Helm | one aboard player | latest `PilotCommand` (throttle, turn). Master integrates. |
| Scanner | one aboard player | reveals contacts in an arc. Helm draws revealed contacts only. |
| Turret | one aboard player | damages the nearest revealed hostile in the facing arc. |
| Airlock | one aboard player | deploy onto the current location, or recall. |

Unrevealed hazards still hit the ship. Shooting a hostile down stops it boarding. A hostile that reaches the ship raises the existing `ShipManager` alarm (`AlarmStateKey`) and the master instantiates a boarder in the interior. Interior fighting stays the current player weapons.

## Phases

| Phase | Meaning | Deploy | Fly away |
| --- | --- | --- | --- |
| `Docked` | inside a location radius, nobody deployed | yes | yes, and the phase becomes `Transiting` |
| `Transiting` | between locations | no | already flying |
| `MissionActive` | at least one player `Deployed` | yes, more may join | no |
| `Escaped` | wormhole with the required equipment, everyone aboard | no | no |
| `Consumed` | hull reached zero, or the storm radius reached zero | no | no |

`Escaped` and `Consumed` latch once, the same way `MatchPhase = Ended` already does. They are a separate room key. Do not write `WinningTeam`.

## Replication

| Data | Channel | Why |
| --- | --- | --- |
| Pose, speed, heading, storm radius, hull | unreliable `OnPhotonSerializeView` on the session object | changes every tick |
| Phase, active location, revealed contact ids, equipment ids, active mission id | room properties | discrete, late joiners need the current value |
| Crew location | player custom property, master writes it | same pattern as `PlayerTeam` |
| Pilot command | `EventTarget` RPC at about 10 Hz while the helm is occupied, master keeps the latest | the session view is master-owned, so the pilot cannot serialize it |
| Alarm | existing `ShipManager.AlarmStateKey` | already a room property |
| Turret fire and scanner ping sounds | `EventTarget` | one-shot |

The master creates one `EscapeSession` prefab from `Resources` with `PhotonNetwork.Instantiate`. Clients do not create a second one.

## Away sites

The first away site is a **new greybox prefab** under `Assets/Resources/Escape/`, instantiated locally when the room property `ActiveMissionId` is set. Do not additive-load `SpaceStationPve` or `ShipBoardingPvp`. Those scenes contain `GameManager3D`, spawn loops, and scene PhotonViews. A later content pass can extract art from them after those singletons are stripped.

Every client, including a late joiner, reads `ActiveMissionId` and instantiates the same prefab. When the last deployed player recalls, the master clears the property and clients destroy the instance.

## Mutations

Hazards and aliens carry a pathogen resource name or a spawn prefab name. Overlap with a hazard applies hull damage and, for a boarding contact, the alarm plus a spawn. A volume on an away site calls the existing owner-authoritative `MutationController.AddPathogen`. Do not add mutation subclasses unless a real gap shows up. Existing pieces: `ZoneMutation`, `EntitySpawnMutation`, `StatModMutation`, `FxMutation`, `Pathogen`.

## Order

1. [escape_session_contract.plan.md](escape_session_contract.plan.md) — types, roster, phase machine, tests. No scenes.
2. [galaxy_simulation.plan.md](galaxy_simulation.plan.md) and [crew_stations_and_views.plan.md](crew_stations_and_views.plan.md) — after the contract. They share `ShipPose` and `PilotCommand` and should not edit each other's files.
3. [additive_missions.plan.md](additive_missions.plan.md) — after stations (airlock) and the contract.
4. [hazards_and_mutations.plan.md](hazards_and_mutations.plan.md) and [escape_objective.plan.md](escape_objective.plan.md) — after the sim and missions. Objective work must not change hazard files, and hazard work must not change the lobby.

## Locked defaults

These are the answers assumed by every plan. Change the plan named in the last column if a default is wrong. Do not invent a different architecture inside an implementation plan.

| Decision | Default | If this is wrong, change |
| --- | --- | --- |
| View ownership | Per player, by sitting at a station | `crew_stations_and_views` |
| Split crew | Some may deploy while others fly and shoot. Everyone aboard to undock or jump | `additive_missions`, `escape_objective` |
| Galaxy shape | Flat disc. Heading, throttle, speed cap. `ShipManager.PowerLevel` scales acceleration | `galaxy_simulation` |
| Storm | Circle shrinks toward the origin. Outside it, hull drains. Radius 0 consumes the ship | `galaxy_simulation`, `escape_objective` |
| Scanner | Contacts have a revealed flag. Helm sees revealed only. Unrevealed still collide | `galaxy_simulation`, `crew_stations_and_views` |
| Turrets | External only in this slice. Interior defense is current FPS combat plus a spawned boarder | `hazards_and_mutations` |
| Mode | New co-op escape entry. Red/blue PvP stays | `escape_objective` |
| First content | 4 locations, 1 hostile, 1 hazard, 1 greybox away site, 1 equipment flag, 1 wormhole | content pass after these plans |
| Engine | Unity 2018.2, PUN Classic | all of them |

## Out of scope for this whole series

Procedural galaxies, 3D Newtonian flight, a second ship, PvP inside the escape mode, PUN 2, ECS gameplay, new mini-game rules, and replacing the ship art.
