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
using LocatorLib;

namespace WpfUserControlTest
{
    public class MainModel : BaseModel
    {
        #region Config


        #endregion Config

        #region MVVM

        #region Fields

        bool isCreatePath;
        bool isStopRecord;
        bool isStartRecord;
        Point lastContextMenuClickPoint;

        #endregion Fields

        #region Properties

        public ObservableCollection<Sensor> Sensors { get; set; } = new ObservableCollection<Sensor>();

        public ObservableCollection<Target> Targets { get; set; } = new ObservableCollection<Target>();

        public Point LastPoint { get; set; }

        public bool IsCreatePath
        {
            get { return isCreatePath; }
            set
            {
                isCreatePath = value;
                OnPropertyChanged();
            }
        }

        public bool IsStopRecord
        {
            get { return isStopRecord; }
            set
            {
                isStopRecord = value;
                if (value) StopTimer();
                OnPropertyChanged();
            }
        }

        public bool IsStartRecord
        {
            get { return isStartRecord; }
            set
            {
                isStartRecord = value;
                if (value) StartTimer();
                OnPropertyChanged();
            }
        }

        public double LightSpeed
        {
            get { return loc.GetLightSpeed(); }
        }

        public Point LastContextMenuClickPoint
        {
            get { return lastContextMenuClickPoint; }
            set
            {
                lastContextMenuClickPoint = value;
                OnPropertyChanged();
            }
        }

        #endregion Properties

        #region Commands

        public void AddSensorCommand() => loc.AddSensor(new Point(lastContextMenuClickPoint.X, lastContextMenuClickPoint.Y));

        public void AddTargetCommand() => loc.AddTarget(new Point(lastContextMenuClickPoint.X, lastContextMenuClickPoint.Y));

        public void InputLocatorsDataCommand() => InputLocatorsData();

        public void InputPointsCommand() => InputTargetPoints();

        public void SavePointsCommand() => SavePoints();

        public void SaveLocatorsDataCommand() => SaveLocatorsData();

        public void ClearAllCommand() => loc.ClearAll();

        public void ClearTargetsCommand() => Targets.Clear();

        public void ClearSensorsCommand() => Sensors.Clear();

        public void RecordCommand() => IsCreatePath = true;

        public void PlayCommand() => loc.Play();

        #endregion Commands

        #endregion MVVM

        #region Etc

        Locator loc;
        public Views.MainWindow view;
        DispatcherTimer timer;

        public delegate void MessageHandler(string message);
        public event MessageHandler SendMessage;

        #endregion Etc

        #region Init

        public MainModel()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            loc = new Locator();
            Sensors = loc.Sensors;
            Targets = loc.Targets;
            loc.SendMessage += (message) => SendMessage(message);
        }

        /// <summary>
        /// init UI
        /// </summary>
        internal void Run()
        {
            viewModel = new ViewModels.MainViewModel(this);
            view = new Views.MainWindow((ViewModels.MainViewModel)viewModel);
            view.Show();
            ((ViewModels.MainViewModel)viewModel).SetHandler();
            SendMessage += view.onSendMessage;
        }

        #endregion Init

        #region Commands actions

        /// <summary>
        /// input sensors coordinates and targets points from sensors data from file
        /// </summary>
        public void InputLocatorsData()
        {
            string[] input = Helpers.GetLinesFromFile();
            loc.CalculateTargetsFromLocatorsData(input);
        }

        /// <summary>
        /// input only targets coordinates
        /// </summary>
        public void InputTargetPoints()
        {
            string[] input = Helpers.GetLinesFromFile();
            if (input == null || input.Length == 0) return;
            loc.InputTargetPoints(input);
        }

        /// <summary>
        /// save only targets coordinates
        /// </summary>
        public void SavePoints()
        {
            if (Targets.Count == 0)
            {
                SendMessage("no points");
                return;
            }
            string text = "";
            foreach (Target targ in Targets)
                text += targ.Point.X + "," + targ.Point.Y + "\n";
            Helpers.SaveTextToFile(text);
        }

        /// <summary>
        /// save points as data of locators
        /// </summary>
        private void SaveLocatorsData()
        {
            if (Sensors.Count < 3)
            {
                SendMessage("need 3 sensors");
                return;
            }

            if (Targets.Count < 1)
            {
                SendMessage("need points");
                return;
            }

            List<string> result = new List<string>();

            string sensString = "";
            foreach (Sensor sens in Sensors)
                sensString += string.Format("{0:N8}", sens.Point.X) + "," + string.Format("{0:N8}", sens.Point.Y) + ",";
            sensString = sensString.Remove(sensString.Length - 1);
            result.Add(sensString);

            foreach (Target targ in Targets)
            {
                string targetData = "";
                foreach (Sensor sens in Sensors)
                {
                    double distance = loc.GetDistance(targ.Point, sens.Point);
                    double delay = loc.GetDelayByDistance(distance); //distance / lightSpeed;
                    targetData += string.Format("{0:N8}", delay) + ",";
                }
                targetData = targetData.Remove(targetData.Length - 1);
                result.Add(targetData);
            }
            string text = String.Join("\n", result);
            Helpers.SaveTextToFile(text);
        }

        #endregion Commands actions

        #region Timer for drawing

        internal void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Start();
            loc.AddTarget(LastPoint);
        }

        internal void TimerTick(object sender, EventArgs e) => loc.AddTarget(LastPoint);

        internal void StopTimer() => timer?.Stop();

        #endregion Timer for drawing
    }
}
