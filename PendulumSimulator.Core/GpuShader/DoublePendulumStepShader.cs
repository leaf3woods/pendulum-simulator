using ComputeSharp;

namespace PendulumSimulator.Core.GpuShader
{
    [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
    [GeneratedComputeShaderDescriptor]
    internal readonly partial struct DoublePendulumStepShader(
        ReadWriteBuffer<float> states,
        float mass0,
        float mass1,
        float length0,
        float length1,
        float dt,
        int steps,
        int systemCount) : IComputeShader
    {
        const float G = 9.80665f;
        const float Pi = 3.14159265358979323846f;
        const float TwoPi = 6.28318530717958647692f;
        const int StateStride = 4;
        public void Execute()
        {
            int sampleIndex = ThreadIds.X + ThreadIds.Y * DispatchSize.X;
            if (sampleIndex >= systemCount)
                return;

            int offset = sampleIndex * StateStride;

            float theta0 = states[offset + 0];
            float theta1 = states[offset + 1];
            float omega0 = states[offset + 2];
            float omega1 = states[offset + 3];

            for (int step = 0; step < steps; step++)
            {
                Float4 k1 = Derivative(theta0, theta1, omega0, omega1);
                Float4 k2 = Derivative(
                    theta0 + k1.X * dt * 0.5f,
                    theta1 + k1.Y * dt * 0.5f,
                    omega0 + k1.Z * dt * 0.5f,
                    omega1 + k1.W * dt * 0.5f);
                Float4 k3 = Derivative(
                    theta0 + k2.X * dt * 0.5f,
                    theta1 + k2.Y * dt * 0.5f,
                    omega0 + k2.Z * dt * 0.5f,
                    omega1 + k2.W * dt * 0.5f);
                Float4 k4 = Derivative(
                    theta0 + k3.X * dt,
                    theta1 + k3.Y * dt,
                    omega0 + k3.Z * dt,
                    omega1 + k3.W * dt);

                theta0 += dt / 6.0f * (k1.X + 2.0f * k2.X + 2.0f * k3.X + k4.X);
                theta1 += dt / 6.0f * (k1.Y + 2.0f * k2.Y + 2.0f * k3.Y + k4.Y);
                omega0 += dt / 6.0f * (k1.Z + 2.0f * k2.Z + 2.0f * k3.Z + k4.Z);
                omega1 += dt / 6.0f * (k1.W + 2.0f * k2.W + 2.0f * k3.W + k4.W);
            }

            states[offset + 0] = NormalizeAngle(theta0);
            states[offset + 1] = NormalizeAngle(theta1);
            states[offset + 2] = omega0;
            states[offset + 3] = omega1;
        }

        Float4 Derivative(float theta0, float theta1, float omega0, float omega1)
        {
            float tailMass0 = mass0 + mass1;
            float delta = theta0 - theta1;
            float sinDelta = Hlsl.Sin(delta);
            float cosDelta = Hlsl.Cos(delta);

            float m00 = tailMass0 * length0 * length0;
            float m01 = mass1 * length0 * length1 * cosDelta;
            float m10 = m01;
            float m11 = mass1 * length1 * length1;

            float rhs0 =
                -G * length0 * tailMass0 * Hlsl.Sin(theta0)
                - mass1 * length0 * length1 * sinDelta * omega1 * omega1;
            float rhs1 =
                -G * length1 * mass1 * Hlsl.Sin(theta1)
                + mass1 * length1 * length0 * sinDelta * omega0 * omega0;

            float determinant = m00 * m11 - m01 * m10;
            float alpha0 = (rhs0 * m11 - m01 * rhs1) / determinant;
            float alpha1 = (m00 * rhs1 - m10 * rhs0) / determinant;

            return new Float4(omega0, omega1, alpha0, alpha1);
        }

        static float NormalizeAngle(float angle)
        {
            float shifted = angle + Pi;
            shifted -= TwoPi * Hlsl.Floor(shifted / TwoPi);
            return shifted - Pi;
        }
    }
}
