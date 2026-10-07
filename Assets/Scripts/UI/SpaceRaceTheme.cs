using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SpaceRaceTheme
{
    public static readonly Color Text = new Color(0.91f, 0.95f, 0.94f, 1f);
    public static readonly Color Orange = new Color(0.88f, 0.48f, 0.18f, 1f);
    public static readonly Color Teal = new Color(0.22f, 0.78f, 0.72f, 1f);
    public static readonly Color Panel = new Color(0.06f, 0.09f, 0.13f, 0.92f);
    public static readonly Color ButtonNormal = new Color(0.10f, 0.16f, 0.21f, 1f);
    public static readonly Color ButtonHighlight = new Color(0.16f, 0.42f, 0.42f, 1f);
    public static readonly Color ButtonPressed = new Color(0.62f, 0.32f, 0.12f, 1f);
    public static readonly Color ButtonDisabled = new Color(0.12f, 0.14f, 0.16f, 0.55f);
    public static readonly Color Health = new Color(0.86f, 0.22f, 0.24f, 1f);
    public static readonly Color Stamina = new Color(0.93f, 0.55f, 0.20f, 1f);
    public static readonly Color Energy = new Color(0.18f, 0.75f, 0.78f, 1f);
    public static readonly Color RedTeam = new Color(0.86f, 0.22f, 0.24f, 1f);
    public static readonly Color BlueTeam = new Color(0.25f, 0.48f, 0.95f, 1f);
    public static readonly Color Success = new Color(0.22f, 0.72f, 0.48f, 1f);
    public static readonly Color Failure = new Color(0.82f, 0.22f, 0.24f, 1f);

    public static Font BodyFont { get; private set; }
    public static Font TitleFont { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        LoadFonts();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void LoadFonts()
    {
        if (BodyFont == null)
        {
            BodyFont = Resources.Load<Font>("Fonts/Orbitron-Regular");
        }

        if (TitleFont == null)
        {
            TitleFont = Resources.Load<Font>("Fonts/Orbitron-Bold");
        }

        if (TitleFont == null)
        {
            TitleFont = BodyFont;
        }
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(scene);
    }

    public static void Apply(Scene scene)
    {
        LoadFonts();
        foreach (var root in scene.GetRootGameObjects())
        {
            Apply(root);
        }

        EnsureInstruction(scene);
    }

    public static void Apply(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        LoadFonts();
        var texts = root.GetComponentsInChildren<Text>(true);
        for (var i = 0; i < texts.Length; i++)
        {
            StyleText(texts[i]);
        }

        var buttons = root.GetComponentsInChildren<Button>(true);
        for (var i = 0; i < buttons.Length; i++)
        {
            StyleButton(buttons[i]);
        }

        var dropdowns = root.GetComponentsInChildren<Dropdown>(true);
        for (var i = 0; i < dropdowns.Length; i++)
        {
            StyleSelectable(dropdowns[i]);
        }

        var fields = root.GetComponentsInChildren<InputField>(true);
        for (var i = 0; i < fields.Length; i++)
        {
            StyleInput(fields[i]);
        }

        var images = root.GetComponentsInChildren<Image>(true);
        for (var i = 0; i < images.Length; i++)
        {
            StylePanel(images[i]);
        }
    }

    public static void StyleText(Text text)
    {
        if (text == null)
        {
            return;
        }

        var font = BodyFont;
        if (font != null)
        {
            text.font = font;
        }

        text.text = RewriteForScene(text.text);

        if (text.text != null && text.text.Trim() == "SPACE RACE!")
        {
            text.color = Orange;
            if (TitleFont != null)
            {
                text.font = TitleFont;
            }

            return;
        }

        if (IsDarkNeutral(text.color))
        {
            text.color = Text;
        }
    }

    public static string RewriteForScene(string value)
    {
        var rewritten = SpaceRaceCopy.Rewrite(value);
        if (string.IsNullOrEmpty(rewritten))
        {
            return rewritten;
        }

        var scene = SceneManager.GetActiveScene();
        var trimmed = rewritten.Trim();
        if (scene.name == "MatchLobby" && trimmed == "QUIT")
        {
            return "Leave lobby";
        }

        if (scene.name != "MatchLobby" && trimmed == "Quit")
        {
            return "Leave match";
        }

        return rewritten;
    }

    public static void StyleButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        var image = button.targetGraphic as Image;
        if (image != null && !IsBuiltInSprite(image.sprite))
        {
            return;
        }

        StyleSelectable(button);
        if (image != null)
        {
            image.color = Color.white;
        }

        var label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            label.color = Text;
        }
    }

    public static void StyleSelectable(Selectable selectable)
    {
        if (selectable == null)
        {
            return;
        }

        var colors = selectable.colors;
        colors.normalColor = ButtonNormal;
        colors.highlightedColor = ButtonHighlight;
        colors.pressedColor = ButtonPressed;
        colors.disabledColor = ButtonDisabled;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        selectable.colors = colors;
    }

    static void StyleInput(InputField field)
    {
        if (field == null)
        {
            return;
        }

        var image = field.targetGraphic as Image;
        if (image != null && IsBuiltInSprite(image.sprite))
        {
            image.color = Panel;
        }

        if (field.textComponent != null)
        {
            field.textComponent.color = Text;
            if (BodyFont != null)
            {
                field.textComponent.font = BodyFont;
            }
        }

        if (field.placeholder != null)
        {
            var placeholder = field.placeholder as Text;
            if (placeholder != null)
            {
                placeholder.color = new Color(Text.r, Text.g, Text.b, 0.45f);
                if (BodyFont != null)
                {
                    placeholder.font = BodyFont;
                }
            }
        }
    }

    static void StylePanel(Image image)
    {
        if (image == null || !IsBuiltInSprite(image.sprite))
        {
            return;
        }

        if (image.GetComponent<Button>() != null || image.GetComponent<Selectable>() != null)
        {
            return;
        }

        if (image.GetComponentInParent<Slider>() != null || image.GetComponentInParent<Scrollbar>() != null)
        {
            return;
        }

        var name = image.gameObject.name;
        if (name.IndexOf("Team", System.StringComparison.Ordinal) >= 0)
        {
            return;
        }

        if (name.IndexOf("Panel", System.StringComparison.Ordinal) < 0 &&
            name.IndexOf("Background", System.StringComparison.Ordinal) < 0)
        {
            return;
        }

        image.color = Panel;
    }

    public static bool IsBuiltInSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return true;
        }

        var name = sprite.name;
        return name == "Background" ||
               name == "UISprite" ||
               name == "Knob" ||
               name == "InputFieldBackground" ||
               name == "UIMask" ||
               name == "Checkmark";
    }

    public static bool IsDarkNeutral(Color color)
    {
        var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        var min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        return max < 0.45f && (max - min) < 0.12f;
    }

    static void EnsureInstruction(Scene scene)
    {
        var instruction = SpaceRaceCopy.ConsoleInstruction(scene.name);
        if (string.IsNullOrEmpty(instruction))
        {
            return;
        }

        var canvases = Object.FindObjectsOfType<Canvas>();
        Canvas screenCanvas = null;
        for (var i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].gameObject.scene != scene)
            {
                continue;
            }

            if (canvases[i].renderMode == RenderMode.WorldSpace)
            {
                continue;
            }

            screenCanvas = canvases[i];
            break;
        }

        if (screenCanvas == null || screenCanvas.transform.Find("SpaceRaceInstruction") != null)
        {
            return;
        }

        var text = SpaceRaceWidgets.CreateText(
            screenCanvas.transform,
            "SpaceRaceInstruction",
            instruction,
            16,
            Teal,
            TextAnchor.MiddleCenter);
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.08f, 0.9f);
        rect.anchorMax = new Vector2(0.92f, 0.98f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
    }
}
