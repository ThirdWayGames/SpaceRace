---
name: Hazards and mutations
overview: Galaxy contacts and away-site volumes drive the existing mutation and alarm paths. No new mutation subclasses and no new combat model.
todos:
  - id: contact-effects
    content: Map hazard and hostile contacts onto hull drain, alarm, and one interior boarder spawn
    status: pending
  - id: site-volume
    content: Add a HazardVolume on the greybox that calls MutationController.AddPathogen for the owner
    status: pending
  - id: tests
    content: Test which contact state requests boarding versus silent hull drain
    status: pending
isProject: false
---

# Hazards and mutations

Depends on [galaxy_simulation.plan.md](galaxy_simulation.plan.md) and [additive_missions.plan.md](additive_missions.plan.md). Read the mutation rules in [escape_views_index.plan.md](escape_views_index.plan.md).

Do not add a subclass of `BaseMutation`. Do not change `MutationController` authority. `AddPathogen` already applies on the owning client only.

## Files

Create:

- `Assets/Scripts/Session/Hazards/ContactEffect.cs`
- `Assets/Scripts/Session/Hazards/ContactEffectTable.cs`
- `Assets/Scripts/Session/Hazards/HazardVolume.cs`
- `Assets/Tests/ContactEffectFixture.cs`

Edit only `EscapeSessionHost` (or a new `HazardPresenter` it calls) to apply `ContactEffect` results. Do not edit `GalaxySim` rules except to read contact state that already exists. If the sim does not expose "overlapped this tick", add `IList<string> Overlaps()` that returns contact ids the pose currently intersects. That is a query, not a new rule.

## Contact effects

`ContactEffect` is a plain class:

- `string ContactId`
- `float HullDrainPerSecond` (already applied by the sim for hostiles and the storm; this plan must not drain twice)
- `bool RaiseAlarm`
- `bool SpawnBoarder`
- `string PathogenResourceName` (empty when none)

`ContactEffectTable.Resolve(GalaxyContact contact, bool overlapping)`:

| Kind | Overlapping | Revealed | Result |
| --- | --- | --- | --- |
| Hazard | yes | either | no extra hull drain (storm and hostile drains stay in the sim), `RaiseAlarm` false, pathogen name `Anthrax` applied once per overlap entry to **aboard** players |
| Hostile, health > 0 | yes | either | `RaiseAlarm` true, `SpawnBoarder` true |
| Hostile, health <= 0 | yes | either | no alarm, no spawn |
| either | no | either | nothing |

Anthrax is already loaded in tests via `Resources.Load("Anthrax")`. Use it as the stand-in environmental pathogen. A designer can change the string on the table later. Apply at most once per player per contact id per visit (track ids in a set on the presenter, clear an id when the ship stops overlapping it).

The sim already drains hull for hostile overlap and for the storm. Do not add that drain again here.

## Presenter

On the master, after `Tick`:

- For each new hostile overlap, set the alarm with the existing `ShipManager` room property path (`GameManager3D.SetAlarmActive(true)` if that writes `AlarmStateKey`; otherwise set the room property the same way `SetAlarmActive` does). Read `SetAlarmActive` before calling it. Do not RPC `RaiseAlarm` in a loop.
- Spawn at most one boarder per hostile contact per visit. `PhotonNetwork.Instantiate` an existing enemy prefab if one is already assigned on a public field `BoarderPrefabName` (default empty). If the field is empty, skip the instantiate and log once. Do not guess a prefab path. The boarder spawn position is the ship greybox spawn, offset by 2 metres. Clearing the overlap allows one future spawn.
- When the hostile dies or the overlap ends, call `SetAlarmActive(false)` only if no other overlapping hostile still wants the alarm.
- Pathogen: master sends the existing `AddPathogen` RPC to each aboard player's `MutationController`, once per visit. Those RPCs already no-op unless they run on the owner, so send them such that the owner applies them. Look at the current `AddPathogen` call sites and use the same target. Do not switch it to `AllBuffered`.

## Away-site volume

`HazardVolume : MonoBehaviour` with `string PathogenResourceName = "Anthrax"` and a trigger collider on the greybox, placed off the spawn pad so deploy does not infect immediately.

`OnTriggerEnter`: if the collider is the local player and `photonView.isMine` on that player, call `MutationController.AddPathogen(PathogenResourceName, teamId)` on that player. Team id comes from `BasePlayerController3D.GetTeamId`. This is local application through the existing method, which is already owner-gated when invoked as an RPC. Call the RPC method so the gate stays in one place, targeted at the owner (`PhotonTargets.Others` is wrong; the owner must execute it). Use the local call to the non-RPC `AddPathogen(Pathogen, int)` if the RPC would not run locally. Do not duplicate the immunity and severity rules.

## Tests

`ContactEffectFixture`:

- overlapping hazard requests the pathogen and does not request a boarder
- overlapping living hostile requests alarm and a boarder
- overlapping dead hostile requests neither
- non-overlapping contact requests nothing
- a second resolve while the presenter-side "already spawned" flag is set does not request another boarder (test that flag as a pure `BoarderVisits` helper with `bool TryBeginVisit(string id)` / `EndVisit(string id)`)

## Done when

The fixture passes, the greybox has a hazard trigger away from the spawn, and boarding cannot fire more than once per overlap visit. If no boarder prefab is wired, the log-once path is the accepted result. Do not import a new enemy asset.
