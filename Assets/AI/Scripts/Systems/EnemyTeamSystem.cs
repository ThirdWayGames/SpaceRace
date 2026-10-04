using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class EnemyTeamSystem : ComponentSystem
{
    public struct Group
    {
        public EnemyTargetComponent Target;
        public EnemyTeamComponent Team;
        public EnemyStateComponent State;
    }

    public static Dictionary<int, Material> TeamMaterialColours;

    protected override void OnUpdate()
    {
        // Check if team material colours is null
        if (TeamMaterialColours == null)
        {
            // Get the prefab materials
            GetTeamMaterials();
        }

        foreach (var entity in GetEntities<Group>())
        {
            // If previous team is null
            if (entity.Team.PreviousTeam == null)
            {
                // Set previous team as current team
                entity.Team.PreviousTeam = entity.Team.TeamIdentifier;
            }

            // Check what team the enemy is on
            if (entity.Team.ColourIdentifier != null)
            {
                if (entity.Team.TeamIdentifier != entity.Team.PreviousTeam)
                {
                    // Check if the banner is the correct colour
                    var enemyMesh = entity.Team.ColourIdentifier.GetComponent<MeshRenderer>();
                    if (enemyMesh != null && TeamMaterialColours != null)
                    {
                        if (TeamMaterialColours.ContainsKey(entity.Team.TeamIdentifier))
                        {
                            enemyMesh.material = TeamMaterialColours[entity.Team.TeamIdentifier];
                        }
                    }

                    // Update the previous team
                    entity.Team.PreviousTeam = entity.Team.TeamIdentifier;

                    if (PhotonNetwork.isMasterClient || !PhotonNetwork.inRoom)
                    {
                        // If the current target is on the same team then set back to patrol
                        if (entity.Target.GetCurrentThreat() != null)
                        {
                            // Get the team component
                            var teamComponent = entity.Target.GetCurrentThreat().Target.GetComponent<TeamComponent>();
                            if (teamComponent != null)
                            {
                                if (teamComponent.TeamIdentifier == entity.Team.TeamIdentifier)
                                {
                                    entity.Target.GetCurrentThreat().Target = null;

                                    // Go back to patrolling
                                    entity.State.CurrentState = EnemyStateComponent.State.Patrol;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the team materials
    /// </summary>
    public void GetTeamMaterials()
    {
        if (GameManager3D.instance != null)
        {
            TeamMaterialColours = new Dictionary<int, Material>();

            // Set red
            var redMarine = GameManager3D.instance.RedMarinePrefab;
            if (redMarine != null)
            {
                var material = GetPrefabMaterial(redMarine);
                if (material != null)
                {
                    TeamMaterialColours[1] = material;
                }
            }

            // set blue
            var blueMarine = GameManager3D.instance.BlueMarinePrefab;
            if (blueMarine != null)
            {
                var material = GetPrefabMaterial(blueMarine);
                if (material != null)
                {
                    TeamMaterialColours[2] = material;
                }
            }
        }
    }

    /// <summary>
    /// Gets the material from a prefab clone
    /// </summary>
    /// <param name="prefab">The prefab</param>
    /// <returns>The material</returns>
    public Material GetPrefabMaterial(GameObject prefab)
    {
        var vfx = prefab.transform.Find("VFX");
        if (vfx != null)
        {
            var body = vfx.Find("Body");
            if (body != null)
            {
                var meshRenderer = body.GetComponent<SkinnedMeshRenderer>();
                if (meshRenderer != null)
                {
                    var materials = meshRenderer.GetComponent<SkinnedMeshRenderer>().sharedMaterials;

                    if (materials != null && materials.Any())
                    {
                        return materials[0];
                    }
                }
            }
        }

        return null;
    }
}
