using System;
using UnityEngine;
using UnityEngine.UI;

public static class SpaceRaceWidgets
{
    public static Text CreateText(Transform parent, string name, string value, int size, Color color, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        SpaceRaceTheme.LoadFonts();
        if (SpaceRaceTheme.BodyFont != null)
        {
            text.font = SpaceRaceTheme.BodyFont;
        }

        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    public static Button CreateButton(Transform parent, string name, string label, Action onClick)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = WhiteSprite();
        image.type = Image.Type.Simple;
        image.color = Color.white;
        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        SpaceRaceTheme.StyleSelectable(button);
        var text = CreateText(go.transform, "Text", label, 16, SpaceRaceTheme.Text, TextAnchor.MiddleCenter);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);
        if (onClick != null)
        {
            button.onClick.AddListener(() => onClick());
        }

        return button;
    }

    public static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    public static GameObject CreateHowToCard(Transform parent, Action onClose)
    {
        var image = CreatePanel(parent, "SpaceRaceHowTo", SpaceRaceTheme.Panel);
        var panel = image.gameObject;
        var rect = panel.GetComponent<RectTransform>();
        Stretch(rect, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.97f), Vector2.zero);
        panel.AddComponent<RectMask2D>();

        var body = CreateText(panel.transform, "Body", SpaceRaceCopy.HowToPlay, 14, SpaceRaceTheme.Text, TextAnchor.UpperLeft);
        body.lineSpacing = 0.85f;
        var bodyRect = body.rectTransform;
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = new Vector2(28f, 18f);
        bodyRect.offsetMax = new Vector2(-28f, -58f);

        var close = CreateButton(panel.transform, "CloseHowTo", "Close", onClose);
        var closeRect = close.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-14f, -12f);
        closeRect.sizeDelta = new Vector2(120f, 32f);
        return panel;
    }

    public static Image CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = WhiteSprite();
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = true;
        return image;
    }

    static Sprite whiteSprite;

    public static Sprite WhiteSprite()
    {
        if (whiteSprite != null)
        {
            return whiteSprite;
        }

        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var pixels = new Color[16];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.name = "SpaceRaceWhite";
        whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 100f);
        whiteSprite.name = "SpaceRaceWhite";
        return whiteSprite;
    }
}
