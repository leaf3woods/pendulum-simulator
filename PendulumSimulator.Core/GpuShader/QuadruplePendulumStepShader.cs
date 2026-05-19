using ComputeSharp;

namespace PendulumSimulator.Core.GpuShader
{
    [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
    [GeneratedComputeShaderDescriptor]
    internal readonly partial struct QuadruplePendulumStepShader(
        ReadWriteBuffer<float> states,
        float mass0,
        float mass1,
        float mass2,
        float mass3,
        float length0,
        float length1,
        float length2,
        float length3,
        float dt,
        int steps,
        int systemCount) : IComputeShader
    {
        const float G = 9.80665f;
        const float Pi = 3.14159265358979323846f;
        const float TwoPi = 6.28318530717958647692f;
        const int StateStride = 8;

        public void Execute()
        {
            int sampleIndex = ThreadIds.X + ThreadIds.Y * DispatchSize.X;
            if (sampleIndex >= systemCount)
                return;

            int offset = sampleIndex * StateStride;

            float theta0 = states[offset + 0];
            float theta1 = states[offset + 1];
            float theta2 = states[offset + 2];
            float theta3 = states[offset + 3];
            float omega0 = states[offset + 4];
            float omega1 = states[offset + 5];
            float omega2 = states[offset + 6];
            float omega3 = states[offset + 7];

            for (int step = 0; step < steps; step++)
            {
                Float4 a1 = Acceleration(theta0, theta1, theta2, theta3, omega0, omega1, omega2, omega3);

                float omega20 = omega0 + a1.X * dt * 0.5f;
                float omega21 = omega1 + a1.Y * dt * 0.5f;
                float omega22 = omega2 + a1.Z * dt * 0.5f;
                float omega23 = omega3 + a1.W * dt * 0.5f;
                Float4 a2 = Acceleration(
                    theta0 + omega0 * dt * 0.5f,
                    theta1 + omega1 * dt * 0.5f,
                    theta2 + omega2 * dt * 0.5f,
                    theta3 + omega3 * dt * 0.5f,
                    omega20,
                    omega21,
                    omega22,
                    omega23);

                float omega30 = omega0 + a2.X * dt * 0.5f;
                float omega31 = omega1 + a2.Y * dt * 0.5f;
                float omega32 = omega2 + a2.Z * dt * 0.5f;
                float omega33 = omega3 + a2.W * dt * 0.5f;
                Float4 a3 = Acceleration(
                    theta0 + omega20 * dt * 0.5f,
                    theta1 + omega21 * dt * 0.5f,
                    theta2 + omega22 * dt * 0.5f,
                    theta3 + omega23 * dt * 0.5f,
                    omega30,
                    omega31,
                    omega32,
                    omega33);

                float omega40 = omega0 + a3.X * dt;
                float omega41 = omega1 + a3.Y * dt;
                float omega42 = omega2 + a3.Z * dt;
                float omega43 = omega3 + a3.W * dt;
                Float4 a4 = Acceleration(
                    theta0 + omega30 * dt,
                    theta1 + omega31 * dt,
                    theta2 + omega32 * dt,
                    theta3 + omega33 * dt,
                    omega40,
                    omega41,
                    omega42,
                    omega43);

                theta0 += dt / 6.0f * (omega0 + 2.0f * omega20 + 2.0f * omega30 + omega40);
                theta1 += dt / 6.0f * (omega1 + 2.0f * omega21 + 2.0f * omega31 + omega41);
                theta2 += dt / 6.0f * (omega2 + 2.0f * omega22 + 2.0f * omega32 + omega42);
                theta3 += dt / 6.0f * (omega3 + 2.0f * omega23 + 2.0f * omega33 + omega43);
                omega0 += dt / 6.0f * (a1.X + 2.0f * a2.X + 2.0f * a3.X + a4.X);
                omega1 += dt / 6.0f * (a1.Y + 2.0f * a2.Y + 2.0f * a3.Y + a4.Y);
                omega2 += dt / 6.0f * (a1.Z + 2.0f * a2.Z + 2.0f * a3.Z + a4.Z);
                omega3 += dt / 6.0f * (a1.W + 2.0f * a2.W + 2.0f * a3.W + a4.W);
            }

            states[offset + 0] = NormalizeAngle(theta0);
            states[offset + 1] = NormalizeAngle(theta1);
            states[offset + 2] = NormalizeAngle(theta2);
            states[offset + 3] = NormalizeAngle(theta3);
            states[offset + 4] = omega0;
            states[offset + 5] = omega1;
            states[offset + 6] = omega2;
            states[offset + 7] = omega3;
        }

        Float4 Acceleration(
            float theta0,
            float theta1,
            float theta2,
            float theta3,
            float omega0,
            float omega1,
            float omega2,
            float omega3)
        {
            float tailMass0 = mass0 + mass1 + mass2 + mass3;
            float tailMass1 = mass1 + mass2 + mass3;
            float tailMass2 = mass2 + mass3;
            float tailMass3 = mass3;

            float sin0 = Hlsl.Sin(theta0);
            float sin1 = Hlsl.Sin(theta1);
            float sin2 = Hlsl.Sin(theta2);
            float sin3 = Hlsl.Sin(theta3);

            float delta01 = theta0 - theta1;
            float delta02 = theta0 - theta2;
            float delta03 = theta0 - theta3;
            float delta12 = theta1 - theta2;
            float delta13 = theta1 - theta3;
            float delta23 = theta2 - theta3;
            float sin01 = Hlsl.Sin(delta01);
            float sin02 = Hlsl.Sin(delta02);
            float sin03 = Hlsl.Sin(delta03);
            float sin12 = Hlsl.Sin(delta12);
            float sin13 = Hlsl.Sin(delta13);
            float sin23 = Hlsl.Sin(delta23);
            float cos01 = Hlsl.Cos(delta01);
            float cos02 = Hlsl.Cos(delta02);
            float cos03 = Hlsl.Cos(delta03);
            float cos12 = Hlsl.Cos(delta12);
            float cos13 = Hlsl.Cos(delta13);
            float cos23 = Hlsl.Cos(delta23);

            float m00 = tailMass0 * length0 * length0;
            float m01 = tailMass1 * length0 * length1 * cos01;
            float m02 = tailMass2 * length0 * length2 * cos02;
            float m03 = tailMass3 * length0 * length3 * cos03;
            float m11 = tailMass1 * length1 * length1;
            float m12 = tailMass2 * length1 * length2 * cos12;
            float m13 = tailMass3 * length1 * length3 * cos13;
            float m22 = tailMass2 * length2 * length2;
            float m23 = tailMass3 * length2 * length3 * cos23;
            float m33 = tailMass3 * length3 * length3;

            float rhs0 =
                -G * length0 * tailMass0 * sin0
                - tailMass1 * length0 * length1 * sin01 * omega1 * omega1
                - tailMass2 * length0 * length2 * sin02 * omega2 * omega2
                - tailMass3 * length0 * length3 * sin03 * omega3 * omega3;
            float rhs1 =
                -G * length1 * tailMass1 * sin1
                + tailMass1 * length1 * length0 * sin01 * omega0 * omega0
                - tailMass2 * length1 * length2 * sin12 * omega2 * omega2
                - tailMass3 * length1 * length3 * sin13 * omega3 * omega3;
            float rhs2 =
                -G * length2 * tailMass2 * sin2
                + tailMass2 * length2 * length0 * sin02 * omega0 * omega0
                + tailMass2 * length2 * length1 * sin12 * omega1 * omega1
                - tailMass3 * length2 * length3 * sin23 * omega3 * omega3;
            float rhs3 =
                -G * length3 * tailMass3 * sin3
                + tailMass3 * length3 * length0 * sin03 * omega0 * omega0
                + tailMass3 * length3 * length1 * sin13 * omega1 * omega1
                + tailMass3 * length3 * length2 * sin23 * omega2 * omega2;

            return SolveSymmetricPositiveDefinite4(
                m00, m01, m02, m03,
                m11, m12, m13,
                m22, m23,
                m33,
                rhs0, rhs1, rhs2, rhs3);
        }

        static Float4 SolveSymmetricPositiveDefinite4(
            float m00,
            float m01,
            float m02,
            float m03,
            float m11,
            float m12,
            float m13,
            float m22,
            float m23,
            float m33,
            float rhs0,
            float rhs1,
            float rhs2,
            float rhs3)
        {
            float l00 = Hlsl.Sqrt(m00);
            float l10 = m01 / l00;
            float l20 = m02 / l00;
            float l30 = m03 / l00;

            float l11 = Hlsl.Sqrt(m11 - l10 * l10);
            float l21 = (m12 - l20 * l10) / l11;
            float l31 = (m13 - l30 * l10) / l11;

            float l22 = Hlsl.Sqrt(m22 - l20 * l20 - l21 * l21);
            float l32 = (m23 - l30 * l20 - l31 * l21) / l22;

            float l33 = Hlsl.Sqrt(m33 - l30 * l30 - l31 * l31 - l32 * l32);

            float y0 = rhs0 / l00;
            float y1 = (rhs1 - l10 * y0) / l11;
            float y2 = (rhs2 - l20 * y0 - l21 * y1) / l22;
            float y3 = (rhs3 - l30 * y0 - l31 * y1 - l32 * y2) / l33;

            float alpha3 = y3 / l33;
            float alpha2 = (y2 - l32 * alpha3) / l22;
            float alpha1 = (y1 - l21 * alpha2 - l31 * alpha3) / l11;
            float alpha0 = (y0 - l10 * alpha1 - l20 * alpha2 - l30 * alpha3) / l00;

            return new Float4(alpha0, alpha1, alpha2, alpha3);
        }

        static float NormalizeAngle(float angle)
        {
            float shifted = angle + Pi;
            shifted -= TwoPi * Hlsl.Floor(shifted / TwoPi);
            return shifted - Pi;
        }
    }
}
