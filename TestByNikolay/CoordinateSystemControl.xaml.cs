using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LocatorLib;

namespace WpfUserControlTest
{
    /// <summary>
    /// Interaction logic for CoordinateSystemControl.xaml
    /// </summary>
    public partial class CoordinateSystemControl : UserControl
    {
        #region Fields and properties

        #region Canvas config

        int gridStep = 50;
        int objectPointSize = 12;
        double scaleKoef = 0.1;
        double zoomKoef = 1;
        double maxKoef = 5;
        double minKoef = 0.3;

        #endregion Canvas config

        #region Canvas drawing variables

        Point lastPoint;
        private bool _isRectDragInProg;
        Double xMax;
        Double xMin;
        Double yMax;
        Double yMin;

        #endregion Canvas drawing variables

        #region Binding sources

        #region LastContextMenuClickPoint

        public static readonly DependencyProperty LastContextMenuClickPointProperty =
            DependencyProperty.Register("LastContextMenuClickPoint", typeof(Point?), typeof(CoordinateSystemControl),
                new PropertyMetadata(null, LastContextMenuClickPointPropertyChanged));

        public Point? LastContextMenuClickPoint
        {
            get { return (Point?)GetValue(LastContextMenuClickPointProperty); }
            set { SetValue(LastContextMenuClickPointProperty, value); }
        }

        private void LastContextMenuClickPointPropertyChanged(Point? LastContextMenuClickPoint)
        {
            //..
        }

        private static void LastContextMenuClickPointPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateSystemControl)d).LastContextMenuClickPointPropertyChanged((Point?)e.NewValue);
        }

        #endregion LastContextMenuClickPoint

        #region LastPoint

        public static readonly DependencyProperty LastPointProperty =
            DependencyProperty.Register("LastPoint", typeof(Point?), typeof(CoordinateSystemControl),
                new PropertyMetadata(null, LastPointPropertyChanged));

        public Point? LastPoint
        {
            get { return (Point?)GetValue(LastPointProperty); }
            set { SetValue(LastPointProperty, value); }
        }

        private void LastPointPropertyChanged(Point? lastPoint)
        {
            //..
        }

        private static void LastPointPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateSystemControl)d).LastPointPropertyChanged((Point?)e.NewValue);
        }

        #endregion LastPoint

        #region IsStartRecord

        public static readonly DependencyProperty IsStartRecordProperty =
            DependencyProperty.Register("IsStartRecord", typeof(bool), typeof(CoordinateSystemControl),
                new PropertyMetadata(false, IsStartRecordPropertyChanged));

        public bool IsStartRecord
        {
            get { return (bool)GetValue(IsStartRecordProperty); }
            set { SetValue(IsStartRecordProperty, value); }
        }

        private void IsStartRecordPropertyChanged(bool isStartRecord)
        {
            //..
        }

        private static void IsStartRecordPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateSystemControl)d).IsStartRecordPropertyChanged((bool)e.NewValue);
        }

        #endregion IsStartRecord

        #region IsStopRecord

        public static readonly DependencyProperty IsStopRecordProperty =
            DependencyProperty.Register("IsStopRecord", typeof(bool), typeof(CoordinateSystemControl),
                new PropertyMetadata(false, IsStopRecordPropertyChanged));

        public bool IsStopRecord
        {
            get { return (bool)GetValue(IsStopRecordProperty); }
            set { SetValue(IsStopRecordProperty, value); }
        }

        private void IsStopRecordPropertyChanged(bool isStopRecord)
        {
            //..
        }

        private static void IsStopRecordPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateSystemControl)d).IsStopRecordPropertyChanged((bool)e.NewValue);
        }

        #endregion IsStopRecord

        #region IsCreatePath

        public static readonly DependencyProperty IsCreatePathProperty =
            DependencyProperty.Register("IsCreatePath", typeof(bool), typeof(CoordinateSystemControl),
                new PropertyMetadata(false, IsCreatePathPropertyChanged));

        public bool IsCreatePath
        {
            get { return (bool)GetValue(IsCreatePathProperty); }
            set { SetValue(IsCreatePathProperty, value); }
        }

        private void IsCreatePathPropertyChanged(bool isCreatePath)
        {
            IsCreatePath = isCreatePath;
            DrawAll();
        }

        private static void IsCreatePathPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateSystemControl)d).IsCreatePathPropertyChanged((bool)e.NewValue);
        }

        #endregion IsCreatePath

        #region IsCurve

        public static readonly DependencyProperty IsCurveProperty =
            DependencyProperty.Register("IsCurve", typeof(bool), typeof(CoordinateSystemControl),
                new PropertyMetadata(false, IsCurvePropertyChanged));

        public bool IsCurve
        {
            get { return (bool)GetValue(IsCurveProperty); }
            set { SetValue(IsCurveProperty, value); }
        }

        private void IsCurvePropertyChanged(bool isCurve)
        {
            IsCurve = isCurve;
            DrawAll();
        }

        private static void IsCurvePropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateSystemControl)d).IsCurvePropertyChanged((bool)e.NewValue);
        }

        #endregion IsCurve

        #region Targets

        public static readonly DependencyProperty TargetsProperty =
            DependencyProperty.Register("Targets", typeof(ObservableCollection<Target>), typeof(CoordinateSystemControl),
                new PropertyMetadata(null, TargetsPropertyChanged));

        public ObservableCollection<Target> Targets
        {
            get { return (ObservableCollection<Target>)GetValue(TargetsProperty); }
            set { SetValue(TargetsProperty, value); }
        }

        //Listen for changes to the dependency property (note this is a static method)
        public static void TargetsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CoordinateSystemControl me = obj as CoordinateSystemControl;
            if (me != null)
            {
                // Call a non-static method on the object whose property has changed
                me.OnTargetsChanged((ObservableCollection<Target>)e.OldValue, (ObservableCollection<Target>)e.NewValue);
            }
        }

        // Listen for changes to the property (non-static) to register CollectionChanged handlers
        protected virtual void OnTargetsChanged(ObservableCollection<Target> oldValue, ObservableCollection<Target> newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= TargetsChangedHandler;
            }
            if (newValue != null)
            {
                newValue.CollectionChanged += TargetsChangedHandler;
            }
        }

        private void TargetsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            DrawAll();
        }

        #endregion Targets

        #region Sensors

        public static readonly DependencyProperty SensorsProperty =
            DependencyProperty.Register("Sensors", typeof(ObservableCollection<Sensor>), typeof(CoordinateSystemControl),
                new PropertyMetadata(null, SensorsPropertyChanged));

        public ObservableCollection<Sensor> Sensors
        {
            get { return (ObservableCollection<Sensor>)GetValue(SensorsProperty); }
            set { SetValue(SensorsProperty, value); }
        }

        //Listen for changes to the dependency property (note this is a static method)
        public static void SensorsPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CoordinateSystemControl me = obj as CoordinateSystemControl;
            if (me != null)
            {
                // Call a non-static method on the object whose property has changed
                me.OnSensorsChanged((ObservableCollection<Sensor>)e.OldValue, (ObservableCollection<Sensor>)e.NewValue);
            }
        }

        // Listen for changes to the property (non-static) to register CollectionChanged handlers
        protected virtual void OnSensorsChanged(ObservableCollection<Sensor> oldValue, ObservableCollection<Sensor> newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= SensorsChangedHandler;
            }
            if (newValue != null)
            {
                newValue.CollectionChanged += SensorsChangedHandler;
            }
        }

        private void SensorsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            DrawAll();
        }

        #endregion Sensors

        #region LightSpeed

        public static readonly DependencyProperty LightSpeedProperty =
            DependencyProperty.Register("LightSpeed", typeof(double), typeof(CoordinateSystemControl),
                new PropertyMetadata(LightSpeedPropertyChanged));

        public double LightSpeed
        {
            get { return (double)GetValue(LightSpeedProperty); }
            set { SetValue(LightSpeedProperty, value); }
        }

        private void LightSpeedPropertyChanged(double lightSpeed)
        {
            //..
        }

        private static void LightSpeedPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateSystemControl)d).LightSpeedPropertyChanged((double)e.NewValue);
        }

        #endregion LightSpeed

        #endregion Binding sources

        #endregion Fields and properties

        #region External events

        public delegate void SetLastPoint_(Point lastPoint);
        public event SetLastPoint_ SetLastPoint;

        #endregion External events

        #region Constructor

        public CoordinateSystemControl()
        {
            InitializeComponent();
        }

        #endregion Constructor

        #region Some events

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            canvas.MouseWheel += Canvas_MouseWheel;
            canvas.MouseLeftButtonDown += Canvas_MouseDown;
            canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
            canvas.MouseMove += Canvas_MouseMove;
            canvas.MouseLeave += Canvas_MouseLeave;
            SizeChanged += Canvas_SizeChanged;
            canvas.ContextMenuOpening += Canvas_ContextMenuOpening;
            DrawAll();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawAll();

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && zoomKoef < maxKoef)
            {
                zoomKoef += .1;
                DrawAll();
            }
            else if (e.Delta < 0 && zoomKoef > minKoef)
            {
                zoomKoef -= .1;
                DrawAll();
            }
        }

        #endregion Some events

        #region Canvas context menu

        private void Canvas_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Point mousePos = Mouse.GetPosition(canvas);
            LastContextMenuClickPoint = new Point(mousePos.X * scaleKoef, mousePos.Y * scaleKoef);
        }

        #endregion Canvas context menu

        #region Real-time targets drawing

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsCreatePath)
            {
                IsCreatePath = false;
                IsStopRecord = true;
            }
        }

        async private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(canvas);
            lastPoint = new Point(ConvertFromCanvasToModel(p.X), ConvertFromCanvasToModel(p.Y));
            if (SetLastPoint != null)
                SetLastPoint(lastPoint);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsCreatePath)
            {
                IsCreatePath = false;
                IsStopRecord = true;
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsCreatePath)
            {
                IsStopRecord = false;
                IsStartRecord = true;
                Point p = e.GetPosition(canvas);
                lastPoint = new Point(ConvertFromCanvasToModel(p.X), ConvertFromCanvasToModel(p.Y));
            }
        }

        #endregion Real-time targets drawing

        #region Drag and drop

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isRectDragInProg = true;
            ((Ellipse)sender).CaptureMouse();
        }

        private void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isRectDragInProg) return;
            var mousePos = e.GetPosition(canvas);
            double left = mousePos.X - (((Ellipse)sender).ActualWidth / 2);
            double top = mousePos.Y - (((Ellipse)sender).ActualHeight / 2);
            Canvas.SetLeft((Ellipse)sender, left);
            Canvas.SetTop((Ellipse)sender, top);
        }

        private void Button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isRectDragInProg = false;
            ((Ellipse)sender).ReleaseMouseCapture();
            var mousePos = e.GetPosition(canvas);
            SomePoint sp = (SomePoint)((Ellipse)sender).Tag;
            sp.Point = new Point(ConvertFromCanvasToModel(mousePos.X), ConvertFromCanvasToModel(mousePos.Y));
            if (sp.GetType() == typeof(Target)) 
                ((Target)sp).SkipSensorsData();
            if (sp.GetType() == typeof(Sensor))
                Targets.ToList().ForEach(t=>t.SkipSensorsData());
            DrawAll();
        }

        #endregion

        #region Draw all

        private void DrawAll()
        {
            xMax = canvas.ActualWidth / 2 * zoomKoef;
            xMin = -xMax;
            yMax = canvas.ActualHeight / 2 * zoomKoef;
            yMin = -yMax;

            canvas.Children.Clear();
            canvas.SetCoordinateSystem(xMin, xMax, yMin, yMax);
            DrawGrid();
            DrawAxises();
            DrawFakeRectangel();
            DrawSensors();
            DrawTargetsLine();
            DrawTargets();
        }

        private void DrawGrid()
        {
            double beginX = xMin + xMax % gridStep;
            for (double i = beginX; i < xMax; i += gridStep)
            {
                canvas.Children.Add(new Line() { X1 = i, Y1 = yMax, X2 = i, Y2 = yMin, Stroke = Brushes.LightGray, StrokeThickness = 1 });
                TextToCanvas(i - 10, -20, (i * scaleKoef).ToString(), new SolidColorBrush(Colors.Blue));
            }

            double beginY = yMin + yMax % gridStep;
            for (double i = beginY; i < yMax; i += gridStep)
            {
                canvas.Children.Add(new Line() { X1 = xMin, Y1 = i, X2 = xMax, Y2 = i, Stroke = Brushes.LightGray, StrokeThickness = 1 });
                if (i != 0) TextToCanvas(-40, i + 10, (i * scaleKoef).ToString(), new SolidColorBrush(Colors.Blue));
            }
        }

        private void DrawAxises()
        {
            //draw axis X
            int axisDivider = 60;//for draw arrow
            canvas.Children.Add(new Line() { X1 = xMin, Y1 = 0, X2 = xMax, Y2 = 0, Stroke = Brushes.Black, StrokeThickness = 2 });
            canvas.Children.Add(new Line() { X1 = xMax, Y1 = 0, X2 = xMax - xMax / axisDivider, Y2 = xMax / axisDivider, Stroke = Brushes.Black, StrokeThickness = 2 });
            canvas.Children.Add(new Line() { X1 = xMax, Y1 = 0, X2 = xMax - xMax / axisDivider, Y2 = -xMax / axisDivider, Stroke = Brushes.Black, StrokeThickness = 2 });

            //draw axis Y
            canvas.Children.Add(new Line() { X1 = 0, Y1 = yMin, X2 = 0, Y2 = yMax, Stroke = Brushes.Black, StrokeThickness = 2 });
            canvas.Children.Add(new Line() { X1 = 0, Y1 = yMax, X2 = -xMax / axisDivider, Y2 = yMax - xMax / axisDivider, Stroke = Brushes.Black, StrokeThickness = 2 });
            canvas.Children.Add(new Line() { X1 = 0, Y1 = yMax, X2 = xMax / axisDivider, Y2 = yMax - xMax / axisDivider, Stroke = Brushes.Black, StrokeThickness = 2 });
        }

        private void DrawFakeRectangel()
        {
            //Add rectangle for context menu working on negative coordinates
            Rectangle rec = new Rectangle()
            {
                Width = xMax - xMin,
                Height = yMax - yMin,
                Fill = Brushes.Transparent
            };
            canvas.Children.Add(rec);
            Canvas.SetTop(rec, yMin);
            Canvas.SetLeft(rec, xMin);
        }

        private void DrawSensors() => Sensors?.ToList().ForEach(s => DrawPoint(s));

        private void DrawTargets() => Targets?.ToList().ForEach(t => DrawPoint(t));

        private void DrawPoint(SomePoint point)
        {
            bool isTarget = point.GetType() == typeof(Sensor) ? false : true; 
            var brash = isTarget ? Brushes.Green : Brushes.OrangeRed;
            Ellipse circle = new Ellipse()
            {
                Width = objectPointSize,
                Height = objectPointSize,
                Stroke = brash,
                StrokeThickness = 1,
                Fill = brash,
                Tag = point,
                ToolTip = point.ToString()
            };

            circle.MouseLeftButtonDown += Button_MouseLeftButtonDown;
            circle.MouseLeftButtonUp += Button_MouseLeftButtonUp;
            circle.MouseMove += Button_MouseMove;
            if(isTarget)
            {
                if (((Target)point).GetSensorsData()!=null)
                {
                    ContextMenu menu = new ContextMenu();
                    MenuItem mi = new MenuItem();
                    mi.Header = "show radiuses";
                    mi.Click += Mi_Click;
                    mi.Tag = point;
                    menu.Items.Add(mi);
                    circle.ContextMenu = menu;
                }
            }
            canvas.Children.Add(circle);
            Canvas.SetTop(circle, point.Point.Y / scaleKoef - objectPointSize / 2);
            Canvas.SetLeft(circle, point.Point.X / scaleKoef - objectPointSize / 2);
        }

        private void Mi_Click(object sender, RoutedEventArgs e)
        {
            DrawAll();
            Target target = (Target)((FrameworkElement)sender).Tag;
            target.GetSensorsData().ForEach(t=> DrawRadiuses(t));
        }

        private void DrawTargetsLine()
        {
            if (IsCurve)
            {
                DrawCurve();
            }
            else
            {
                List<Point> points = GetPoints();
                Polyline line = new Polyline();
                PointCollection collection = new PointCollection();
                points.ForEach(p => collection.Add(p));
                line.Points = collection;
                line.Stroke = new SolidColorBrush(Colors.Black);
                line.StrokeThickness = 1;
                canvas.Children.Add(line);
            }
        }

        private void TextToCanvas(double x, double y, string text, SolidColorBrush color, bool constSize = false)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = color;
            ScaleTransform st = new ScaleTransform();
            if (constSize)
            {
                st.ScaleY = -1 * zoomKoef;
                st.ScaleX = zoomKoef;
            }
            else
            {
                st.ScaleY = -1;
            }
            textBlock.RenderTransform = st;
            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            canvas.Children.Add(textBlock);
        }

        void DrawRadiuses(SensorData sd)
        {
            double radius = LightSpeed * sd.Delay;
            double maxRadius = radius * 1.05;
            double minRadius = radius * 0.95;
            double opacity = .5;

            SolidColorBrush brRed = new SolidColorBrush(Colors.Red);
            brRed.Opacity = opacity;
            Ellipse circleMax = new Ellipse()
            {
                Width = maxRadius / scaleKoef * 2,
                Height = maxRadius / scaleKoef * 2,
                Stroke = brRed,
                StrokeThickness = 1
            };

            SolidColorBrush brOrange = new SolidColorBrush(Colors.Orange);
            brOrange.Opacity = opacity;
            Ellipse circleMin = new Ellipse()
            {
                Width = minRadius / scaleKoef * 2,
                Height = minRadius / scaleKoef * 2,
                Stroke = brOrange,
                StrokeThickness = 1
            };

            SolidColorBrush brBlue = new SolidColorBrush(Colors.Blue);
            brBlue.Opacity = opacity;
            Ellipse circle = new Ellipse()
            {
                Width = radius / scaleKoef * 2,
                Height = radius / scaleKoef * 2,
                Stroke = brBlue,
                StrokeThickness = 1
            };

            canvas.Children.Add(circleMax);
            circleMax.SetValue(Canvas.LeftProperty, sd.Point.X / scaleKoef - maxRadius / scaleKoef);
            circleMax.SetValue(Canvas.TopProperty, sd.Point.Y / scaleKoef - maxRadius / scaleKoef);

            canvas.Children.Add(circleMin);
            circleMin.SetValue(Canvas.LeftProperty, sd.Point.X / scaleKoef - minRadius / scaleKoef);
            circleMin.SetValue(Canvas.TopProperty, sd.Point.Y / scaleKoef - minRadius / scaleKoef);

            canvas.Children.Add(circle);
            circle.SetValue(Canvas.LeftProperty, sd.Point.X / scaleKoef - radius / scaleKoef);
            circle.SetValue(Canvas.TopProperty, sd.Point.Y / scaleKoef - radius / scaleKoef);
        }

        #endregion Draw all

        #region DrawCurve

        // Make an array containing Bezier curve points and control points.
        private Point[] MakeCurvePoints(Point[] points, double tension)
        {
            if (points.Length < 2) return null;
            double controlScale = tension / 0.5 * 0.175;

            // Make a list containing the points and
            // appropriate control points.
            List<Point> resultPoints = new List<Point>();
            resultPoints.Add(points[0]);

            for (int i = 0; i < points.Length - 1; i++)
            {
                // Get the point and its neighbors.
                Point pt_before = points[Math.Max(i - 1, 0)];
                Point pt = points[i];
                Point pt_after = points[i + 1];
                Point pt_after2 = points[Math.Min(i + 2, points.Length - 1)];

                double dx1 = pt_after.X - pt_before.X;
                double dy1 = pt_after.Y - pt_before.Y;

                Point p1 = points[i];
                Point p4 = pt_after;

                double dx = pt_after.X - pt_before.X;
                double dy = pt_after.Y - pt_before.Y;
                Point p2 = new Point(
                    pt.X + controlScale * dx,
                    pt.Y + controlScale * dy);

                dx = pt_after2.X - pt.X;
                dy = pt_after2.Y - pt.Y;
                Point p3 = new Point(
                    pt_after.X - controlScale * dx,
                    pt_after.Y - controlScale * dy);

                // Save points p2, p3, and p4.
                resultPoints.Add(p2);
                resultPoints.Add(p3);
                resultPoints.Add(p4);
            }

            // Return the points.
            return resultPoints.ToArray();
        }

        // Make a Path holding a series of Bezier curves.
        // The points parameter includes the points to visit
        // and the control points.
        private Path MakeBezierPath(Point[] points)
        {
            // Create a Path to hold the geometry.
            System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();

            // Add a PathGeometry.
            PathGeometry pathGeometry = new PathGeometry();
            path.Data = pathGeometry;

            // Create a PathFigure.
            PathFigure pathFigure = new PathFigure();
            pathGeometry.Figures.Add(pathFigure);

            // Start at the first point.
            pathFigure.StartPoint = points[0];

            // Create a PathSegmentCollection.
            PathSegmentCollection pathSegmentCollection =
                new PathSegmentCollection();
            pathFigure.Segments = pathSegmentCollection;

            // Add the rest of the points to a PointCollection.
            PointCollection pointCollection =
                new PointCollection(points.Length - 1);
            for (int i = 1; i < points.Length; i++)
                pointCollection.Add(points[i]);

            // Make a PolyBezierSegment from the points.
            PolyBezierSegment bezierSegment = new PolyBezierSegment();
            bezierSegment.Points = pointCollection;

            // Add the PolyBezierSegment to othe segment collection.
            pathSegmentCollection.Add(bezierSegment);

            return path;
        }

        // Make a Bezier curve connecting these points.
        private Path MakeCurve(Point[] points, double tension)
        {
            if (points.Length < 2) return null;
            Point[] result_points = MakeCurvePoints(points, tension);

            // Use the points to create the path.
            return MakeBezierPath(result_points.ToArray());
        }

        private List<Point> GetPoints()
        {
            List<Point> res = new List<Point>();
            Targets?.ToList().ForEach(p => res.Add(new Point(ConvetFromModelToCanvas(p.Point.X), ConvetFromModelToCanvas(p.Point.Y))));
            return res;
        }

        private void DrawCurve()
        {
            Point[] points = GetPoints().ToArray();
            DrawCurve(1, points);
        }

        // Make the curve.
        private void DrawCurve(double tension, Point[] points)
        {
            System.Windows.Shapes.Path path1 = MakeCurve(points, tension);
            if (path1 == null) return;
            path1.Stroke = Brushes.Green;
            path1.StrokeThickness = 2;
            canvas.Children.Add(path1);
        }

        #endregion DrawCurve

        #region Some methods

        private double ConvetFromModelToCanvas(double coordinateValue) => coordinateValue / scaleKoef;

        private double ConvertFromCanvasToModel(double coordinateValue) => coordinateValue * scaleKoef;

        #endregion Some methods

    }
}
