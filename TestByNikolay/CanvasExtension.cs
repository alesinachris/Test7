using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfUserControlTest
{
    public static class CanvasUtils
    {
        public static Canvas SetCoordinateSystem(this Canvas canvas, Double xMin, Double xMax, Double yMin, Double yMax)
        {
            var width = xMax - xMin;
            var height = yMax - yMin;

            var translateX = -xMin;
            var translateY = height + yMin;

            var group = new TransformGroup();

            group.Children.Add(new TranslateTransform(translateX, -translateY));
            group.Children.Add(new ScaleTransform(canvas.ActualWidth / width, canvas.ActualHeight / -height));
            canvas.RenderTransform = group;

            return canvas;
        }
    }
}
