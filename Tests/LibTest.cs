using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using LocatorLib;
using System.IO;
using System.Windows;

namespace Tests
{
    public class LibTest
    {
        Locator loc;
        string[] inputData;
        string[] coordinates;

        [SetUp]
        public void Setup()
        {
            loc = new Locator();
            inputData = File.ReadAllLines("input.txt");
            coordinates = File.ReadAllLines("coordinates.txt");
        }

        /// <summary>
        /// methods
        /// CalculateTargetsFromLocatorsData
        /// CreateSensors
        /// CreateTargets
        /// SetPointFromSensorsData
        /// GetGoodPoint
        /// </summary>
        [Test]
        public void CalculateTargetsFromLocatorsDataTest()
        {
            loc.CalculateTargetsFromLocatorsData(inputData);
            Assert.AreEqual(loc.Targets.Count, 10);
            Assert.AreEqual(loc.Sensors.Count, 3);
        }

        [Test]
        public void GetIntersectionPointsTest()
        {
            double koef1 = 1;
            double koef2 = 1;
            List<SensorData> sensorDataList = new List<SensorData>();
            SensorData sdCenter = new SensorData(new Point(0, 0), 0.00001000);
            SensorData sdLeft = new SensorData(new Point(-20, 0), 0.00001000);
            SensorData sdRight = new SensorData(new Point(10, 0), 0.00001000);
            SensorData sdTop = new SensorData(new Point(0, 30), 0.00001000);

            List<Point> points1 = loc.GetIntersectionPoints(koef1, koef2, new List<SensorData>() { sdCenter, sdLeft });
            Assert.AreEqual(points1.Count, 1);

            List<Point> points2 = loc.GetIntersectionPoints(koef1, koef2, new List<SensorData>() { sdCenter, sdRight });
            Assert.AreEqual(points2.Count, 2);

            List<Point> points3 = loc.GetIntersectionPoints(koef1, koef2, new List<SensorData>() { sdCenter, sdTop });
            Assert.AreEqual(points3.Count, 0);
        }

        [Test]
        public void GetDistanceTest()
        {
            Point p1 = new Point(0, 0);
            Point p2 = new Point(4, 3);
            double d = loc.GetDistance(p1, p2);
            Assert.AreEqual(d, 5);
        }

        [Test]
        public void GetDelayByDistanceTest()
        {
            double delay = loc.GetDelayByDistance(10);
            Assert.AreEqual(delay, 0.00001000);
        }

        [Test]
        public void AddTargetTest()
        {
            Point p = new Point(4, 3);
            int beginCount = loc.Targets.Count;
            loc.AddTarget(p);
            Assert.AreEqual(beginCount + 1, loc.Targets.Count);
        }

        [Test]
        public void InputTargetPointsTest()
        {
            loc.InputTargetPoints(coordinates);
            Assert.AreEqual(loc.Targets.Count, 5);
        }

        [Test]
        public void ClearAllTest()
        {
            loc.ClearAll();
            Assert.AreEqual(loc.Targets.Count, 0);
            Assert.AreEqual(loc.Sensors.Count, 0);
        }

        [Test]
        public void AddSensorTest()
        {
            loc.AddSensor(new Point(1, 1));
            Assert.AreEqual(loc.Sensors.Count, 1);
            loc.AddSensor(new Point(2, 2));
            Assert.AreEqual(loc.Sensors.Count, 2);
            loc.AddSensor(new Point(3, 3));
            Assert.AreEqual(loc.Sensors.Count, 3);
            loc.AddSensor(new Point(4, 4));
            Assert.AreEqual(loc.Sensors.Count, 3);
        }
    }
}
