using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class SniperRifle : Weapon
{
    public float ScopeRangePullback = 3.5f;

    [Tooltip("Starting scope zoom. Matched to the nearest Scope Zoom Step. Fire delay, bullet speed, lifetime, and energy are the fields above. Damage is the SniperRifleDmg asset.")]
    public float ScopeMagnification = 10f;

    [Tooltip("Camera magnification for each mouse-wheel step.")]
    public float[] ScopeZoomSteps = { 8f, 10f, 12f };

    [Tooltip("Name shown in the scope for each step. 2, 4, and 6 name the 8, 10, and 12 zooms.")]
    public float[] ScopeZoomLabels = { 2f, 4f, 6f };

    public Sprite ScopeCrosshair;

    public ScopeSwaySettings Profile;

    public static readonly float[] ScopeMagnifications = { 8f, 10f, 12f };

    public static readonly float[] ScopeMagnificationLabels = { 2f, 4f, 6f };

    public const int DefaultScopeMagnificationIndex = 1;

    public static readonly Color LaserCore = new Color(1f, 0.14f, 0.1f, 0.55f);

    public static readonly Color LaserGlow = new Color(1f, 0.05f, 0.03f, 0.22f);

    const string LaserShaderName = "SpaceRace/LaserBeam";

    bool scoping;

    LineRenderer beam;

    LineRenderer glow;

    Transform dot;

    ScopeSwayState sway;

    Vector2 aimPixels;

    public override bool OccupiesBothHands
    {
        get { return true; }
    }

    public override GameObject AltFire(IPlayerController player, bool isAltFire, bool isRunning, float frameTiming)
    {
        if (isAltFire && !scoping)
        {
            sway = new ScopeSwayState();
        }

        scoping = isAltFire;
        var camera = Camera.main;
        var follow = camera != null ? camera.GetComponent<CameraFollow3D>() : null;
        if (follow != null)
        {
            follow.SetScoped(isAltFire, ScopeRangePullback);
        }

        if (isAltFire)
        {
            UpdateAim(player);
            PresentLaser();
        }
        else
        {
            sway = new ScopeSwayState();
            aimPixels = Vector2.zero;
            SniperScopeView.ClearHud();
            HideLaser();
        }

        SniperScopeView.Set(isAltFire, ScopeCrosshair, ScopeMagnification, ActiveZoomSteps(), ActiveZoomLabels());
        return null;
    }

    public ScopeSwaySettings ActiveProfile()
    {
        return Profile != null ? Profile : ScopeSwaySettings.Fallback;
    }

    void UpdateAim(IPlayerController player)
    {
        var settings = ActiveProfile();
        var still = false;
        if (player != null)
        {
            var movement = player.GetComponent<Assets.Scripts.Components.MovementComponent>();
            if (movement != null)
            {
                movement.IsRunning = false;
                still = ScopeSwayMath.StandingStill(movement.Horizontal, movement.Vertical);
            }
        }

        var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var holding = ScopeSwayMath.HoldingBreath(still, shift, sway.Recovering);
        sway = ScopeSwayMath.Step(sway, holding, Time.deltaTime, settings.BreathHoldSeconds, settings.SwayReturnSeconds, settings.BreathSettleSeconds);
        holding = ScopeSwayMath.HoldingBreath(still, shift, sway.Recovering);
        var multiplier = ScopeSwayMath.Multiplier(settings.BreathHoldReduction, holding, sway.Recovering, sway.Recovery, settings.SwayPenalty, sway.Settle);
        aimPixels = ScopeSwayMath.Offset(Time.time, settings.SwayAmount, settings.SwaySpeed) * multiplier;
        SniperScopeView.AimPixels = aimPixels;
        SniperScopeView.Breath = sway.Breath;
        SniperScopeView.ShowBreath = holding || sway.Recovering;
        SniperScopeView.BreathBroken = sway.Recovering;
        SniperScopeView.Shade = settings.BlockedShade;
    }

    public static int StepScopeMagnification(int index, float scrollDelta)
    {
        return StepScopeMagnification(index, scrollDelta, ScopeMagnifications.Length);
    }

    public static int StepScopeMagnification(int index, float scrollDelta, int stepCount)
    {
        if (scrollDelta > 0.01f)
        {
            index++;
        }
        else if (scrollDelta < -0.01f)
        {
            index--;
        }

        if (stepCount < 1)
        {
            stepCount = 1;
        }

        if (index < 0)
        {
            index = 0;
        }

        if (index >= stepCount)
        {
            index = stepCount - 1;
        }

        return index;
    }

    public static int NearestScopeMagnification(float zoom)
    {
        return NearestScopeMagnification(zoom, ScopeMagnifications);
    }

    public static int NearestScopeMagnification(float zoom, float[] steps)
    {
        var table = steps != null && steps.Length > 0 ? steps : ScopeMagnifications;
        var best = DefaultScopeMagnificationIndex;
        if (best >= table.Length)
        {
            best = table.Length - 1;
        }

        var bestGap = float.MaxValue;
        for (var i = 0; i < table.Length; i++)
        {
            var gap = table[i] - zoom;
            if (gap < 0f)
            {
                gap = -gap;
            }

            if (gap < bestGap)
            {
                bestGap = gap;
                best = i;
            }
        }

        return best;
    }

    public static float ScopeMagnificationAt(int index)
    {
        return ScopeMagnificationAt(index, ScopeMagnifications);
    }

    public static float ScopeMagnificationAt(int index, float[] steps)
    {
        return TableValue(index, steps != null && steps.Length > 0 ? steps : ScopeMagnifications);
    }

    public static string ScopeMagnificationName(int index)
    {
        return ScopeMagnificationName(index, ScopeMagnificationLabels);
    }

    public static string ScopeMagnificationName(int index, float[] labels)
    {
        var table = labels != null && labels.Length > 0 ? labels : ScopeMagnificationLabels;
        return TableValue(index, table).ToString("0") + "x";
    }

    public float[] ActiveZoomSteps()
    {
        return ScopeZoomSteps != null && ScopeZoomSteps.Length > 0 ? ScopeZoomSteps : ScopeMagnifications;
    }

    public float[] ActiveZoomLabels()
    {
        return ScopeZoomLabels != null && ScopeZoomLabels.Length > 0 ? ScopeZoomLabels : ScopeMagnificationLabels;
    }

    static float TableValue(int index, float[] table)
    {
        if (table == null || table.Length == 0)
        {
            return 0f;
        }

        if (index < 0)
        {
            index = 0;
        }

        if (index >= table.Length)
        {
            index = table.Length - 1;
        }

        return table[index];
    }

    protected override Vector3 ApplyBulletSpread(IPlayerController player, Transform gunPortPos)
    {
        if (!scoping)
        {
            return base.ApplyBulletSpread(player, gunPortPos);
        }

        var fallback = gunPortPos.forward;
        var cursor = ShotAim.ScreenPoint((Vector2)Input.mousePosition + aimPixels, gunPortPos.position, fallback);
        var direction = ShotAim.Direction(gunPortPos.position, cursor, fallback);
        return Quaternion.LookRotation(direction, Vector3.up).eulerAngles;
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

        sway = new ScopeSwayState();
        aimPixels = Vector2.zero;
        SniperScopeView.ClearHud();
        SniperScopeView.Set(false, ScopeCrosshair, ScopeMagnification, null, null);
        HideLaser();
    }

    void PresentLaser()
    {
        EnsureLaser();
        var muzzle = transform.Find("Muzzle");
        var origin = muzzle != null ? muzzle.position : transform.position;
        var fallback = muzzle != null ? muzzle.forward : transform.forward;
        var aim = ShotAim.ScreenPoint((Vector2)Input.mousePosition + aimPixels, origin, fallback);
        var clear = LaserEnd(origin, aim, -1f);
        var end = LaserPoint(origin, aim, transform.root);
        SniperScopeView.ShotIsBlocked = IsShotBlocked(FlatReach(origin, clear), FlatReach(origin, end), 0.2f);
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

        beam = MakeLine("SniperLaser", 0.03f, new Color(LaserCore.r, LaserCore.g, LaserCore.b, 0.34f), LaserCore);
        glow = MakeLine("SniperLaserGlow", 0.11f, LaserGlow, LaserGlow);
        beam.transform.SetParent(transform, false);
        glow.transform.SetParent(transform, false);
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "SniperLaserDot";
        marker.transform.SetParent(transform, false);
        marker.transform.localScale = Vector3.one * 0.14f;
        var collider = marker.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        var renderer = marker.GetComponent<Renderer>();
        var material = LaserMaterial();
        if (renderer != null && material != null)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.material = material;
            renderer.material.color = new Color(1f, 0.2f, 0.12f, 0.7f);
        }

        dot = marker.transform;
    }

    static Material LaserMaterial()
    {
        var shader = Shader.Find(LaserShaderName);
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", Color.white);
        }

        if (material.HasProperty("_Emission"))
        {
            material.SetFloat("_Emission", 2.6f);
        }

        return material;
    }

    static LineRenderer MakeLine(string name, float width, Color start, Color end)
    {
        var item = new GameObject(name);
        var line = item.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.widthMultiplier = width;
        line.numCapVertices = 4;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.startColor = start;
        line.endColor = end;
        var material = LaserMaterial();
        if (material != null)
        {
            line.material = material;
        }

        return line;
    }

    public static float FlatReach(Vector3 origin, Vector3 point)
    {
        var offset = point - origin;
        offset.y = 0f;
        return offset.magnitude;
    }

    public static bool IsShotBlocked(float reach, float traveled, float slack)
    {
        if (reach < 0.05f)
        {
            return false;
        }

        if (slack < 0f)
        {
            slack = 0f;
        }

        return traveled < reach - slack;
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

            if (!BlocksScopeLos(IsPlayerBody(hit.collider.transform), IsMonsterBody(hit.collider.transform), IsProjectile(hit.collider.transform)))
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

    public static bool BlocksScopeLos(bool isPlayer, bool isMonster)
    {
        return BlocksScopeLos(isPlayer, isMonster, false);
    }

    public static bool BlocksScopeLos(bool isPlayer, bool isMonster, bool isProjectile)
    {
        return !isPlayer && !isMonster && !isProjectile;
    }

    static bool IsProjectile(Transform hit)
    {
        if (hit == null)
        {
            return false;
        }

        return hit.GetComponentInParent<Bullet3D>() != null
            || hit.GetComponentInParent<Assets.Scripts.Interfaces.IBullet>() != null;
    }

    static bool IsPlayerBody(Transform hit)
    {
        return hit != null && hit.GetComponentInParent<Assets.Scripts.BasePlayerController3D>() != null;
    }

    static bool IsMonsterBody(Transform hit)
    {
        if (hit == null)
        {
            return false;
        }

        if (hit.GetComponentInParent<EnemyStateComponent>() != null || hit.GetComponentInParent<EnemyTargetComponent>() != null)
        {
            return true;
        }

        var current = hit;
        while (current != null)
        {
            if (current.tag == "EnemyTarget")
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}

public class SniperScopeView : MonoBehaviour
{
    static SniperScopeView active;

    public const float ScopeDiameterScale = 1.05f;
    public const float MinimapWidgetWidth = 593.71f;
    public const float MinimapWidgetHeight = 1387.7f;
    public const float MinimapScaleX = 0.25f;
    public const float MinimapScaleY = 0.096f;
    public const float BorderThickness = 6f;
    public const float BottomBandDepth = 16f;
    public const float BreathArcSpan = 0.18f;
    public const float BreathArcOffsetY = -5f;
    public const float LabelArcCenter = -62f;
    public const float LabelGlyphSpacing = 11f;
    const int MaxMagnificationGlyphs = 4;

    public static float LensRadius
    {
        get { return MinimapCircleDiameter(MinimapWidgetWidth, MinimapWidgetHeight, MinimapScaleX, MinimapScaleY) * 0.5f * ScopeDiameterScale; }
    }

    public static Vector2 AimPixels;
    public static bool ShotIsBlocked;
    public static float Shade = 0.62f;
    public static float Breath;
    public static bool ShowBreath;
    public static bool BreathBroken;

    RawImage lens;
    RectTransform lensRoot;
    RectTransform border;
    Image crosshair;
    Image blockedShade;
    Image blockedMarkA;
    Image blockedMarkB;
    Image breathTrack;
    RectTransform bottomBand;
    Text[] magGlyphs;
    float[] zoomSteps;
    float[] zoomLabels;
    Camera scopeCamera;
    RenderTexture texture;
    float magnification = 4f;
    int magIndex = SniperRifle.DefaultScopeMagnificationIndex;
    Sprite whiteSprite;

    public static void ClearHud()
    {
        AimPixels = Vector2.zero;
        ShotIsBlocked = false;
        Breath = 0f;
        ShowBreath = false;
        BreathBroken = false;
    }

    public static void Set(bool on, Sprite sprite, float zoom, float[] steps, float[] labels)
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
        active.zoomSteps = steps;
        active.zoomLabels = labels;
        var opening = !active.gameObject.activeSelf;
        active.gameObject.SetActive(true);
        active.scopeCamera.enabled = true;
        if (opening)
        {
            active.magIndex = SniperRifle.NearestScopeMagnification(zoom, steps);
            active.magnification = SniperRifle.ScopeMagnificationAt(active.magIndex, steps);
        }
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

        var borderObject = new GameObject("ScopeBorder");
        borderObject.transform.SetParent(canvasObject.transform, false);
        var borderImage = borderObject.AddComponent<Image>();
        borderImage.sprite = CircleSprite();
        borderImage.color = Color.black;
        borderImage.raycastTarget = false;
        active.border = borderImage.rectTransform;
        active.border.sizeDelta = new Vector2(BorderDiameter(LensRadius, BorderThickness), BorderDiameter(LensRadius, BorderThickness));

        var maskObject = new GameObject("ScopeLens");
        maskObject.transform.SetParent(canvasObject.transform, false);
        var maskImage = maskObject.AddComponent<Image>();
        maskImage.sprite = borderImage.sprite;
        maskImage.raycastTarget = false;
        active.lensRoot = maskImage.rectTransform;
        active.lensRoot.sizeDelta = new Vector2(LensRadius * 2f, LensRadius * 2f);
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
        active.crosshair.rectTransform.sizeDelta = new Vector2(LensRadius * 2f, LensRadius * 2f);

        var shadeObject = new GameObject("ScopeShade");
        shadeObject.transform.SetParent(maskObject.transform, false);
        active.blockedShade = shadeObject.AddComponent<Image>();
        active.blockedShade.raycastTarget = false;
        active.blockedShade.color = new Color(0f, 0f, 0f, 0f);
        var shadeRect = active.blockedShade.rectTransform;
        shadeRect.anchorMin = Vector2.zero;
        shadeRect.anchorMax = Vector2.one;
        shadeRect.offsetMin = Vector2.zero;
        shadeRect.offsetMax = Vector2.zero;
        active.blockedShade.enabled = false;

        active.whiteSprite = WhiteSprite();
        active.blockedMarkA = MakeBar(canvasObject.transform, "ScopeBlockA", active.whiteSprite, 45f);
        active.blockedMarkB = MakeBar(canvasObject.transform, "ScopeBlockB", active.whiteSprite, -45f);

        var bandObject = new GameObject("ScopeBottomBand");
        bandObject.transform.SetParent(canvasObject.transform, false);
        var bandImage = bandObject.AddComponent<Image>();
        bandImage.sprite = BottomBorderSprite(LensRadius, LensRadius + BottomBandDepth);
        bandImage.color = Color.black;
        bandImage.raycastTarget = false;
        active.bottomBand = bandImage.rectTransform;
        var bandSize = (LensRadius + BottomBandDepth) * 2f;
        active.bottomBand.sizeDelta = new Vector2(bandSize, bandSize);

        var trackObject = new GameObject("BreathTrack");
        trackObject.transform.SetParent(canvasObject.transform, false);
        active.breathTrack = trackObject.AddComponent<Image>();
        active.breathTrack.sprite = RingSprite(LensRadius, LensRadius + BorderThickness);
        active.breathTrack.type = Image.Type.Filled;
        active.breathTrack.fillMethod = Image.FillMethod.Radial360;
        active.breathTrack.fillOrigin = 0;
        active.breathTrack.fillClockwise = true;
        active.breathTrack.fillAmount = 0f;
        active.breathTrack.raycastTarget = false;
        active.breathTrack.color = new Color(0.25f, 0.95f, 0.45f, 0.95f);
        var arcSize = BorderDiameter(LensRadius, BorderThickness);
        active.breathTrack.rectTransform.sizeDelta = new Vector2(arcSize, arcSize);
        active.breathTrack.enabled = false;

        active.magGlyphs = new Text[MaxMagnificationGlyphs];
        for (var i = 0; i < active.magGlyphs.Length; i++)
        {
            active.magGlyphs[i] = MakeMagnificationGlyph(canvasObject.transform);
        }

        active.texture = new RenderTexture(512, 512, 16);
        active.lens.texture = active.texture;
        var cameraObject = new GameObject("SniperScopeCamera");
        active.scopeCamera = cameraObject.AddComponent<Camera>();
        active.scopeCamera.enabled = false;
        active.scopeCamera.targetTexture = active.texture;
        active.scopeCamera.clearFlags = CameraClearFlags.SolidColor;
        active.scopeCamera.backgroundColor = new Color(0.02f, 0.04f, 0.06f, 1f);
        active.scopeCamera.nearClipPlane = 0.05f;
        active.scopeCamera.farClipPlane = 220f;
        active.scopeCamera.depth = -2;
        canvasObject.SetActive(false);
    }

    void LateUpdate()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0.01f || scroll < -0.01f)
        {
            var stepCount = zoomSteps != null && zoomSteps.Length > 0 ? zoomSteps.Length : SniperRifle.ScopeMagnifications.Length;
            magIndex = SniperRifle.StepScopeMagnification(magIndex, scroll, stepCount);
            magnification = SniperRifle.ScopeMagnificationAt(magIndex, zoomSteps);
        }

        Frame();
    }

    public static void Refresh()
    {
        if (active != null && active.isActiveAndEnabled)
        {
            active.Frame();
        }
    }

    public static Vector3 BirdseyePosition(Vector3 aim, Vector3 offset)
    {
        return aim + offset;
    }

    public static Vector3 ScopeLookDirection(Vector3 offset)
    {
        if (offset.sqrMagnitude < 0.0001f)
        {
            return new Vector3(0f, -1f, 0.2f).normalized;
        }

        return (-offset).normalized;
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
        var aimScreen = (Vector2)Input.mousePosition + AimPixels;
        var aim = ShotAim.ScreenPoint(aimScreen, plane, anchor.forward);
        var offset = follow != null ? follow.BirdseyeOffset() : main.transform.position - anchor.position;
        if (offset.sqrMagnitude < 0.25f)
        {
            offset = new Vector3(0f, 12f, -8f);
        }

        scopeCamera.transform.position = BirdseyePosition(aim, offset);
        scopeCamera.transform.LookAt(aim, Vector3.up);
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
        if (border != null)
        {
            border.position = screen;
        }

        if (lensRoot != null)
        {
            lensRoot.position = screen;
        }

        crosshair.rectTransform.position = screen;
        var shade = ShotIsBlocked ? Shade : 0f;
        if (shade < 0f)
        {
            shade = 0f;
        }

        if (shade > 0.92f)
        {
            shade = 0.92f;
        }

        if (lens != null)
        {
            lens.color = Color.Lerp(Color.white, new Color(0.18f, 0.14f, 0.14f, 1f), shade);
        }

        if (blockedShade != null)
        {
            blockedShade.enabled = shade > 0.01f;
            blockedShade.color = new Color(0f, 0f, 0f, shade);
        }

        PlaceMark(blockedMarkA, screen, ShotIsBlocked);
        PlaceMark(blockedMarkB, screen, ShotIsBlocked);
        if (bottomBand != null)
        {
            bottomBand.position = screen;
        }

        if (breathTrack != null)
        {
            var fill = Breath < 0f ? 0f : (Breath > 1f ? 1f : Breath);
            breathTrack.enabled = ShowBreath && fill > 0.001f;
            var arc = BreathArcPosition(screen, BreathArcOffsetY);
            breathTrack.rectTransform.position = arc;
            breathTrack.fillAmount = BreathArcFill(fill, BreathArcSpan);
            breathTrack.color = BreathBroken
                ? new Color(0.9f, 0.22f, 0.16f, 0.95f)
                : new Color(0.25f, 0.95f, 0.45f, 0.95f);
        }

        if (magGlyphs != null)
        {
            var label = SniperRifle.ScopeMagnificationName(magIndex, zoomLabels);
            var arcRadius = LabelArcRadius(LensRadius, BottomBandDepth);
            for (var i = 0; i < magGlyphs.Length; i++)
            {
                var glyph = magGlyphs[i];
                if (glyph == null)
                {
                    continue;
                }

                var show = i < label.Length;
                glyph.enabled = show;
                if (!show)
                {
                    continue;
                }

                var angle = CurvedGlyphAngle(i, label.Length, LabelArcCenter, LabelGlyphSpacing);
                var pos = CurvedGlyphPosition(screen, arcRadius, angle);
                glyph.rectTransform.position = new Vector3(pos.x, pos.y, screen.z);
                glyph.rectTransform.localRotation = Quaternion.Euler(0f, 0f, CurvedGlyphRotation(angle));
                glyph.text = label[i].ToString();
            }
        }
    }

    public static Vector3 BreathArcPosition(Vector3 scopeCenter, float offsetY)
    {
        return new Vector3(scopeCenter.x, scopeCenter.y + offsetY, scopeCenter.z);
    }

    public static float LabelArcRadius(float lensRadius, float bandDepth)
    {
        if (bandDepth < 0f)
        {
            bandDepth = 0f;
        }

        return lensRadius + bandDepth * 0.5f;
    }

    public static float CurvedGlyphAngle(int index, int count, float centerAngle, float spacing)
    {
        if (count < 1)
        {
            count = 1;
        }

        if (index < 0)
        {
            index = 0;
        }

        if (index >= count)
        {
            index = count - 1;
        }

        return centerAngle + (index - (count - 1) * 0.5f) * spacing;
    }

    public static Vector2 CurvedGlyphPosition(Vector2 scopeCenter, float radius, float angleDegrees)
    {
        var radians = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(scopeCenter.x + Mathf.Cos(radians) * radius, scopeCenter.y + Mathf.Sin(radians) * radius);
    }

    public static float CurvedGlyphRotation(float angleDegrees)
    {
        return angleDegrees + 90f;
    }

    public static float MinimapCircleDiameter(float widgetWidth, float widgetHeight, float scaleX, float scaleY)
    {
        var screenW = widgetWidth * scaleX;
        var screenH = widgetHeight * scaleY;
        if (screenW < 0f)
        {
            screenW = -screenW;
        }

        if (screenH < 0f)
        {
            screenH = -screenH;
        }

        var diameter = screenW < screenH ? screenW : screenH;
        return diameter < 16f ? 16f : diameter;
    }

    public static float BreathArcFill(float breath, float span)
    {
        if (breath < 0f)
        {
            breath = 0f;
        }

        if (breath > 1f)
        {
            breath = 1f;
        }

        if (span < 0f)
        {
            span = 0f;
        }

        if (span > 0.45f)
        {
            span = 0.45f;
        }

        return breath * span;
    }

    public static float BorderDiameter(float lensRadius, float thickness)
    {
        if (thickness < 0f)
        {
            thickness = 0f;
        }

        return (lensRadius + thickness) * 2f;
    }

    public static Vector2 MagnificationLabelPosition(Vector2 scopeCenter, float lensRadius, float borderThickness, float bandDepth)
    {
        if (bandDepth < borderThickness)
        {
            bandDepth = borderThickness;
        }

        return CurvedGlyphPosition(scopeCenter, LabelArcRadius(lensRadius, bandDepth), LabelArcCenter);
    }

    static Text MakeMagnificationGlyph(Transform parent)
    {
        var labelObject = new GameObject("ScopeMagnification");
        labelObject.transform.SetParent(parent, false);
        var text = labelObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 11;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.55f, 0.95f, 0.82f, 0.95f);
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        text.rectTransform.sizeDelta = new Vector2(16f, 16f);
        text.enabled = false;
        return text;
    }

    static void PlaceMark(Image mark, Vector3 screen, bool visible)
    {
        if (mark == null)
        {
            return;
        }

        mark.enabled = visible;
        mark.rectTransform.position = screen;
    }

    static Image MakeBar(Transform parent, string name, Sprite sprite, float angle)
    {
        var item = new GameObject(name);
        item.transform.SetParent(parent, false);
        var image = item.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        image.color = new Color(1f, 0.16f, 0.12f, 0.95f);
        image.rectTransform.sizeDelta = new Vector2(LensRadius * 1.05f, 3f);
        image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        image.enabled = false;
        return image;
    }

    static Sprite WhiteSprite()
    {
        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.SetPixel(1, 0, Color.white);
        tex.SetPixel(0, 1, Color.white);
        tex.SetPixel(1, 1, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
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

    static Sprite RingSprite(float innerRadius, float outerRadius)
    {
        return ArcSprite(innerRadius, outerRadius, -180f, 180f);
    }

    static Sprite BottomBorderSprite(float innerRadius, float outerRadius)
    {
        return ArcSprite(innerRadius, outerRadius, -150f, -30f);
    }

    static Sprite ArcSprite(float innerRadius, float outerRadius, float startDegrees, float endDegrees)
    {
        const int size = 160;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var center = (size - 1) * 0.5f;
        var maxR = center - 0.5f;
        var inner = outerRadius <= 0.001f ? 0f : maxR * (innerRadius / outerRadius);
        if (inner < 0f)
        {
            inner = 0f;
        }

        var start = startDegrees * Mathf.Deg2Rad;
        var end = endDegrees * Mathf.Deg2Rad;
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist = Mathf.Sqrt((dx * dx) + (dy * dy));
                var angle = Mathf.Atan2(dy, dx);
                var onArc = dist <= maxR && dist >= inner && angle >= start && angle <= end;
                tex.SetPixel(x, y, onArc ? Color.white : new Color(1f, 1f, 1f, 0f));
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
