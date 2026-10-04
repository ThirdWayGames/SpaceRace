using UnityEngine;
using System.Collections.Generic;

public class EquipmentMutationApplicator : MonoBehaviour
{
    public List<ScriptableObject> MutationsToApplyWhenEquiped;
    public List<ScriptableObject> ImmunitiesToApplyWhenEquiped;

    public List<ScriptableObject> MutationsToRemoveWhenEquiped;
    public List<ScriptableObject> ImmunitiesToRemoveWhenEquiped;

    public List<ScriptableObject> MutationsToApplyWhenUnequiped;
    public List<ScriptableObject> ImmunitiesToApplyWhenUnequiped;

    public List<ScriptableObject> MutationsToCureWhenUnequiped;
    public List<ScriptableObject> ImmunitiesToCureWhenUnequiped;
}