using System.Windows;

namespace LocatorLib
{
    abstract public class SomePoint
    {
        public Point Point { get; set; }

        public override string ToString() => GetType().Name + " X:" + Point.X + "; " + "Y:" + Point.Y;
    }
}
