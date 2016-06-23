using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MyLecture.Controls
{
    public sealed partial class SelectionLayer : UserControl
    {
        private uint pointer;
        public List<Point> selectionPoints
        {
            get;
            private set;
        }
        
        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler SelectionMade;

        public SelectionLayer()
        {
            this.InitializeComponent();
        }

        private void SelectionCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.pointer = e.Pointer.PointerId;
            var currentPoint = e.GetCurrentPoint(this.SelectionCanvas).Position;
            Rectangle rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Colors.Gray);
            rect.Fill = new SolidColorBrush(Colors.Transparent);
            rect.StrokeThickness = 3;
            rect.StrokeDashArray = new DoubleCollection() { 2 };
            Canvas.SetLeft(rect, currentPoint.X);
            Canvas.SetTop(rect, currentPoint.Y);
            rect.Width = 0;
            rect.Height = 0;
            this.SelectionCanvas.Children.Add(rect);
        }

        private void SelectionCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this.pointer == e.Pointer.PointerId)
            {
                var currentPoint = e.GetCurrentPoint(this.SelectionCanvas).Position;
                var rect = this.SelectionCanvas.Children[0] as Rectangle;
                rect.Width = currentPoint.X - Canvas.GetLeft(rect);
                rect.Height = currentPoint.Y - Canvas.GetTop(rect);
            }
        }

        private void SelectionCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (this.pointer != 0)
            {
                this.pointer = 0;
                var rect = this.SelectionCanvas.Children[0] as Rectangle;
                this.selectionPoints = new List<Point>()
                {
                    new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect)),
                    new Point(Canvas.GetLeft(rect), Canvas.GetTop(rect) + rect.Height),
                    new Point(Canvas.GetLeft(rect) + rect.Width, Canvas.GetTop(rect) + rect.Height),
                    new Point(Canvas.GetLeft(rect) + rect.Width, Canvas.GetTop(rect))
                };

                this.SelectionMade(this, new EventArgs());
            }
        }
    }
}
