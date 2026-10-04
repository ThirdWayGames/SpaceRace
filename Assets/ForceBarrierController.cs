using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.ScriptableObjects;

public class ForceBarrierController : Photon.MonoBehaviour
{
    public List<EventTriggerVariable> TriggerEvents;

    public List<BooleanReference> DeactivateConditions;

    void Start()
    {
        if (TriggerEvents.Any())
        {
            foreach (var triggerEvent in TriggerEvents)
            {
                triggerEvent.TriggerEvent.AddListener(ToggleForceBarrier);
            }
        }
    }

    // Update is called once per frame
    void Update ()
    {
	    if (DeactivateConditions.Any())
	    {
            this.gameObject.SetActive(!DeactivateConditions.All(x => x.Value));
	    }
    }

    public void ToggleForceBarrier(FloatTrigger toggleValue)
    {
        gameObject.SetActive(toggleValue.value > 0);
    }
}
