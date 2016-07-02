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
        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler SelectionMade;

        public List<Point> selectionPoints
        {
            get
            {
                return this.SelectionLasso.Points.ToList<Point>();
            }
        }
        public void resetSelection()
        {
            this.SelectionLasso.Points = new PointCollection();
            this.SelectionCanvas.PointerPressed += SelectionCanvas_PointerPressed;
            this.SelectionCanvas.PointerMoved -= SelectionCanvas_PointerMoved;
            this.SelectionCanvas.PointerReleased -= SelectionCanvas_PointerReleased;
            this.SelectionCanvas.Children.Clear();
        }

        public SelectionLayer()
        {
            this.InitializeComponent();
        }

        private void SelectionCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this.SelectionCanvas).Position;
            this.SelectionLasso.Points.Add(new Point(currentPoint.X, currentPoint.Y));

            this.SelectionCanvas.PointerPressed -= SelectionCanvas_PointerPressed;
            this.SelectionCanvas.PointerMoved += SelectionCanvas_PointerMoved;
            this.SelectionCanvas.PointerReleased += SelectionCanvas_PointerReleased;
        }

        private void SelectionCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this.SelectionCanvas).Position;
            this.SelectionLasso.Points.Add(currentPoint);
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
