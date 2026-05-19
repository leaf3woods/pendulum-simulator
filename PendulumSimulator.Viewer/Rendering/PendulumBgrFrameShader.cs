using ComputeSharp;

namespace PendulumSimulator.Viewer.Rendering
{
    [ThreadGroupSize(DefaultThreadGroupSizes.XY)]
    [GeneratedComputeShaderDescriptor]
    internal readonly partial struct PendulumBgrFrameShader(
        ReadWriteBuffer<float> states,
        ReadWriteBuffer<int> frame,
        int systemCount,
        int grayscale) : IComputeShader
    {
        const float Pi = 3.14159265358979323846f;
        const float TwoPi = 6.28318530717958647692f;
        const int StateStride = 4;

        public void Execute()
        {
            int sampleIndex = ThreadIds.X + ThreadIds.Y * DispatchSize.X;
            if (sampleIndex >= systemCount)
                return;

            int stateOffset = sampleIndex * StateStride;
            int r = AngleToByte(states[stateOffset + 0]);
            int g = AngleToByte(states[stateOffset + 1]);
            int b = AngleToByte(states[stateOffset + 1] - states[stateOffset + 0]);

            if (grayscale != 0)
            {
                int gray = (r + g + b) / 3;
                r = gray;
                g = gray;
                b = gray;
            }

            frame[sampleIndex] = b + g * 256 + r * 65536;
        }

        static int AngleToByte(float angle)
        {
            angle = NormalizeAngle(angle);
            float value = (angle + Pi) / TwoPi * 255.0f;
            return (int)Hlsl.Clamp(value, 0.0f, 255.0f);
        }

        static float NormalizeAngle(float angle)
        {
            float shifted = angle + Pi;
            shifted -= TwoPi * Hlsl.Floor(shifted / TwoPi);
            return shifted - Pi;
        }
    }
}
