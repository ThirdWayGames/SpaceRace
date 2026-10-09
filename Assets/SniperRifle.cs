using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class SniperRifle : Weapon
{
    public float ScopedZoom = 5.5f;

    public Sprite ScopeCrosshair;

    bool scoping;

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
            follow.SetScoped(isAltFire, ScopedZoom);
        }

        SniperScopeView.Set(isAltFire, ScopeCrosshair);
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
            follow.SetScoped(false, ScopedZoom);
        }

        SniperScopeView.Set(false, ScopeCrosshair);
    }
}

public class SniperScopeView : MonoBehaviour
{
    static SniperScopeView active;

    Image ring;
    Image cross;
    Image upright;

    public static void Set(bool on, Sprite sprite)
    {
        if (!on)
        {
            if (active != null)
            {
                active.gameObject.SetActive(false);
            }

            return;
        }

        Ensure(sprite);
        active.gameObject.SetActive(true);
        if (sprite != null)
        {
            active.ring.sprite = sprite;
            active.ring.color = Color.white;
            active.cross.gameObject.SetActive(false);
            active.upright.gameObject.SetActive(false);
        }

        active.FollowCursor();
    }

    static void Ensure(Sprite sprite)
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

        active.ring = Mark(canvasObject.transform, "ScopeRing", sprite, 280f);
        active.cross = Mark(canvasObject.transform, "ScopeCross", null, 150f);
        active.cross.color = new Color(0.45f, 1f, 0.85f, 0.95f);
        active.upright = Mark(canvasObject.transform, "ScopeCrossUpright", null, 150f);
        active.upright.color = active.cross.color;
    }

    static Image Mark(Transform parent, string name, Sprite sprite, float size)
    {
        var item = new GameObject(name);
        item.transform.SetParent(parent, false);
        var image = item.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        image.preserveAspect = true;
        if (sprite == null)
        {
            image.color = new Color(0.45f, 1f, 0.85f, 0.95f);
        }

        var rect = image.rectTransform;
        rect.sizeDelta = new Vector2(size, size);
        return image;
    }

    void Update()
    {
        FollowCursor();
    }

    void FollowCursor()
    {
        if (ring != null)
        {
            ring.rectTransform.position = Input.mousePosition;
        }

        if (cross != null)
        {
            cross.rectTransform.position = Input.mousePosition;
            cross.rectTransform.localScale = new Vector3(1f, 0.08f, 1f);
        }

        if (upright != null)
        {
            upright.rectTransform.position = Input.mousePosition;
            upright.rectTransform.localScale = new Vector3(0.08f, 1f, 1f);
        }
    }
}
