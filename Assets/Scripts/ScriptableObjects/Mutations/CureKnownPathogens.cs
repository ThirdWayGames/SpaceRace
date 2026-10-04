using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutations/CurePathogens")]
[Serializable]
public class CureKnownPathogens : BaseMutation, IBaseMutation
{
    protected override void ExecutionTick(GameObject parentGameObject)
    {
        var mutationController = parentGameObject.GetComponent<MutationController>();
        if (mutationController != null)
        {
            mutationController.CurePathogens(this.MutationTeamId);
        }
    }
}