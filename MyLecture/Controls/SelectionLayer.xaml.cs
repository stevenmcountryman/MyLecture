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
            get
            {
                return this.selectionLasso.Points.ToList<Point>();
            }
        }
        private Polygon selectionLasso;
        
        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler SelectionMade;

        public SelectionLayer()
        {
            this.InitializeComponent();
        }

        private void SelectionCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.SelectionCanvas.Children.Clear();
            this.pointer = e.Pointer.PointerId;
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
        }

        private void SelectionCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (this.pointer == e.Pointer.PointerId)
            {                
                var currentPoint = e.GetCurrentPoint(this.SelectionCanvas).Position;
                this.selectionLasso.Points.Add(currentPoint);
            }
        }

        private void SelectionCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (this.pointer != 0)
            {
                this.pointer = 0;

                this.SelectionCanvas.PointerPressed -= SelectionCanvas_PointerPressed;
                this.SelectionCanvas.PointerMoved -= SelectionCanvas_PointerMoved;
                this.SelectionCanvas.PointerReleased -= SelectionCanvas_PointerReleased;

                this.SelectionMade(this, new EventArgs());                
            }
        }

        public void showCopyMovePopup(CopyPasteMovePopup cpmPopup)
        {
            this.SelectionCanvas.Children.Add(cpmPopup);
        }
    }
}
