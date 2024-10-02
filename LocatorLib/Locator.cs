using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;


namespace LocatorLib
{
    public class Locator
    {
        #region Config

        readonly double lightSpeed = 1000000; //near a black hole
        readonly int maxSensorsCount = 3;
        readonly double precistion = .05; //not suitable for bombing

        #endregion Config

        #region Etc

        public delegate void MessageHandler(string message);
        public event MessageHandler SendMessage;

        public ObservableCollection<Sensor> Sensors { get; set; } = new ObservableCollection<Sensor>();

        public ObservableCollection<Target> Targets { get; set; } = new ObservableCollection<Target>();

        #endregion Etc

        public Locator()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #region Calculate point coordinates from sensors data

        /// <summary>
        /// calculate targets points from string array and add sensors
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public void CalculateTargetsFromLocatorsData(string[] input)
        {
            List<Target> result = new List<Target>();

            if (input == null || input.Length == 0) return;
            CreateSensors(input[0]);
            CreateTargets(input.Skip(1));
        }

        /// <summary>
        /// add targets from array of string in format as x, y
        /// </summary>
        /// <param name="input"></param>
        private void CreateTargets(IEnumerable<string> input)
        {
            foreach (string timesStr in input)
            {
                string[] times = timesStr.Split(",");

                if (times.Length != maxSensorsCount)
                {
                    Send("file with sensors data is uncorrect");
                    return;
                }

                if (Sensors.Count < maxSensorsCount)
                {
                    Send("sensors countt must be " + maxSensorsCount);
                    return;
                }

                double t1;
                bool isOk1 = Double.TryParse(times[0], out t1);
                double t2;
                bool isOk2 = Double.TryParse(times[1], out t2);
                double t3;
                bool isOk3 = Double.TryParse(times[2], out t3);

                if (!isOk1 || !isOk2 || !isOk3)
                {
                    Send("uncorrect sensor data string");
                    return;
                }

                List<SensorData> sd = new List<SensorData>();
                sd.Add(new SensorData(Sensors[0].Point, t1));
                sd.Add(new SensorData(Sensors[1].Point, t2));
                sd.Add(new SensorData(Sensors[2].Point, t3));
                Target target = new Target(sd);
                SetPointFromSensorsData(target);
                Targets.Add(target);
            }
        }

        /// <summary>
        /// add sensors from string in format as x1,y1,x2,y2,x3,y3 
        /// </summary>
        /// <param name="sensorsCoordinates"></param>
        private void CreateSensors(string sensorsCoordinates)
        {
            string[] coords = sensorsCoordinates.Split(',');
            if (coords.Length != 6)
            {
                Send("unknow sensors coordinate string format");
                return;
            }
            Sensors.Clear();
            int id = 0;
            for (int i = 0; i < coords.Length; i += 2)
            {
                double x = Double.Parse(coords[i]);
                double y = Double.Parse(coords[i + 1]);
                Sensor sens = new Sensor() { Id = ++id, Point = new Point(x, y) };
                Sensors.Add(sens);
            }
        }

        /// <summary>
        /// set point for target based on sensors info
        /// </summary>
        /// <param name="target"></param>
        private void SetPointFromSensorsData(Target target)
        {
            //start in order from the largest radius, this is important
            var sensorsDataList = target.GetSensorsData().OrderByDescending(s => s.Delay).ToList();
            bool isComplete = false;
            foreach (SensorData sd in sensorsDataList)
            {
                if (isComplete) break;
                List<SensorData> othersSd = target.GetSensorsData().Where(s => s.Point.X != sd.Point.X || s.Point.Y != sd.Point.Y).ToList();

                for (double i = 0; i <= precistion && !isComplete; i += 0.01)
                {
                    double negative = 1 - i;
                    double positive = 1 + i;
                    for (double j = 0; j <= precistion && !isComplete; j += 0.01)
                    {

                        double negative2 = 1 - j;
                        double positive2 = 1 + j;

                        List<List<Point>> allPointsPairs = new List<List<Point>>();

                        //do not repeat calculations
                        allPointsPairs.Add(GetIntersectionPoints(negative, negative2, othersSd));
                        if (i != 0 && j != 0)
                        {
                            allPointsPairs.Add(GetIntersectionPoints(negative, positive2, othersSd));

                            if (j != 0)
                            {
                                allPointsPairs.Add(GetIntersectionPoints(positive, negative2, othersSd));
                                allPointsPairs.Add(GetIntersectionPoints(positive, positive2, othersSd));
                            }
                        }

                        foreach (var points in allPointsPairs)
                        {
                            if (points == null) continue;
                            Point? thirdPoint = GetGoodPoint(sd, points);
                            if (thirdPoint != null)
                            {
                                target.Point = (Point)thirdPoint;
                                isComplete = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// check the distance from the third point to 2 others and return the point that matches the accuracy
        /// </summary>
        /// <param name="sdMin"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        private Point? GetGoodPoint(SensorData sensorData, List<Point> points)
        {
            double radius = sensorData.Delay * lightSpeed;
            double maxRadius = radius * 1.05;
            double minRadius = radius * 0.95;
            Point centerPoint = sensorData.Point;
            foreach (Point point in points)
            {
                double realRadius = Math.Sqrt(Math.Pow((point.X - centerPoint.X), 2) + Math.Pow((point.Y - centerPoint.Y), 2));
                if (realRadius <= maxRadius && realRadius >= minRadius) return point;
            }
            return null;
        }

        /// <summary>
        /// get intersection points of 2 circles, koefficients needed for 
        /// enumeration of values with acceptable precision
        /// </summary>
        /// <param name="koef1"></param>
        /// <param name="koef2"></param>
        /// <param name="bigSd"></param>
        /// <returns></returns>
        public List<Point> GetIntersectionPoints(double koef1, double koef2, List<SensorData> sensorDataList)
        {
            double x0 = sensorDataList[0].Point.X;
            double y0 = sensorDataList[0].Point.Y;
            double r0 = sensorDataList[0].Delay * lightSpeed * koef1;
            double x1 = sensorDataList[1].Point.X;
            double y1 = sensorDataList[1].Point.Y;
            double r1 = sensorDataList[1].Delay * lightSpeed * koef2;

            double a, dx, dy, d, h, rx, ry;
            double point2_x, point2_y;

            /* dx and dy are the vertical and horizontal distances between
            * the circle centers.
            */
            dx = x1 - x0;
            dy = y1 - y0;

            /* Determine the straight-line distance between the centers. */
            d = Math.Sqrt((dy * dy) + (dx * dx));

            /* Check for solvability. */
            if (d > (r0 + r1))
            {
                /* no solution. circles do not intersect. */
                return new List<Point>();
            }
            if (d < Math.Abs(r0 - r1))
            {
                /* no solution. one circle is contained in the other */
                return new List<Point>();
            }

            /* 'point 2' is the point where the line through the circle
            * intersection points crosses the line between the circle
            * centers.
            */

            /* Determine the distance from point 0 to point 2. */
            a = ((r0 * r0) - (r1 * r1) + (d * d)) / (2.0 * d);

            /* Determine the coordinates of point 2. */
            point2_x = x0 + (dx * a / d);
            point2_y = y0 + (dy * a / d);

            /* Determine the distance from point 2 to either of the
            * intersection points.
            */
            h = Math.Sqrt((r0 * r0) - (a * a));

            /* Now determine the offsets of the intersection points from
            * point 2.
            */
            rx = -dy * (h / d);
            ry = dx * (h / d);

            /* Determine the absolute intersection points. */
            double intersectionPoint1_x = point2_x + rx;
            double intersectionPoint2_x = point2_x - rx;
            double intersectionPoint1_y = point2_y + ry;
            double intersectionPoint2_y = point2_y - ry;
            Point p1 = new Point(intersectionPoint1_x, intersectionPoint1_y);
            Point p2 = new Point(intersectionPoint2_x, intersectionPoint2_y);
            List<Point> points = new List<Point>();
            if (p1 == p2)
                points.Add(p1);
            else
            {
                points.Add(p1);
                points.Add(p2);
            }

            return points;
        }

        /// <summary>
        /// get distace between 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public double GetDistance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            double d = Math.Sqrt((dy * dy) + (dx * dx));
            return d;
        }

        /// <summary>
        /// get delay of signal for distance
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public double GetDelayByDistance(double distance) => distance / lightSpeed;

        #endregion Calculate point coordinates from sensors data

        #region Actions

        /// <summary>
        /// add target to instance
        /// </summary>
        /// <param name="p"></param>
        public void AddTarget(Point p) => Targets.Add(new Target(new Point(p.X, p.Y)));

        /// <summary>
        /// show object real-time moving
        /// </summary>
        async public void Play()
        {
            List<Target> tempTargets = Targets.ToList();
            Targets.Clear();
            foreach (Target targ in tempTargets)
            {
                await Task.Delay(1000);
                Targets.Add(targ);
            }
        }

        /// <summary>
        /// input saved target points in format as x,y on each string
        /// </summary>
        /// <param name="input"></param>
        public void InputTargetPoints(string[] input)
        {
            foreach (string coords in input)
            {
                string[] xy = coords.Split(",");
                if (xy.Length != 2)
                {
                    Send("file with data has uncorrect format");
                    return;
                }

                double x;
                bool isOk = Double.TryParse(xy[0], out x);
                if (!isOk)
                {
                    Send("file with data has uncorrect format");
                    return;
                }

                double y;
                isOk = Double.TryParse(xy[1], out y);
                if (!isOk)
                {
                    Send("file with data has uncorrect format");
                    return;
                }

                Target target = new Target(new Point(x, y));
                Targets.Add(target);
            }
        }

        /// <summary>
        /// remove targets and sensors
        /// </summary>
        public void ClearAll()
        {
            Targets.Clear();
            Sensors.Clear();
        }

        /// <summary>
        /// add sensor by point
        /// </summary>
        /// <param name="p"></param>
        public void AddSensor(Point p)
        {
            if (Sensors.Count >= maxSensorsCount)
            {
                Send("count of sensors must be " + maxSensorsCount);
                return;
            }

            int id = 0;
            for (int i = 1; i <= maxSensorsCount; i++)
            {
                var sensor = Sensors.Where(s => s.Id == i).FirstOrDefault();
                if (sensor == null)
                {
                    id = i;
                    break;
                }
            }
            Sensor sens = new Sensor(p);
            Sensors.Add(sens);
        }

        /// <summary>
        /// generating an event for sending a message if someone subscribes to it
        /// </summary>
        /// <param name="message"></param>
        void Send(string message)
        {
            if (SendMessage != null) SendMessage(message);
        }

        /// <summary>
        /// get light speed from config
        /// </summary>
        /// <returns></returns>
        public double GetLightSpeed() => lightSpeed;

        #endregion Actions
    }
}
