using UnityEngine;

public class EmergencyLightController3D : LightController
{
    protected GameObject AlertLight;

    public bool AlertState = false;

    public bool ReverseAnimation = false;

    public virtual void Update()
    {
        // If we are in an alert state and the normal light is on.
        if (AlertState && Light.enabled)
        {
            // Turn it off.
            TurnLightOff();
        }

        // Turn on the alert light.
        AlertLight.GetComponent<Light>().enabled = AlertState;

        // Apply the alert state material.
        UpdateMaterial(AlertState);

        // Trigger the animation
        var alertAnimation = AlertLight.GetComponent<Animation>();
        if (alertAnimation == null)
        {
            Debug.Log("No animation for the alert light found");
            return;
        }

        alertAnimation.enabled = AlertState;
        if (AlertState)
        {
            alertAnimation.Play(ReverseAnimation ? "AlertReverse" : "Alert");
        }
        else
        {
            alertAnimation.Stop();
        }
    }

    public void SetAlertState(bool alertState)
    {
        AlertState = alertState;
    }

    public override void TurnLightOn()
    {
        if (!this.Light.enabled && !AlertState)
        {
            // Turn on the normal light.
            this.Light.enabled = true;
        }
    }

    public override void TurnLightOff()
    {
        if (this.Light.enabled)
        {
            // Turn off the normal light.
            this.Light.enabled = false;
        }
    }

    protected void UpdateMaterial(bool applyNew)
    {
        // Swtich to the alert material
        if (this.ChangeMaterial)
        {
            // Get the current materials.
            var currentMats = this.MeshRenderer.materials;

            // Determine if the current material is the new material.
            var currentMatIsNew = currentMats[MaterialIndex].name.StartsWith(this.NewMaterial.name);

            // If the current material is not the new material and we want to apply the new material.
            if (!currentMatIsNew && applyNew)
            {
                // Store the old material.
                this.OldMaterial = currentMats[MaterialIndex];

                // Update to the new material.
                currentMats[MaterialIndex] = NewMaterial;
            }
            else
            {
                // Providing we have an old material stored.
                if (this.OldMaterial != null)
                {
                    // Set the current material to be the original material.
                    currentMats[MaterialIndex] = this.OldMaterial;

                    // unstore the old material.
                    this.OldMaterial = null;
                }                
            }

            // Reset the current materials.
            MeshRenderer.materials = currentMats;
        }
    }

    protected override void SetNormalLight()
    {
        var lightObject = transform.Find("Light");
        if (lightObject == null)
        {
            Debug.LogWarning(string.Format("No 'Light' object found on '{0}'", transform.name));
            return;
        }

        Light = lightObject.Find("Normal").GetComponent<Light>();
        AlertLight = lightObject.Find("Alert").gameObject;

        if (this.Light == null)
        {
            Debug.LogWarning(string.Format("No child component of type 'Light' found on {0}", transform.name));
            return;
        }

        if (this.AlertLight == null)
        {
            Debug.LogWarning(string.Format("No child component of type 'AlertLight' found on {0}", transform.name));
        }
    }
}
