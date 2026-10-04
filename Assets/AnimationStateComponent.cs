using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationStateComponent : MonoBehaviour
{
    protected Animator Animator { get; set; }

    protected List<AnimatorControllerParameter> animatorParams { get; set; }

    void Awake()
    {
        Animator = GetComponent<Animator>();
        animatorParams = Animator.parameters.ToList();
        if (animatorParams == null || !animatorParams.Any())
        {
            Debug.LogWarningFormat("'{0}' Doesn't contain any parameters", Animator.name);
        }
    }

    public void SetState(string paramName, string paramValue)
    {
        var animatorParam = animatorParams.FirstOrDefault(x => x.name == paramName);
        if (animatorParam == null)
        {
            Debug.LogWarningFormat("{0} doesn't contain the parameter '{1}'", Animator.name, paramName);
        }

        switch (animatorParam.type)
        {
            case AnimatorControllerParameterType.Bool:
                {
                    bool castParamValue;
                    if (bool.TryParse(paramValue, out castParamValue))
                    {
                        if (Animator.GetBool(paramName) != castParamValue)
                        {
                            Animator.SetBool(paramName, castParamValue);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("{0} is not of type 'bool'", paramValue);
                    }

                    break;
                }
            case AnimatorControllerParameterType.Float:
                {
                    float castParamValue;
                    if (float.TryParse(paramValue, out castParamValue))
                    {
                        if (Animator.GetFloat(paramName) != castParamValue)
                        {
                            Animator.SetFloat(paramName, castParamValue);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("{0} is not of type 'float'", paramValue);
                    }

                    break;
                }
            case AnimatorControllerParameterType.Int:
                {
                    int castParamValue;
                    if (int.TryParse(paramValue, out castParamValue))
                    {
                        if (Animator.GetInteger(paramName) != castParamValue)
                        {
                            Animator.SetInteger(paramName, castParamValue);
                        }
                    }
                    else
                    {
                        Debug.LogWarningFormat("{0} is not of type 'int'", paramValue);
                    }

                    break;
                }
            case AnimatorControllerParameterType.Trigger:
                {
                    Animator.SetTrigger(paramName);
                    break;
                }
        }
    }

    /// <summary>
    /// Sets the weight of a specific layer.
    /// </summary>
    /// <param name="layerName">The name of the layer</param>
    /// <param name="layerWeight">The weight to set the layer to.</param>
    public void SetLayerWeight(string layerName, float layerWeight)
    {
        var layerIndex = Animator.GetLayerIndex(layerName);
        if (layerIndex < 0)
        {
            Debug.LogWarningFormat("No layer with name '{0}' exists on {1}", layerName, Animator.name);
        }
        else
        {
            if (Animator.GetLayerWeight(layerIndex) != layerWeight)
            {
                Animator.SetLayerWeight(layerIndex, layerWeight);
            }
        }
    }
}
