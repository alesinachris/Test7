using System;
using System.Collections.Generic;
using System.Windows;

namespace LocatorLib
{
    public class Target : SomePoint
    {
        private List<SensorData> sensorsData;

        public Target() { }

        public Target(List<SensorData> sensorsData) => this.sensorsData = sensorsData;

        public Target(Point point) => Point = point;

        public void SkipSensorsData() => sensorsData = null;

        public List<SensorData> GetSensorsData() => sensorsData;
    }
}
