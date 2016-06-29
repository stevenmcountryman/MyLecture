using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MyLecture.Controls
{
    public sealed partial class SelectionLayer : UserControl
    {
        private Polygon selectionLasso;

        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler SelectionMade;

        public List<Point> selectionPoints
        {
            get
            {
                return this.selectionLasso.Points.ToList<Point>();
            }
        }

        public SelectionLayer()
        {
            this.InitializeComponent();
        }

        private void SelectionCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.SelectionCanvas.Children.Clear();
            var currentPoint = e.GetCurrentPoint(this.SelectionCanvas).Position;
            this.selectionLasso = new Polygon();
            this.selectionLasso.Stroke = new SolidColorBrush(Colors.Gray);
            this.selectionLasso.Fill = new SolidColorBrush(Colors.Transparent);
            this.selectionLasso.StrokeThickness = 2;
            this.selectionLasso.StrokeDashArray = new DoubleCollection() { 2 };
            PointCollection points = new PointCollection()
            {
                new Point(currentPoint.X, currentPoint.Y)
            };
            this.selectionLasso.Points = points;
            this.SelectionCanvas.Children.Add(this.selectionLasso);
            this.SelectionCanvas.PointerPressed -= SelectionCanvas_PointerPressed;
            this.SelectionCanvas.PointerMoved += SelectionCanvas_PointerMoved;
            this.SelectionCanvas.PointerReleased += SelectionCanvas_PointerReleased;
        }

        private void SelectionCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this.SelectionCanvas).Position;
            Debug.WriteLine(currentPoint.X + "," + currentPoint.Y);
            this.selectionLasso.Points.Add(currentPoint);
            e.Handled = true;
        }

        private void SelectionCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            this.SelectionCanvas.PointerMoved -= SelectionCanvas_PointerMoved;
            this.SelectionCanvas.PointerReleased -= SelectionCanvas_PointerReleased;

            this.SelectionMade(this, new EventArgs());
        }

        public void showCopyMovePopup(CopyPasteMovePopup cpmPopup)
        {
            this.SelectionCanvas.Children.Add(cpmPopup);
        }
    }
}
