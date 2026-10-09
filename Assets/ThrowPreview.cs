using UnityEngine;

public class ThrowPreview : MonoBehaviour
{
    static ThrowPreview active;
    static int ownerId;

    Transform barRoot;
    Transform barFill;
    LineRenderer ring;
    const float BarWidth = 1.35f;

    public static void Show(Transform owner, Vector3 bodyPosition, float groundY, Vector3 landing, float charge)
    {
        if (owner == null)
        {
            return;
        }

        Ensure();
        ownerId = owner.GetInstanceID();
        active.gameObject.SetActive(true);
        active.Place(bodyPosition, groundY, landing, Mathf.Clamp01(charge));
    }

    public static void Hide(Transform owner)
    {
        if (active == null)
        {
            return;
        }

        if (owner != null && owner.GetInstanceID() != ownerId)
        {
            return;
        }

        active.gameObject.SetActive(false);
    }

    static void Ensure()
    {
        if (active != null)
        {
            return;
        }

        var host = new GameObject("ThrowPreview");
        active = host.AddComponent<ThrowPreview>();
        active.Build();
    }

    void Build()
    {
        barRoot = Quad("ThrowChargeBar", new Color(0.08f, 0.09f, 0.1f, 0.9f));
        barRoot.SetParent(transform, false);
        barFill = Quad("ThrowChargeFill", new Color(1f, 0.55f, 0.16f, 1f));
        barFill.SetParent(barRoot, false);

        var ringObject = new GameObject("ThrowReticle");
        ringObject.transform.SetParent(transform, false);
        ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 28;
        ring.widthMultiplier = 0.045f;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
        var shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            ring.material = new Material(shader);
        }

        ring.startColor = new Color(0.2f, 0.95f, 0.9f, 1f);
        ring.endColor = ring.startColor;
        for (var i = 0; i < ring.positionCount; i++)
        {
            var angle = (Mathf.PI * 2f * i) / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.38f, 0.02f, Mathf.Sin(angle) * 0.38f));
        }
    }

    void Place(Vector3 bodyPosition, float groundY, Vector3 landing, float charge)
    {
        var camera = Camera.main;
        var barPosition = new Vector3(bodyPosition.x, groundY + 0.08f, bodyPosition.z);
        barRoot.position = barPosition;
        if (camera != null)
        {
            barRoot.rotation = Quaternion.LookRotation(barRoot.position - camera.transform.position, Vector3.up);
        }

        barRoot.localScale = new Vector3(BarWidth, 0.11f, 1f);
        var fill = Mathf.Max(0.02f, charge);
        barFill.localPosition = new Vector3((fill - 1f) * 0.5f, 0f, -0.01f);
        barFill.localScale = new Vector3(fill, 0.72f, 1f);

        ring.transform.position = landing;
        ring.transform.rotation = Quaternion.identity;
    }

    static Transform Quad(string name, Color color)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        var collider = quad.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var renderer = quad.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material.color = color;
        }

        return quad.transform;
    }
}
