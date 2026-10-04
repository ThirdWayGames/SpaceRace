using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutations/Component/Container")]
[Serializable]
public class ComponentPropertyMutation : RepeatableMutation
{
    protected Component ActualComponent;

    public List<ScriptableObject> PropertyMutations;

    protected override void ExecutionTick(GameObject parentGameObject)
    {
        PropertyMutations.Cast<IPropertyMutation>().ToList().ForEach(x => x.Mutate(parentGameObject));
    }

    public override void CureMutation(GameObject parentGameObject, GameObject gameObject)
    {
        // Perform base call to determine if it is cured.
        base.CureMutation(parentGameObject, gameObject);

        // If it is cured.
        if (isCured)
        {
            // If we have executed the mutation at least once and we are to reinstate the values after cure.
            if (CurrentExecCount > 0 && ReinstateValuesAfterCure)
            {
                PropertyMutations.Cast<IPropertyMutation>().ToList().ForEach(x => x.Unmutate(parentGameObject));
            }
        }
    }
}