using UnityEngine;
using UnityEngine.UI;

public class ThrowPreview : MonoBehaviour
{
    public const float BarBelowPixels = 72f;

    static ThrowPreview active;
    static int ownerId;

    RectTransform barRoot;
    RectTransform barFill;
    LineRenderer ring;

    public static Vector2 BarScreenPosition(Vector3 playerScreen, float belowPixels)
    {
        var drop = belowPixels < 0f ? 0f : belowPixels;
        return new Vector2(playerScreen.x, playerScreen.y - drop);
    }

    public static void Show(Transform owner, Vector3 bodyPosition, Vector3 landing, float charge)
    {
        if (owner == null)
        {
            return;
        }

        Ensure();
        ownerId = owner.GetInstanceID();
        active.gameObject.SetActive(true);
        active.Place(bodyPosition, landing, Mathf.Clamp01(charge));
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
        var canvasObject = new GameObject("ThrowChargeCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        var back = new GameObject("ThrowChargeBar");
        back.transform.SetParent(canvasObject.transform, false);
        var backImage = back.AddComponent<Image>();
        backImage.color = new Color(0.08f, 0.09f, 0.1f, 0.9f);
        backImage.raycastTarget = false;
        barRoot = backImage.rectTransform;
        barRoot.sizeDelta = new Vector2(150f, 14f);
        barRoot.pivot = new Vector2(0.5f, 0.5f);

        var fill = new GameObject("ThrowChargeFill");
        fill.transform.SetParent(back.transform, false);
        var fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(1f, 0.55f, 0.16f, 1f);
        fillImage.raycastTarget = false;
        barFill = fillImage.rectTransform;
        barFill.pivot = new Vector2(0f, 0.5f);
        barFill.anchorMin = new Vector2(0f, 0.15f);
        barFill.anchorMax = new Vector2(0f, 0.85f);
        barFill.anchoredPosition = new Vector2(4f, 0f);
        barFill.sizeDelta = new Vector2(0f, 0f);

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
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.38f, 0.05f, Mathf.Sin(angle) * 0.38f));
        }
    }

    void Place(Vector3 bodyPosition, Vector3 landing, float charge)
    {
        var camera = Camera.main;
        if (camera != null)
        {
            var screen = camera.WorldToScreenPoint(bodyPosition);
            var bar = BarScreenPosition(screen, BarBelowPixels);
            barRoot.gameObject.SetActive(screen.z > 0f);
            barRoot.position = new Vector3(bar.x, bar.y, 0f);
        }

        var width = Mathf.Max(4f, (barRoot.sizeDelta.x - 8f) * Mathf.Max(0.04f, charge));
        barFill.sizeDelta = new Vector2(width, 0f);

        ring.transform.position = landing;
        ring.transform.rotation = Quaternion.identity;
    }
}
