using Assets.Scripts.ScriptableObjects;
using Assets.Scripts.ScriptableObjects.Pathogens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MutationController : MonoBehaviour
{
    public List<ScriptableObject> Mutations;

    public List<ScriptableObject> PathogenMutations;

    public ScriptableObject PersistentPathogen;

    public List<string> Immunities;

    public List<string> PathogenImmunities;

    // Use this for initialization
    public void Awake ()
    {
        Mutations = new List<ScriptableObject>();
        PathogenMutations = new List<ScriptableObject>();
    }

    public void Start()
    {
        if (PersistentPathogen == null)
        {
            Debug.LogWarning("There is no Persistent Pathogen defined for this mutation controller!!");
        }
    }

    /// <summary>
    ///  Adds a pathogen to the player providing that there is either no current pathogen or the serverit of the incumbent pathogen is greater
    /// </summary>
    /// <param name="pathogen">the list of mutation to add.</param>
    public void AddPathogen(Pathogen pathogen, int teamId)
    {
        // Assume that we will always apply the incumbent
        bool applyIncumbentPathogen = true;

        // If we have a current pathogen mutations and we have a current pathogen
        if (PersistentPathogen != null)
        {
            if (PathogenMutations.Any() && ((PathogenLoadout)PersistentPathogen).CurrentPathogen != string.Empty)
            {
                // If the iucumbents severity is lower than the current.
                if (pathogen.Severity <= ((PathogenLoadout)PersistentPathogen).CurrentPathogenSeverity)
                {
                    // Reset the flag to apply the incumbent.
                    applyIncumbentPathogen = false;
                }
            }
        }

        // If I am still applying the pathogen.
        if (applyIncumbentPathogen)
        {
            // Clear the existing mutations
            ClearPathogenMutations();

            // Reset the current pathogen data.
            if (PersistentPathogen != null)
            {
                ((PathogenLoadout)PersistentPathogen).CurrentPathogen = pathogen.name;
                ((PathogenLoadout)PersistentPathogen).CurrentPathogenSeverity = pathogen.Severity;
            }

            // For each mutation in the pathogen.
            foreach (var item in pathogen.SymptomMutations)
            {
                // recast item as base mutation
                var mutation = item as BaseMutation;

                // Providing the mutation can affect the host.
                if (mutation.CanApplyMutation(gameObject))
                {
                    // Create the mutation
                    var baseMutation = Instantiate(mutation);
                    baseMutation.SetMutationTeamId(teamId);

                    // Add the mutation.
                    PathogenMutations.Add(baseMutation);

                    // Initialise the mutation.
                    baseMutation.Initialise(this.gameObject);

                    // Execute the mutation
                    ExecuteMutation(baseMutation);
                }
            }
        }
    }

    /// <summary>
    ///  Adds pathogens to the mutation controller for the player if they dont have the pathogen already.
    /// </summary>
    /// <param name="pathogenTypeName">the type name of the pathogen to add.</param>
    [PunRPC]
    public void AddPathogen(string pathogenTypeName, int teamId)
    {
        // If I am not already immune to the mutation
        if (!PathogenImmunities.Any(x => x.StartsWith(pathogenTypeName)))
        {
            // Get the mutation.
            var pathogen = Resources.Load(pathogenTypeName) as Pathogen;
            AddPathogen(pathogen, teamId);
        }
    }

    /// <summary>
    /// Called when a heal bullet collides with the player. The bullet is passed to each mutation and the mutation does the appropriate logic for handling itself.
    /// </summary>
    /// <param name="healBullet">The bullet that hit the player.</param>
    public void CurePathogens(int teamId)
    {
        // Get the pathogen library for my team.
        var pathogenLib = FindObjectOfType<PathogenLibrary>();
        if (pathogenLib != null)
        {
            // Providing I have a pathogen
            if (!string.IsNullOrWhiteSpace(((PathogenLoadout)PersistentPathogen).CurrentPathogen))
            {
                // If so, cure it and make me immune from future infection.
                if (pathogenLib.CanCurePathogen(teamId, ((PathogenLoadout)PersistentPathogen).CurrentPathogen))
                {
                    if (!PathogenImmunities.Contains(((PathogenLoadout)PersistentPathogen).CurrentPathogen))
                    {
                        PathogenImmunities.Add(((PathogenLoadout)PersistentPathogen).CurrentPathogen);
                    }

                    ((PathogenLoadout)PersistentPathogen).CurrentPathogen = null;
                    ((PathogenLoadout)PersistentPathogen).CurrentPathogenSeverity = 0;
                    ClearPathogenMutations();
                }
            }
        }
        else
        {
            // No pathogen library found in scene.
            Debug.LogError("No PathogenLibrary is present in scene.");
        }
    }

    /// <summary>
    /// Cures and clears the the pathogen mutations from the pathogen mutations list.
    /// </summary>
    public void ClearPathogenMutations()
    {
        foreach (var mutation in PathogenMutations)
        {
            var baseMutation = mutation as BaseMutation;

            if (baseMutation != null)
            {
                baseMutation.CureMutation(gameObject, null);
            }
        }

        // Remove all pathogen mutations
        PathogenMutations.Clear();
    }

    /// <summary>
    ///  Adds mutations to the mutation controller for the player if they dont have the mutation already.
    /// </summary>
    /// <param name="mutations">the list of mutation to add.</param>
    public void AddMutation(List<BaseMutation> mutations, int mutationTeamId)
    {
        // For each mutation you want to add.
        foreach (var item in mutations)
        {
            if (item != null)
            {
                // Add that mutation.
                AddMutation(item.name, mutationTeamId);
            }
        }
    }

    /// <summary>
    ///  Adds mutation to the mutation controller for the player if they dont have the mutation already.
    /// </summary>
    /// <param name="mutation">the mutation to add.</param>
    [PunRPC]
    public void AddMutation(string mutationType, int mutationTeamId)
    {
        // If I am not already immune to the mutation
        if (!Immunities.Any(x => x.StartsWith(mutationType)))
        {
            // Get the mutation.
            var mutation = Resources.Load(mutationType) as BaseMutation;
            AddMutation(mutation, mutationTeamId);
        }
    }

    public void AddImmunities(List<BaseMutation> immunities)
    {
        // For each mutation you want to add.
        foreach (var item in immunities)
        {
            if (item != null)
            {
                // Add that mutation.
                AddImmunity(item.name);
            }
        }
    }

    /// <summary>
    ///  Adds immunity to the mutation controller.
    /// </summary>
    /// <param name="immunity">the mutation to add.</param>
    [PunRPC]
    public void AddImmunity(string immunity)
    {
        // If I am not already immune to the mutation
        if (!Immunities.Any(x => x.StartsWith(immunity)))
        {
            Immunities.Add(immunity);
        }
    }

    public void RemoveImmunities(List<BaseMutation> immunities)
    {
        // For each mutation you want to add.
        foreach (var item in immunities)
        {
            if (item != null)
            {
                // Add that mutation.
                RemoveImmunity(item.name);
            }
        }
    }

    /// <summary>
    ///  Removes immunity to the mutation controller.
    /// </summary>
    /// <param name="immunity">the mutation to add.</param>
    [PunRPC]
    public void RemoveImmunity(string immunity)
    {
        // If I am not already immune to the mutation
        if (Immunities.Any(x => x.StartsWith(immunity)))
        {
            Immunities.Remove(Immunities.First(x => x == immunity));
        }
    }

    /// <summary>
    /// Adds a base mutation to the list of mutations.
    /// </summary>
    /// <param name="mutation"></param>
    public void AddMutation(BaseMutation mutation, int mutationTeamId)
    {
        // Providing I am not already aflicted by the mutation.
        var existingMutation = GetMutationByPrefab(mutation);
        var canApply = mutation.CanApplyMutation(gameObject);
        if (existingMutation == null && canApply)
        {
            // Create the mutation
            var baseMutation = Instantiate(mutation);
            baseMutation.SetMutationTeamId(mutationTeamId);

            // Add the mutation.
            Mutations.Add(baseMutation);

            // Initialise the mutation.
            baseMutation.Initialise(this.gameObject);

            // Execute the mutation
            ExecuteMutation(baseMutation);
        }
    }

    /// <summary>
    /// Called when a heal bullet collides with the player. The bullet is passed to each mutation and the mutation does the appropriate logic for handling itself.
    /// </summary>
    /// <param name="healBullet">The bullet that hit the player.</param>
    public void CureMutation(BaseMutation mutation, GameObject healBullet = null)
    {
        // Get the mutation that is aflicting me
        var mutationToCure = GetMutationByPrefab(mutation, false);

        // Providing its not null and it is ready to cure (any mutation duration has passed since contracting the mutation)
        if (mutationToCure != null)
        {
            // Cure it.
            mutationToCure.CureMutation(gameObject, healBullet);

            // If it is vaccinate after cure
            if (mutationToCure.VacinateAfterCure)
            {
                // And im am not already immune
                if (!Immunities.Any(x => x == mutationToCure.name))
                {
                    // Add it to the list of immunities.
                    Immunities.Add(mutationToCure.name);
                }
            }

            // Remove the mutation from the list of mutations
            Mutations.Remove(mutationToCure);
        }
        else
        {
            // Log a warning that we attempted to cure a mutation that didn't exist.
            Debug.LogWarning(string.Format("Attempted to cure mutation '{0}' but was not found in mutation controller for '{1}'", mutation.name, this.gameObject.name));
        }
    }

    /// <summary>
    /// Cures the player of each mutation in the list of mutations to cure.
    /// </summary>
    /// <param name="mutationsToCure">The list of mutations to cure.</param>
    public void CureMutation(List<BaseMutation> mutationsToCure, GameObject healBullet = null)
    {
        // As long as I have mutations.
        if (Mutations.Any())
        {
            // For each mutation that I have to cure.
            foreach (var mutationToCure in mutationsToCure)
            {
                if (mutationToCure != null)
                {
                    // Cure it..
                    CureMutation(mutationToCure, healBullet);
                }
            }
        }
    }

    public void ExecuteMutation(BaseMutation mutation)
    {
        // Execute the mutation.
        mutation.ExecuteMutation(this.gameObject);

        // If the mutation can be cured after execution.
        if (mutation.CanCureAfterExec())
        {
            // Cure the mutation.
            CureMutation(mutation);
        }
    }

    public BaseMutation GetMutationByPrefab(BaseMutation mutationPrefab, bool? isCured = null)
    {
        // Get the mutation
        var results = Mutations.Cast<BaseMutation>().Where(x => x.name.StartsWith(mutationPrefab.name));

        // If we have specified an "IsCured" value.
        if (isCured.HasValue)
        {
            // Apply the IsCured filter.
            results = results.Where(x => x.IsCured() == isCured);
        }

        // Return the first one in the list.
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Update is called once per frame and is used to re-trigger repeatable mutations.
    /// </summary>
    public void Update ()
    {
        // So long as there are mutations.
        if (Mutations.Any())
        {
            // Itterate though each mutation.
            var castMutations = Mutations.Cast<BaseMutation>().ToArray();
            for(int i = 0; i < castMutations.Length; i++)
            {
                var mutation = castMutations[i];
                // Execute it.
                if (mutation != null)
                {
                    // Perform a double check to make sure its initialised before execution
                    if (!mutation.IsInitialised())
                    {
                        mutation.Initialise(this.gameObject);
                    }

                    ExecuteMutation(mutation);
                }
            }
        }

        // So long as there are mutations.
        if (PathogenMutations.Any())
        {
            // Itterate though each mutation.
            var castPathogens = PathogenMutations.Cast<BaseMutation>().ToArray();
            for (int i = 0; i < castPathogens.Length; i++)
            {
                var mutation = castPathogens[i];
                if (mutation != null)
                {
                    // Perform a double check to make sure its initialised before execution
                    if (!mutation.IsInitialised())
                    {
                        mutation.Initialise(this.gameObject);
                    }

                    // Execute it.
                    ExecuteMutation(mutation);
                }
            }
        }

        // If my pathogen loadout says I have a pathogen, but I dont have any pathogen mutations.
        var castPathLoadout = ((PathogenLoadout)PersistentPathogen);
        if (castPathLoadout != null && PathogenMutations != null)
        {
            if (!string.IsNullOrWhiteSpace(castPathLoadout.CurrentPathogen) && !PathogenMutations.Any())
            {
                var teamId = 0;
                var teamComp = GetComponent<TeamComponent>();
                if (teamComp != null)
                {
                    teamId = teamComp.TeamIdentifier;
                }

                // Add the pathogen.
                AddPathogen(((PathogenLoadout)PersistentPathogen).CurrentPathogen, teamId);
            }
        }
    }
}
