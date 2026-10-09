using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class SniperRifle : Weapon
{
    public float ScopeRangePullback = 3.5f;

    public float ScopeMagnification = 4f;

    public Sprite ScopeCrosshair;

    bool scoping;

    LineRenderer beam;

    LineRenderer glow;

    Transform dot;

    public override bool OccupiesBothHands
    {
        get { return true; }
    }

    public override GameObject AltFire(IPlayerController player, bool isAltFire, bool isRunning, float frameTiming)
    {
        scoping = isAltFire;
        var camera = Camera.main;
        var follow = camera != null ? camera.GetComponent<CameraFollow3D>() : null;
        if (follow != null)
        {
            follow.SetScoped(isAltFire, ScopeRangePullback);
        }

        SniperScopeView.Set(isAltFire, ScopeCrosshair, ScopeMagnification);
        PresentLaser();
        return null;
    }

    protected override Vector3 ApplyBulletSpread(IPlayerController player, Transform gunPortPos)
    {
        if (!scoping)
        {
            return base.ApplyBulletSpread(player, gunPortPos);
        }

        var savedMin = MinBulletDeviation;
        var savedMax = MaxBulletDeviation;
        MinBulletDeviation = 0.05f;
        MaxBulletDeviation = 0.12f;
        var result = base.ApplyBulletSpread(player, gunPortPos);
        MinBulletDeviation = savedMin;
        MaxBulletDeviation = savedMax;
        return result;
    }

    public void OnDisable()
    {
        scoping = false;
        var camera = Camera.main;
        var follow = camera != null ? camera.GetComponent<CameraFollow3D>() : null;
        if (follow != null)
        {
            follow.SetScoped(false, ScopeRangePullback);
        }

        SniperScopeView.Set(false, ScopeCrosshair, ScopeMagnification);
        HideLaser();
    }

    void PresentLaser()
    {
        EnsureLaser();
        var muzzle = transform.Find("Muzzle");
        var origin = muzzle != null ? muzzle.position : transform.position;
        var fallback = muzzle != null ? muzzle.forward : transform.forward;
        var aim = ShotAim.CursorPoint(origin, fallback);
        var end = LaserPoint(origin, aim, transform.root);
        beam.SetPosition(0, origin);
        beam.SetPosition(1, end);
        glow.SetPosition(0, origin);
        glow.SetPosition(1, end);
        dot.position = end;
        beam.enabled = true;
        glow.enabled = true;
        dot.gameObject.SetActive(true);
    }

    void HideLaser()
    {
        if (beam != null)
        {
            beam.enabled = false;
        }

        if (glow != null)
        {
            glow.enabled = false;
        }

        if (dot != null)
        {
            dot.gameObject.SetActive(false);
        }
    }

    void EnsureLaser()
    {
        if (beam != null)
        {
            return;
        }

        beam = MakeLine("SniperLaser", 0.055f, new Color(0.45f, 0.78f, 1f, 0.95f));
        glow = MakeLine("SniperLaserGlow", 0.18f, new Color(0.15f, 0.45f, 1f, 0.28f));
        beam.transform.SetParent(transform, false);
        glow.transform.SetParent(transform, false);
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "SniperLaserDot";
        marker.transform.SetParent(transform, false);
        marker.transform.localScale = Vector3.one * 0.22f;
        var collider = marker.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var renderer = marker.GetComponent<Renderer>();
        var shader = Shader.Find("Sprites/Default");
        if (renderer != null && shader != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = new Material(shader);
            renderer.material.color = new Color(0.55f, 0.82f, 1f, 1f);
        }

        var light = marker.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(0.35f, 0.65f, 1f);
        light.range = 3.2f;
        light.intensity = 4.5f;
        light.shadows = LightShadows.None;
        dot = marker.transform;
    }

    static LineRenderer MakeLine(string name, float width, Color color)
    {
        var item = new GameObject(name);
        var line = item.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.widthMultiplier = width;
        line.numCapVertices = 4;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.startColor = color;
        line.endColor = color;
        var shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            line.material = new Material(shader);
        }

        return line;
    }

    public static Vector3 LaserEnd(Vector3 origin, Vector3 aim, float stopDistance)
    {
        var offset = aim - origin;
        offset.y = 0f;
        if (offset.sqrMagnitude < 0.04f)
        {
            offset = new Vector3(0f, 0f, 0.5f);
        }

        var distance = offset.magnitude;
        if (stopDistance >= 0f && stopDistance < distance)
        {
            distance = stopDistance;
        }

        var end = origin + offset.normalized * distance;
        end.y = origin.y;
        return end + Vector3.up * 0.06f;
    }

    public static Vector3 LaserPoint(Vector3 origin, Vector3 aim, Transform ignoreRoot)
    {
        var clear = LaserEnd(origin, aim, -1f);
        var offset = clear - origin;
        offset.y = 0f;
        var reach = offset.magnitude;
        if (reach < 0.001f)
        {
            return clear;
        }

        var hits = Physics.RaycastAll(origin, offset.normalized, reach, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        var best = -1f;
        for (var i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit.collider == null)
            {
                continue;
            }

            if (ignoreRoot != null && hit.collider.transform.IsChildOf(ignoreRoot))
            {
                continue;
            }

            if (best < 0f || hit.distance < best)
            {
                best = hit.distance;
            }
        }

        return best < 0f ? clear : LaserEnd(origin, aim, best);
    }
}

public class SniperScopeView : MonoBehaviour
{
    static SniperScopeView active;

    RawImage lens;
    RectTransform lensRoot;
    Image crosshair;
    Camera scopeCamera;
    RenderTexture texture;
    float magnification = 4f;

    public static void Set(bool on, Sprite sprite, float zoom)
    {
        if (!on)
        {
            if (active != null)
            {
                active.gameObject.SetActive(false);
                if (active.scopeCamera != null)
                {
                    active.scopeCamera.enabled = false;
                }
            }

            return;
        }

        Ensure();
        active.magnification = zoom < 1.5f ? 4f : zoom;
        active.gameObject.SetActive(true);
        active.scopeCamera.enabled = true;
        if (sprite != null)
        {
            active.crosshair.sprite = sprite;
            active.crosshair.color = Color.white;
        }

        active.Frame();
    }

    static void Ensure()
    {
        if (active != null)
        {
            return;
        }

        var canvasObject = new GameObject("SniperScope");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        canvasObject.AddComponent<CanvasScaler>();
        active = canvasObject.AddComponent<SniperScopeView>();

        var maskObject = new GameObject("ScopeLens");
        maskObject.transform.SetParent(canvasObject.transform, false);
        var maskImage = maskObject.AddComponent<Image>();
        maskImage.sprite = CircleSprite();
        maskImage.raycastTarget = false;
        active.lensRoot = maskImage.rectTransform;
        active.lensRoot.sizeDelta = new Vector2(300f, 300f);
        maskObject.AddComponent<Mask>().showMaskGraphic = false;

        var viewObject = new GameObject("ScopeView");
        viewObject.transform.SetParent(maskObject.transform, false);
        active.lens = viewObject.AddComponent<RawImage>();
        active.lens.raycastTarget = false;
        var viewRect = active.lens.rectTransform;
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = Vector2.zero;
        viewRect.offsetMax = Vector2.zero;

        var hairObject = new GameObject("ScopeCrosshair");
        hairObject.transform.SetParent(canvasObject.transform, false);
        active.crosshair = hairObject.AddComponent<Image>();
        active.crosshair.raycastTarget = false;
        active.crosshair.preserveAspect = true;
        active.crosshair.color = new Color(0.45f, 1f, 0.85f, 0.95f);
        active.crosshair.rectTransform.sizeDelta = new Vector2(300f, 300f);

        active.texture = new RenderTexture(512, 512, 16);
        active.lens.texture = active.texture;
        var cameraObject = new GameObject("SniperScopeCamera");
        active.scopeCamera = cameraObject.AddComponent<Camera>();
        active.scopeCamera.enabled = false;
        active.scopeCamera.targetTexture = active.texture;
        active.scopeCamera.clearFlags = CameraClearFlags.SolidColor;
        active.scopeCamera.backgroundColor = new Color(0.02f, 0.04f, 0.06f, 1f);
        active.scopeCamera.nearClipPlane = 0.05f;
        active.scopeCamera.farClipPlane = 80f;
        active.scopeCamera.depth = -2;
    }

    void LateUpdate()
    {
        Frame();
    }

    void Frame()
    {
        var main = Camera.main;
        if (main == null || scopeCamera == null)
        {
            return;
        }

        var follow = main.GetComponent<CameraFollow3D>();
        var anchor = follow != null && follow.myTarget != null ? follow.myTarget : main.transform;
        var plane = anchor.position + Vector3.up * 1.2f;
        var aim = ShotAim.CursorPoint(plane, anchor.forward);
        var toAim = aim - main.transform.position;
        if (toAim.sqrMagnitude < 0.01f)
        {
            toAim = main.transform.forward;
        }

        scopeCamera.transform.position = main.transform.position;
        scopeCamera.transform.rotation = Quaternion.LookRotation(toAim, Vector3.up);
        scopeCamera.cullingMask = main.cullingMask;
        if (main.orthographic)
        {
            scopeCamera.orthographic = true;
            scopeCamera.orthographicSize = Mathf.Max(0.35f, main.orthographicSize / magnification);
        }
        else
        {
            scopeCamera.orthographic = false;
            scopeCamera.fieldOfView = Mathf.Max(4f, main.fieldOfView / magnification);
        }

        var screen = Input.mousePosition;
        if (lensRoot != null)
        {
            lensRoot.position = screen;
        }

        crosshair.rectTransform.position = screen;
    }

    static Sprite CircleSprite()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = (size - 1) * 0.5f;
        var radius = center - 0.5f;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var inside = (dx * dx) + (dy * dy) <= radius * radius;
                tex.SetPixel(x, y, inside ? Color.white : new Color(1f, 1f, 1f, 0f));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
