using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmergencyLightController : MonoBehaviour
{
    protected GameObject NormalLight;

    protected GameObject AlertLight;

	// Use this for initialization
	void Awake ()
	{
	    var lightObject = transform.Find("Light");
	    if (lightObject == null)
	    {
            Debug.LogWarning(string.Format("No 'Light' object found on '{0}'", transform.name));
	        return;
	    }

	    NormalLight = lightObject.Find("Normal").gameObject;
	    AlertLight = lightObject.Find("Alert").gameObject;
	}

    public void SetState(bool isAlert)
    {
        if (NormalLight == null || AlertLight == null)
        {
            Debug.LogWarning(string.Format("No Normal/Alert light found on '{0}'", transform.name));
            return;
        }

        NormalLight.GetComponent<Light>().enabled = !isAlert;
        AlertLight.GetComponent<Light>().enabled = isAlert;
        var alertAnimation = AlertLight.GetComponent<Animation>();
        alertAnimation.enabled = isAlert;
    }
}
