public static class LoadoutHands
{
    public static bool OccupiesBothHands(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName))
        {
            return false;
        }

        return resourceName.IndexOf("Sniper") >= 0;
    }

    public static void Assign(ref string left, ref string right, bool toLeft, string resource)
    {
        if (toLeft)
        {
            left = resource;
        }
        else
        {
            right = resource;
        }

        if (OccupiesBothHands(resource))
        {
            if (toLeft)
            {
                right = null;
            }
            else
            {
                left = null;
            }

            return;
        }

        if (toLeft && OccupiesBothHands(right))
        {
            right = null;
        }

        if (!toLeft && OccupiesBothHands(left))
        {
            left = null;
        }
    }
}
