using UnityEngine;

/// <summary>
/// Changes the material of anything that the ray hits between itself and the target.
/// </summary>
public class RayCastMaterialEditor : MonoBehaviour
{
    /// <summary>
    /// The ray cast target
    /// </summary>
    public Transform RayTarget;

    /// <summary>
    /// The altered mesh
    /// </summary>
    private Renderer AlteredMesh;

    public CursorLockMode wantedMode;

    public void SetCursorState()
    {
        Cursor.lockState = wantedMode;

        Cursor.visible = (CursorLockMode.Locked != wantedMode);
    }

    public void OnGui()
    {
        GUILayout.BeginVertical();
        // Release cursor on escape keypress
        if (Input.GetKeyDown(KeyCode.Escape))
            Cursor.lockState = wantedMode = CursorLockMode.None;

        switch (Cursor.lockState)
        {
            case CursorLockMode.None:
                GUILayout.Label("Cursor is normal");
                if (GUILayout.Button("Lock cursor"))
                    wantedMode = CursorLockMode.Locked;
                if (GUILayout.Button("Confine cursor"))
                    wantedMode = CursorLockMode.Confined;
                break;
            case CursorLockMode.Confined:
                GUILayout.Label("Cursor is confined");
                if (GUILayout.Button("Lock cursor"))
                    wantedMode = CursorLockMode.Locked;
                if (GUILayout.Button("Release cursor"))
                    wantedMode = CursorLockMode.None;
                break;
            case CursorLockMode.Locked:
                GUILayout.Label("Cursor is locked");
                if (GUILayout.Button("Unlock cursor"))
                    wantedMode = CursorLockMode.None;
                if (GUILayout.Button("Confine cursor"))
                    wantedMode = CursorLockMode.Confined;
                break;
        }

        GUILayout.EndVertical();

        SetCursorState();
    }

    /// <summary>
    /// Sets the material transparent.
    /// </summary>
    /// <param name="hitRenderer">The hit renderer.</param>
    protected void SetMaterialTransparent(Renderer hitRenderer)
    {
        foreach (var m in hitRenderer.materials)
        {
            m.SetFloat("_Mode", 2);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
        }
    }

    /// <summary>
    /// Sets the material opaque.
    /// </summary>
    /// <param name="hitRenderer">The hit renderer.</param>
    protected void SetMaterialOpaque(Renderer hitRenderer)
    {
        foreach (var m in hitRenderer.materials)
        {
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            m.SetInt("_ZWrite", 1);
            m.DisableKeyword("_ALPHATEST_ON");
            m.DisableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = -1;
        }
    }

    // Update is called once per frame
    void Update () 
    {
        if (RayTarget == null)
        {
            Debug.Log(string.Format("No ray target set for {0}", this.gameObject.name));
            return;
        }

        // Cast a ray from me to the target.
        RaycastHit rayCastHit;
        var rayDirection = (RayTarget.position - this.gameObject.transform.position).normalized;

        // If I hit anything
        if (Physics.Raycast(this.gameObject.transform.position, rayDirection, out rayCastHit, Vector3.Distance(this.gameObject.transform.position, RayTarget.transform.position)))
        {
            // If we hit an object on the obstacles layer.
            if (rayCastHit.collider.gameObject.layer == 11)
            {
                // Does it have a mesh renderer.
                var meshRenderer = rayCastHit.collider.gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null && AlteredMesh != meshRenderer)
                {
                    // Log that we hit this renderer.
                    AlteredMesh = meshRenderer;
                    //// SetMaterialTransparent(AlteredMesh);
                }
            }
            else
            {
                // If the hit mesh is set.
                if (AlteredMesh != null)
                {
                    //// SetMaterialOpaque(AlteredMesh);
                    AlteredMesh = null;
                }
            }
        }
    }
}
