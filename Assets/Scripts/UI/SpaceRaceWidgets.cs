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

    public const float HowToPadX = 28f;

    public const float HowToPadTop = 52f;

    public const float HowToPadBottom = 22f;

    public static Vector2 HowToCardSize(float preferredTextWidth, float preferredTextHeight, float parentWidth)
    {
        var textWidth = preferredTextWidth < 40f ? 560f : preferredTextWidth;
        var cardWidth = textWidth + HowToPadX * 2f;
        var maxWidth = parentWidth > 320f ? parentWidth * 0.7f : 760f;
        if (maxWidth > 760f)
        {
            maxWidth = 760f;
        }

        if (maxWidth < 420f)
        {
            maxWidth = 420f;
        }

        if (cardWidth > maxWidth)
        {
            cardWidth = maxWidth;
        }

        if (cardWidth < 420f)
        {
            cardWidth = 420f;
        }

        var textHeight = preferredTextHeight;
        if (textHeight < 64f)
        {
            textHeight = 432f;
        }

        return new Vector2(cardWidth, HowToPadTop + textHeight + HowToPadBottom);
    }

    public static GameObject CreateHowToCard(Transform parent, Action onClose)
    {
        var image = CreatePanel(parent, "SpaceRaceHowTo", SpaceRaceTheme.Panel);
        var panel = image.gameObject;
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;

        var body = CreateText(panel.transform, "Body", SpaceRaceCopy.HowToPlay, 16, SpaceRaceTheme.Text, TextAnchor.UpperLeft);
        body.lineSpacing = 1f;
        body.horizontalOverflow = HorizontalWrapMode.Overflow;
        body.verticalOverflow = VerticalWrapMode.Overflow;

        var parentRect = parent as RectTransform;
        var parentWidth = parentRect != null ? parentRect.rect.width : 0f;
        var size = HowToCardSize(body.preferredWidth, 0f, parentWidth);
        var textWidth = size.x - HowToPadX * 2f;

        var bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 1f);
        bodyRect.anchorMax = new Vector2(0f, 1f);
        bodyRect.pivot = new Vector2(0f, 1f);
        bodyRect.anchoredPosition = new Vector2(HowToPadX, -HowToPadTop);
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyRect.sizeDelta = new Vector2(textWidth, 10f);
        var textHeight = body.preferredHeight + 8f;
        size = HowToCardSize(textWidth, textHeight, parentWidth);
        textWidth = size.x - HowToPadX * 2f;
        textHeight = size.y - HowToPadTop - HowToPadBottom;
        bodyRect.sizeDelta = new Vector2(textWidth, textHeight);
        rect.sizeDelta = size;

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
