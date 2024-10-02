using System.Windows;

namespace LocatorLib
{
    public class Sensor : SomePoint
    {
        public Sensor() { }

        public Sensor(Point p) => Point = p;

        public int Id { get; set; }
    }
}
