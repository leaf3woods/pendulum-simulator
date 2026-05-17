using PendulumSimulator.Core.PhysicsSystem;

namespace PendulumSimulator.Viewer.Rendering
{
    public static class PendulumStateColorMap
    {
        public static RgbColor Map(PendulumSystem system, PendulumColorScheme scheme)
        {
            byte r = AngleToByte(system[0].Theta);
            byte g = AngleToByte(system.Count > 1 ? system[1].Theta : system[0].Theta);
            byte b = AngleToByte(system.Count > 1 ? system[1].Theta - system[0].Theta : 0.0);

            if (scheme == PendulumColorScheme.GrayscaleAngles)
            {
                byte gray = (byte)((r + g + b) / 3);
                return new RgbColor(gray, gray, gray);
            }

            return new RgbColor(r, g, b);
        }

        static byte AngleToByte(double angle)
        {
            angle = NormalizeAngle(angle);
            double value = (angle + Math.PI) / (2.0 * Math.PI) * 255.0;
            return (byte)Math.Clamp(value, 0, 255);
        }

        static double NormalizeAngle(double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle < -Math.PI) angle += 2 * Math.PI;
            return angle;
        }
    }
}
