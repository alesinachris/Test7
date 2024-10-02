using System.Windows;

namespace LocatorLib
{
    public class SensorData : SomePoint
    {
        public double Delay { get; set; }

        public SensorData(Point point, double delay)
        {
            Point = point;
            Delay = delay;
        }

        public override string ToString() => GetType().Name + " X:" + Point.X + "; " + "Y:" + Point.Y + " delay: " + string.Format("{0:N8}", Delay);
    }
}
