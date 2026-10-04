using Assets.Scripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class BasePropertyMutation<T> : ScriptableObject, IPropertyMutation
{
    public Type MutatingType;

    public string ComponentToAffect;

    public string Property;

    public T PropertyValue;

    public bool AddPropertyValue;

    public bool ReinstateAfterCure;

    protected object ImmutableValue;

    protected Component ActualComponent;

    public virtual void Unmutate(GameObject parent)
    {
        var component = parent.GetComponent(ComponentToAffect);
        if (component != null)
        {
            var field = component.GetType().GetFields().FirstOrDefault(x => x.Name.ToLowerInvariant() == Property.ToLowerInvariant());
            if (field != null)
            {
                // Store the initial value state.
                if (ImmutableValue != null && ReinstateAfterCure)
                {
                    field.SetValue(component, ImmutableValue);
                }
            }
            else
            {
                var property = component.GetType().GetProperties().FirstOrDefault(x => x.Name.ToLowerInvariant() == Property.ToLowerInvariant());
                if (property != null)
                {
                    // Store the initial value state.
                    if (ImmutableValue != null && ReinstateAfterCure)
                    {
                        property.SetValue(component, ImmutableValue);
                    }
                }
            }
        }
        else
        {
            Debug.LogErrorFormat("'{0}' is not a component of '{1}'", ComponentToAffect, parent.name);
        }
    }

    public virtual void Mutate(GameObject parent)
    {
        var component = parent.GetComponent(ComponentToAffect);
        if (component != null)
        { 
            HashSet<Type> NumericTypes = new HashSet<Type>
            {
                typeof(decimal), typeof(byte), typeof(sbyte),
                typeof(short), typeof(ushort), typeof(float),
                typeof(int), typeof(Int32), typeof(Int64),
                typeof(double), typeof(Double), typeof(long)
            };

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
                    if (NumericTypes.Contains(field.FieldType))
                    {
                        if (AddPropertyValue)
                        {
                            field.SetValue(component, (float)field.GetValue(component) + (float)value);
                        }
                        else
                        {
                            field.SetValue(component, value);
                        }
                    }
                    else
                    {
                        field.SetValue(component, value);
                    }
                }
            }
            else
            {
                // Try updating a property
                var property = component.GetType().GetProperties().FirstOrDefault(x => x.Name.ToLowerInvariant() == Property.ToLowerInvariant());
                if (property != null)
                {
                    // Store the initial value state.
                    if (ImmutableValue == null)
                    {
                        ImmutableValue = property.GetValue(component);
                    }

                    var value = Convert.ChangeType(PropertyValue, property.PropertyType);
                    if (value != null)
                    {
                        // If its a numeric value
                        if (NumericTypes.Contains(property.PropertyType))
                        {
                            if (AddPropertyValue)
                            {
                                property.SetValue(component, (float)property.GetValue(component) + (float)value);
                            }
                            else
                            {
                                property.SetValue(component, value);
                            }
                        }
                        else
                        {
                            property.SetValue(component, value);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogErrorFormat("'{0}' is not a component of '{1}'", ComponentToAffect, parent.name);
        }
    }
}
