using System.Globalization;
using UnityEngine;

public static class TurretAim
{
    public const float Range = 60f;

    public const float ArcDegrees = 15f;

    public const float Damage = 5f;

    public static string Pick(float shipX, float shipZ, float headingDegrees, string contactSummary)
    {
        if (string.IsNullOrEmpty(contactSummary))
        {
            return string.Empty;
        }

        var bestId = string.Empty;
        var bestDistance = Range;
        var records = contactSummary.Split(';');
        for (int i = 0; i < records.Length; i++)
        {
            var fields = records[i].Split(',');
            if (fields.Length < 6)
            {
                continue;
            }

            if (fields[3] != GalaxyContactKind.Hostile.ToString() || fields[4] != "1")
            {
                continue;
            }

            float health;
            if (!float.TryParse(fields[5], NumberStyles.Float, CultureInfo.InvariantCulture, out health) || health <= 0f)
            {
                continue;
            }

            float x;
            float z;
            if (!float.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out x) || !float.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
            {
                continue;
            }

            var dx = x - shipX;
            var dz = z - shipZ;
            var distance = Mathf.Sqrt((dx * dx) + (dz * dz));
            if (distance > Range || distance > bestDistance)
            {
                continue;
            }

            var bearing = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
            if (Mathf.Abs(Mathf.DeltaAngle(headingDegrees, bearing)) > ArcDegrees)
            {
                continue;
            }

            bestDistance = distance;
            bestId = fields[0];
        }

        return bestId;
    }
}
