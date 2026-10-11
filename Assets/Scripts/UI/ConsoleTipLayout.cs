public static class ConsoleTipLayout
{
    public const float Padding = 24f;

    public const float FallbackScaleX = 0.06255645f;

    public static float TextWidth(float preferredWidth, int characterCount)
    {
        if (characterCount < 0)
        {
            characterCount = 0;
        }

        var estimate = characterCount * 10f;
        if (estimate < 64f)
        {
            estimate = 64f;
        }

        var width = preferredWidth;
        if (IsUnusable(width) || width < estimate * 0.85f || width > estimate * 1.6f)
        {
            width = estimate;
        }

        return width + Padding;
    }

    public static float CanvasWidth(float textWidth, float textScaleX, float panelScaleX, float currentCanvasWidth)
    {
        var scale = Abs(textScaleX) * Abs(panelScaleX);
        if (scale < 0.0001f)
        {
            scale = FallbackScaleX;
        }

        var width = textWidth * scale;
        if (!IsUnusable(currentCanvasWidth) && currentCanvasWidth > width)
        {
            return currentCanvasWidth;
        }

        return width;
    }

    static bool IsUnusable(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) || value < 1f;
    }

    static float Abs(float value)
    {
        return value < 0f ? -value : value;
    }
}
