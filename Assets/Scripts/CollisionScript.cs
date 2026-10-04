using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollisionScript : MonoBehaviour
{
    public List<EmergencyLightController> LinkedLights;

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (LinkedLights != null && LinkedLights.Any())
        {
            LinkedLights.ForEach(x => x.SetState(true));
        }
    }

    public void OnTriggerExit2D(Collider2D col)
    {
        if (LinkedLights != null && LinkedLights.Any())
        {
            LinkedLights.ForEach(x => x.SetState(false));
        }
    }
}
