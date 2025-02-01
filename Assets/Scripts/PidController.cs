//PID controller: https://discussions.unity.com/t/rigidbody-lookat-torque/483110/7

using UnityEngine;

public class PidController  // Proportional Integral Derivative controller
{
    public static float TargetAngle;
    public static Vector3 Torque;

    float Kp = 1;
    float Ki;
    float Kd = .1f;
    float P, I, D;
    float prevError;

    public PidController(float Kp, float Kd, float Ki = 0)
    {
        this.Kp = Kp;
        this.Ki = Ki;
        this.Kd = Kd;
    }

    public float GetOutput(float currentError, float deltaTime)
    {
        P = currentError;
        I += P * deltaTime;
        D = (P - prevError) / deltaTime;
        prevError = currentError;

        return P * Kp + I * Ki + D * Kd;
    }
}
