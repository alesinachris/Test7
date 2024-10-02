using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using LocatorLib;

namespace WpfUserControlTest.ViewModels
{
    public class MainViewModel: ViewModelBase
    {
        private MainModel mod;

        public MainViewModel(MainModel mainModel)
        {
            this.mod = mainModel;
            OnSendCommand += mainModel.OnSendCommandHandler;
        }

        public void SetHandler()
        {
            mod.view.CoordinatesControl.SetLastPoint += CoordinatesControl_SetLastPoint;
        }

        private void CoordinatesControl_SetLastPoint(Point lastPoint)
        {
            LastPoint = lastPoint;
        }

        public ObservableCollection<Sensor> Sensors
        {
            get { return mod.Sensors; }
            set
            {
                mod.Sensors = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Target> Targets
        {
            get { return mod.Targets; }
            set
            {
                mod.Targets = value;
                OnPropertyChanged();
            }
        }

        public Point LastPoint
        {
            get { return mod.LastPoint; }
            set
            {
                mod.LastPoint = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LastPointMessage));
            }
        }

        public string LastPointMessage
        {
            get 
            { 
                return " X:" + string.Format("{0:N3}", LastPoint.X) + "\n" + " Y:" + string.Format("{0:N3}", LastPoint.Y); 
            }
        }

        public Point LastContextMenuClickPoint
        {
            get { return mod.LastContextMenuClickPoint; }
            set
            {
                mod.LastContextMenuClickPoint = value;
                OnPropertyChanged();
            }
        }

        public bool IsCreatePath
        {
            get { return mod.IsCreatePath; }
            set
            {
                mod.IsCreatePath = value;
                OnPropertyChanged();
            }
        }

        public bool IsStopRecord
        {
            get { return mod.IsStopRecord; }
            set
            {
                mod.IsStopRecord = value;
                OnPropertyChanged();
            }
        }

        public bool IsStartRecord
        {
            get { return mod.IsStartRecord; }
            set
            {
                mod.IsStartRecord = value;
                OnPropertyChanged();
            }
        }

        public double LightSpeed
        {
            get { return mod.LightSpeed; }
        }
    }
}
