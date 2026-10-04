using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Mutations/Component/Container Mutators/Sprite")]
[Serializable]
public class ImageMutator : BasePropertyMutation<Sprite>
{
    public override void Mutate(GameObject parent)
    {
        var component = parent.GetComponent(ComponentToAffect);
        if (component != null)
        {
            // Try updating a field
            var field = component.GetType().GetFields().FirstOrDefault(x => x.Name.ToLowerInvariant() == Property.ToLowerInvariant());
            if (field != null)
            {
                // Store the initial value state.
                if (ImmutableValue == null)
                {
                    ImmutableValue = field.GetValue(component);
                }

                var value = Convert.ChangeType(PropertyValue, field.FieldType);
                if (value != null)
                {
                    // If its a numeric value
                    field.SetValue(component, value);
                }
            }
        }
        else
        {
            Debug.LogErrorFormat("'{0}' is not a component of '{1}'", ComponentToAffect, parent.name);
        }
    }
}
