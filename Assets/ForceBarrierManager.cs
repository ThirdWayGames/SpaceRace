using Assets.Scripts.ScriptableObjects;
using Photon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class ForceBarrierManager : PunBehaviour
{
    public List<EventTriggerVariable> TriggerEvents;

    public List<ForceBarrierController> ForceBarriers;

    public List<ForceBarrierController> ConsoleBarriers;

    public int MaxTriggersAllowed;

    private int currentTriggerCount;

    void Start ()
    {
        currentTriggerCount = 0;

        if (TriggerEvents.Any())
        {
            foreach (var trigger in TriggerEvents)
            {
                trigger.TriggerEvent.AddListener(ListenerTriggered);
            }
        }

        if (PhotonNetwork.isMasterClient || GameManager3D.instance.IsDebug)
        {
            foreach (var barrierToShutOff in GetRandomLockedConsoles())
            {
                // Shut it off.
                if (PhotonNetwork.inRoom)
                {
                    photonView.RPC("RemoveConsoleBarrier", PhotonNetworkSettings.DefaultRPCNetworkTarget, new object[] { barrierToShutOff.gameObject.name });
                }
                else
                {
                    RemoveConsoleBarrier(barrierToShutOff.gameObject.name);
                }
            }
        }
    }

    [PunRPC]
    public void RemoveConsoleBarrier(string barrierToRemove)
    {
        var barrierToShutOff = ConsoleBarriers.FirstOrDefault(x => x.name == barrierToRemove);
        if (barrierToShutOff != null)
        {
            ConsoleBarriers.Remove(barrierToShutOff);
            Object.Destroy(barrierToShutOff.gameObject);
        }
    }

    [PunRPC]
    public void RemoveForceBarrier(string barrierToRemove)
    {
        var barrierToShutOff = ForceBarriers.FirstOrDefault(x => x.name == barrierToRemove);
        if (barrierToShutOff != null)
        {
            ForceBarriers.Remove(barrierToShutOff);
            Object.Destroy(barrierToShutOff.gameObject);
            currentTriggerCount += 1;
        }
    }

    public void ListenerTriggered(FloatTrigger triggerResult)
    {
        // If we are allowed to trigger events
        if (currentTriggerCount < MaxTriggersAllowed && triggerResult.value > 0)
        {
            // Try and select a specific barrier based on the console that was unlocked.
            var tempIndex = -1;
            switch (triggerResult.triggeredBy.name)
            {
                case "SecurityConsole (1)":
                    {
                        tempIndex = ForceBarriers.FindIndex(x => x.name == "ForceBarrier (2)");
                        break;
                    }
                case "SecurityConsole (2)":
                    {
                        tempIndex = ForceBarriers.FindIndex(x => x.name == "ForceBarrier (3)");
                        break;
                    }
                case "SecurityConsole (3)":
                    {
                        tempIndex = ForceBarriers.FindIndex(x => x.name == "ForceBarrier (1)");
                        break;
                    }
                case "SecurityConsole (4)":
                    {
                        tempIndex = ForceBarriers.FindIndex(x => x.name == "ForceBarrier");
                        break;
                    }
            }

            // if no barrier could be specifically unlocked, randomly select on from the list.
            var randomIndex = tempIndex > 0 ? tempIndex : Random.Range(0, ForceBarriers.Count(x => x.isActiveAndEnabled));

            // Get the random barrier to shut off.
            var barrierToShutOff = ForceBarriers.Where(x => x.isActiveAndEnabled).ElementAt(randomIndex);

            if (PhotonNetwork.inRoom)
            {
                photonView.RPC("RemoveForceBarrier", PhotonNetworkSettings.DefaultRPCNetworkTarget, new object[] { barrierToShutOff.gameObject.name });
            }
            else
            {
                this.RemoveForceBarrier(barrierToShutOff.gameObject.name);
            }
        }
    }

    protected List<ForceBarrierController> GetRandomLockedConsoles(int unlockCount = 2)
    {
        var results = new List<ForceBarrierController>();
        if (ConsoleBarriers.Any())
        {
            // Unlock 2 consoles
            for (int i = 0; i < unlockCount; i++)
            {
                // Get a random barrier that isn't already disabled.
                var listOfActiveBarriers = ConsoleBarriers.Where(x => x.isActiveAndEnabled).Except(results).ToList<ForceBarrierController>();

                // If there are any active barriers.
                if (listOfActiveBarriers.Count() > 0)
                {
                    // Randomly select one from the list.
                    var randomIndex = Random.Range(0, listOfActiveBarriers.Count());

                    // Get the console at index 'randomIndex'
                    var selectedConsole = listOfActiveBarriers.ElementAt(randomIndex);

                    // Output the index of the randomly selected console.
                    Debug.LogWarningFormat("Selecting console '{1}' at index {0}", randomIndex, selectedConsole.name);

                    // Get the random barrier to shut off.
                    results.Add(selectedConsole);
                }
            }
        }

        return results;
    }
}
