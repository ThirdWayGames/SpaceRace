using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLight : MonoBehaviour
{
    private Light lightComponent;

    public float minWaitTime;

    public float maxWaitTime;

    // Use this for initialization
    void Start ()
    {
        lightComponent = GetComponent<Light>();
        StartCoroutine(Flashing());
    }

    IEnumerator Flashing()
    {
        while (true)
        {
            yield return  new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
            lightComponent.enabled = !lightComponent.enabled;
        }
    }
}
