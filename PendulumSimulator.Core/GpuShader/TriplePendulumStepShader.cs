using ComputeSharp;

namespace PendulumSimulator.Core.GpuShader
{
    [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
    [GeneratedComputeShaderDescriptor]
    internal readonly partial struct TriplePendulumStepShader(
        ReadWriteBuffer<float> states,
        float mass0,
        float mass1,
        float mass2,
        float length0,
        float length1,
        float length2,
        float dt,
        int steps,
        int systemCount) : IComputeShader
    {
        const float G = 9.80665f;
        const float Pi = 3.14159265358979323846f;
        const float TwoPi = 6.28318530717958647692f;
        const int StateStride = 6;

        public void Execute()
        {
            int sampleIndex = ThreadIds.X + ThreadIds.Y * DispatchSize.X;
            if (sampleIndex >= systemCount)
                return;

            int offset = sampleIndex * StateStride;

            float theta0 = states[offset + 0];
            float theta1 = states[offset + 1];
            float theta2 = states[offset + 2];
            float omega0 = states[offset + 3];
            float omega1 = states[offset + 4];
            float omega2 = states[offset + 5];

            for (int step = 0; step < steps; step++)
            {
                Float3 a1 = Acceleration(theta0, theta1, theta2, omega0, omega1, omega2);

                float omega20 = omega0 + a1.X * dt * 0.5f;
                float omega21 = omega1 + a1.Y * dt * 0.5f;
                float omega22 = omega2 + a1.Z * dt * 0.5f;
                Float3 a2 = Acceleration(
                    theta0 + omega0 * dt * 0.5f,
                    theta1 + omega1 * dt * 0.5f,
                    theta2 + omega2 * dt * 0.5f,
                    omega20,
                    omega21,
                    omega22);

                float omega30 = omega0 + a2.X * dt * 0.5f;
                float omega31 = omega1 + a2.Y * dt * 0.5f;
                float omega32 = omega2 + a2.Z * dt * 0.5f;
                Float3 a3 = Acceleration(
                    theta0 + omega20 * dt * 0.5f,
                    theta1 + omega21 * dt * 0.5f,
                    theta2 + omega22 * dt * 0.5f,
                    omega30,
                    omega31,
                    omega32);

                float omega40 = omega0 + a3.X * dt;
                float omega41 = omega1 + a3.Y * dt;
                float omega42 = omega2 + a3.Z * dt;
                Float3 a4 = Acceleration(
                    theta0 + omega30 * dt,
                    theta1 + omega31 * dt,
                    theta2 + omega32 * dt,
                    omega40,
                    omega41,
                    omega42);

                theta0 += dt / 6.0f * (omega0 + 2.0f * omega20 + 2.0f * omega30 + omega40);
                theta1 += dt / 6.0f * (omega1 + 2.0f * omega21 + 2.0f * omega31 + omega41);
                theta2 += dt / 6.0f * (omega2 + 2.0f * omega22 + 2.0f * omega32 + omega42);
                omega0 += dt / 6.0f * (a1.X + 2.0f * a2.X + 2.0f * a3.X + a4.X);
                omega1 += dt / 6.0f * (a1.Y + 2.0f * a2.Y + 2.0f * a3.Y + a4.Y);
                omega2 += dt / 6.0f * (a1.Z + 2.0f * a2.Z + 2.0f * a3.Z + a4.Z);
            }

            states[offset + 0] = NormalizeAngle(theta0);
            states[offset + 1] = NormalizeAngle(theta1);
            states[offset + 2] = NormalizeAngle(theta2);
            states[offset + 3] = omega0;
            states[offset + 4] = omega1;
            states[offset + 5] = omega2;
        }

        Float3 Acceleration(float theta0, float theta1, float theta2, float omega0, float omega1, float omega2)
        {
            float tailMass0 = mass0 + mass1 + mass2;
            float tailMass1 = mass1 + mass2;
            float tailMass2 = mass2;

            float sin0 = Hlsl.Sin(theta0);
            float sin1 = Hlsl.Sin(theta1);
            float sin2 = Hlsl.Sin(theta2);

            float delta01 = theta0 - theta1;
            float delta02 = theta0 - theta2;
            float delta12 = theta1 - theta2;
            float sin01 = Hlsl.Sin(delta01);
            float sin02 = Hlsl.Sin(delta02);
            float sin12 = Hlsl.Sin(delta12);
            float cos01 = Hlsl.Cos(delta01);
            float cos02 = Hlsl.Cos(delta02);
            float cos12 = Hlsl.Cos(delta12);

            float m00 = tailMass0 * length0 * length0;
            float m01 = tailMass1 * length0 * length1 * cos01;
            float m02 = tailMass2 * length0 * length2 * cos02;
            float m11 = tailMass1 * length1 * length1;
            float m12 = tailMass2 * length1 * length2 * cos12;
            float m22 = tailMass2 * length2 * length2;

            float rhs0 =
                -G * length0 * tailMass0 * sin0
                - tailMass1 * length0 * length1 * sin01 * omega1 * omega1
                - tailMass2 * length0 * length2 * sin02 * omega2 * omega2;
            float rhs1 =
                -G * length1 * tailMass1 * sin1
                + tailMass1 * length1 * length0 * sin01 * omega0 * omega0
                - tailMass2 * length1 * length2 * sin12 * omega2 * omega2;
            float rhs2 =
                -G * length2 * tailMass2 * sin2
                + tailMass2 * length2 * length0 * sin02 * omega0 * omega0
                + tailMass2 * length2 * length1 * sin12 * omega1 * omega1;

            float determinant = Det3(
                m00, m01, m02,
                m01, m11, m12,
                m02, m12, m22);
            float alpha0 = Det3(
                rhs0, m01, m02,
                rhs1, m11, m12,
                rhs2, m12, m22) / determinant;
            float alpha1 = Det3(
                m00, rhs0, m02,
                m01, rhs1, m12,
                m02, rhs2, m22) / determinant;
            float alpha2 = Det3(
                m00, m01, rhs0,
                m01, m11, rhs1,
                m02, m12, rhs2) / determinant;

            return new Float3(alpha0, alpha1, alpha2);
        }

        static float Det3(
            float a00, float a01, float a02,
            float a10, float a11, float a12,
            float a20, float a21, float a22)
        {
            return
                a00 * (a11 * a22 - a12 * a21)
                - a01 * (a10 * a22 - a12 * a20)
                + a02 * (a10 * a21 - a11 * a20);
        }

        static float NormalizeAngle(float angle)
        {
            float shifted = angle + Pi;
            shifted -= TwoPi * Hlsl.Floor(shifted / TwoPi);
            return shifted - Pi;
        }
    }
}
