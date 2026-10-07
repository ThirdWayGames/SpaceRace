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
        image.color = Color.white;
        image.sprite = BuiltinSprite();
        image.type = Image.Type.Sliced;
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

    static Sprite BuiltinSprite()
    {
        var sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null)
        {
            sprite = Resources.GetBuiltinResource<Sprite>("UISprite");
        }

        return sprite;
    }
}
