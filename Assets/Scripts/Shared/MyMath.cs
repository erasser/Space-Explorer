using UnityEngine;

public static class MyMath
{
    public const float TwoPI = 2 * Mathf.PI;

    public static Vector3 SetVectorLength(Vector3 vector, float length)
    {
        return vector.normalized * length;
    }

    public static Vector3 SetVectorYToZero(Vector3 vector, bool maintainMagnitude = false)
    {
        if (!maintainMagnitude)
            return new(vector.x, 0, vector.z);

        return SetVectorLength(new(vector.x, 0, vector.z), vector.magnitude);
    }

    public static Vector3 ClampVectorToSqrLength(Vector3 vector, float maxSqrLength)
    {
        if (vector.sqrMagnitude > maxSqrLength)
            return SetVectorLength(vector, Mathf.Sqrt(maxSqrLength));

        return vector;
    }

    public static Vector3 RaycastPlane(Plane plane, Camera cam)
    {
        return RaycastPlane(plane, cam, Input.mousePosition);
    }

    public static Vector3 RaycastPlane(Plane plane, Ray ray)
    {
        plane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }

    public static Vector3 RaycastPlane(Plane plane, Camera camera, Vector3 screenPoint)
    {
        var ray = camera.ScreenPointToRay(screenPoint);
        return RaycastPlane(plane, ray);
    }
    
}
