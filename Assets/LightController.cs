using System;

using UnityEngine;

public class LightController : MonoBehaviour
{
    protected Light Light;

    protected Material OldMaterial;

    public Renderer MeshRenderer;

    public bool ChangeMaterial = false;

    public Material NewMaterial;

    public int MaterialIndex = 0;

    // Use this for initialization
    public virtual void Awake()
    {
        SetNormalLight();

        // Make sure that if we have a light it is off by default.
        if (this.Light != null)
        {
            this.Light.enabled = false;
        }

        if (ChangeMaterial)
        {
            if (this.MeshRenderer == null)
            {
                throw new Exception(string.Format("No child component of type 'MeshRenderer' found on {0}", transform.name));
            }

            if (this.NewMaterial == null)
            {
                throw new Exception(string.Format("No property 'NewMaterial' of type 'Material' set on {0}", transform.name));
            }
        }
    }

    public virtual void TurnLightOn()
    {
        if (!this.Light.enabled)
        {
            this.Light.enabled = true;

            if (this.ChangeMaterial && this.MeshRenderer != null)
            {
                var currentMats = this.MeshRenderer.materials;
                if (!currentMats[MaterialIndex].name.StartsWith(this.NewMaterial.name))
                {
                    this.OldMaterial = currentMats[MaterialIndex];
                    currentMats[MaterialIndex] = NewMaterial;
                    MeshRenderer.materials = currentMats;
                }
            }
        }
    }

    public virtual void TurnLightOff()
    {
        if (this.Light.enabled)
        {
            this.Light.enabled = false;

            if (this.ChangeMaterial && this.MeshRenderer != null)
            {
                var currentMats = this.MeshRenderer.materials;
                if (currentMats[MaterialIndex].name.StartsWith(this.NewMaterial.name))
                {
                    if (this.OldMaterial != null)
                    {
                        currentMats[MaterialIndex] = this.OldMaterial;
                        this.OldMaterial = null;
                        MeshRenderer.materials = currentMats;
                    }
                }
            }
        }
    }

    protected virtual void SetNormalLight()
    {
        this.Light = this.GetComponentInChildren<Light>();

        if (this.Light == null)
        {
            Debug.LogWarning(string.Format("No child component of type 'Light' found on {0}", transform.name));
            return;
        }
    }
}
