public static class SpawnVitals
{
    public static float RestoredEnergy(float current, float max)
    {
        if (float.IsNaN(current) || float.IsInfinity(current) || current <= 0f)
        {
            if (float.IsNaN(max) || float.IsInfinity(max) || max < 0f)
            {
                return 0f;
            }

            return max;
        }

        if (!float.IsNaN(max) && !float.IsInfinity(max) && max > 0f && current > max)
        {
            return max;
        }

        return current;
    }

    public static bool ShouldHideOverlay(float alpha, float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY)
    {
        if (alpha > 0.02f)
        {
            return false;
        }

        return anchorMinX <= 0.02f && anchorMinY <= 0.02f && anchorMaxX >= 0.98f && anchorMaxY >= 0.98f;
    }
}
