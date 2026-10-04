using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Trigger Applicator keeps track of targets within the collider that require 
/// actions to be performed on them
/// </summary>
public abstract class TriggerTargetManager : MonoBehaviour, IDisposeCallback
{
    //The list of colliders currently inside the trigger
    protected List<Collider> TriggerList;

    public virtual void Awake()
    {
        TriggerList = new List<Collider>();
    }

    // Called when the item is removed by the DestroyMe script.
    public void DisposeItem(float currentTime)
    {
        for(int i = 0; i < TriggerList.Count; i++)
        {
            OnTriggerExit(TriggerList[0]);
        }
    }

    // get the MutationController from the player and add all mutations.
    public virtual void OnTriggerStay(Collider other) { }

    public virtual void OnTriggerExit(Collider other) { }
}
