using UnityEngine;

public static class ShotAim
{
    public const float MinimumRange = 0.35f;

    public static bool TryPlanePoint(Vector3 rayOrigin, Vector3 rayDirection, float planeY, out Vector3 point)
    {
        point = new Vector3(rayOrigin.x, planeY, rayOrigin.z);
        if (Mathf.Abs(rayDirection.y) < 0.0001f)
        {
            return false;
        }

        var distance = (planeY - rayOrigin.y) / rayDirection.y;
        if (distance < 0f)
        {
            return false;
        }

        point = rayOrigin + rayDirection * distance;
        return true;
    }

    public static Vector3 CursorPoint(Vector3 muzzle, Vector3 fallbackForward)
    {
        return ScreenPoint(Input.mousePosition, muzzle, fallbackForward);
    }

    public static Vector3 ScreenPoint(Vector2 screen, Vector3 muzzle, Vector3 fallbackForward)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return muzzle + fallbackForward;
        }

        var ray = camera.ScreenPointToRay(screen);
        Vector3 point;
        if (!TryPlanePoint(ray.origin, ray.direction, muzzle.y, out point))
        {
            return muzzle + fallbackForward;
        }

        return point;
    }

    public static Vector3 Direction(Vector3 muzzle, Vector3 cursor, Vector3 fallbackForward)
    {
        var direction = cursor - muzzle;
        direction.y = 0f;
        if (direction.sqrMagnitude < MinimumRange * MinimumRange)
        {
            return FlatForward(fallbackForward);
        }

        return direction.normalized;
    }

    public static float YawOffset(Vector3 muzzleForward, Vector3 shotDirection)
    {
        var forward = muzzleForward;
        forward.y = 0f;
        var shot = shotDirection;
        shot.y = 0f;
        if (forward.sqrMagnitude < 0.0001f || shot.sqrMagnitude < 0.0001f)
        {
            return 0f;
        }

        return Vector3.SignedAngle(forward.normalized, shot.normalized, Vector3.up);
    }

    public static Quaternion BeamLocalRotation(float aimYaw)
    {
        return Quaternion.Euler(0f, aimYaw, 0f);
    }

    static Vector3 FlatForward(Vector3 fallbackForward)
    {
        var fallback = fallbackForward;
        fallback.y = 0f;
        if (fallback.sqrMagnitude < 0.0001f)
        {
            return new Vector3(0f, 0f, 1f);
        }

        return fallback.normalized;
    }
}
