using UnityEngine;

public class MyMath
{
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
}
