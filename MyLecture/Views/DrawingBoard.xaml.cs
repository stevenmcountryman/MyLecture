using MyLecture.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace MyLecture.Views
{
    /// <summary>
    /// This is the main drawing board class. This will be the main UI for the big screen view
    /// </summary>
    public sealed partial class DrawingBoard : Page
    {
        private Boolean isCanvasWhite = true;
        private SolidColorBrush whiteColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush blackColor = new SolidColorBrush(Colors.Black);
        private bool canTouchInk = false;
        private uint pointer;

        public DrawingBoard()
        {
            this.InitializeComponent();
        }

        #region InkToolbar Custom Actions

        private void ToggleTouchInkingButton_Checked(object sender, RoutedEventArgs e)
        {
            this.toggleTouchInking();
        }

        private void ToggleTouchInkingButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.toggleTouchInking();
        }

        private void ToggleBackgroundColor_Checked(object sender, RoutedEventArgs e)
        {
            this.isCanvasWhite = false;
            this.invertUIColors();
        }

        private void ToggleBackgroundColor_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isCanvasWhite = true;
            this.invertUIColors();
        }

        #region InkToolbar Custom Helpers

        private void toggleTouchInking()
        {
            if (this.ToggleTouchInkingButton.IsChecked == true)
            {
                this.MainCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
            }
            else
            {
                this.MainCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;
            }
        }

        private void invertUIColors()
        {
            if (isCanvasWhite)
            {
                this.MainPanel.Background = this.whiteColor;
                this.SlideViewButton.Foreground = this.blackColor;

                var allStrokes = this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes();
                foreach (var stroke in allStrokes)
                {
                    if (stroke.DrawingAttributes.Color == this.whiteColor.Color)
                    {
                        this.modifyStrokeColor(stroke, this.blackColor.Color);
                    }
                }
            }
            else
            {
                this.MainPanel.Background = this.blackColor;
                this.SlideViewButton.Foreground = this.whiteColor;

                var allStrokes = this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes();
                foreach (var stroke in allStrokes)
                {
                    if (stroke.DrawingAttributes.Color == this.blackColor.Color)
                    {
                        this.modifyStrokeColor(stroke, this.whiteColor.Color);
                    }
                }
            }
        }

        private void modifyStrokeColor(InkStroke stroke, Color color)
        {
            var strokeClone = stroke.Clone();
            stroke.DrawingAttributes = new InkDrawingAttributes()
            {
                Color = color,
                FitToCurve = strokeClone.DrawingAttributes.FitToCurve,
                DrawAsHighlighter = strokeClone.DrawingAttributes.DrawAsHighlighter,
                IgnorePressure = strokeClone.DrawingAttributes.IgnorePressure,
                PenTip = strokeClone.DrawingAttributes.PenTip,
                PenTipTransform = strokeClone.DrawingAttributes.PenTipTransform,
                Size = strokeClone.DrawingAttributes.Size
            };
        }

        #endregion

        #endregion

        private void MainInkToolbar_ActiveToolChanged(InkToolbar sender, object args)
        {
            this.removeSelectionLayer();
            if (this.MainInkToolbar.ActiveTool == SelectionTool)
            {
                this.canTouchInk = this.ToggleTouchInkingButton.IsChecked.Value;
                this.ToggleTouchInkingButton.IsChecked = false;
                this.injectSelectionLayer();
            }
            else
            {
                this.ToggleTouchInkingButton.IsChecked = this.canTouchInk;
            }
        }

        private void SelectionLayer_SelectionMade(object sender, EventArgs e)
        {
            var selectionPoints = (sender as SelectionLayer).selectionPoints;
            this.MainCanvas.InkPresenter.StrokeContainer.SelectWithPolyLine(selectionPoints);
            this.MainCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
            this.MainCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(new Point(50,50));
            this.injectSelectionLayer();
        }

        private void removeSelectionLayer()
        {
            if (this.MainPanel.Children.OfType<SelectionLayer>().Count() > 0)
            {
                this.MainPanel.Children.Remove(this.MainPanel.Children.OfType<SelectionLayer>().First());
            }
        }

        private void injectSelectionLayer()
        {
            this.removeSelectionLayer();
            SelectionLayer selectionLayer = new SelectionLayer();
            selectionLayer.SelectionMade += SelectionLayer_SelectionMade;
            RelativePanel.SetAlignBottomWithPanel(selectionLayer, true);
            RelativePanel.SetAlignLeftWithPanel(selectionLayer, true);
            RelativePanel.SetAlignTopWithPanel(selectionLayer, true);
            RelativePanel.SetAlignRightWithPanel(selectionLayer, true);
            this.MainPanel.Children.Insert(this.MainPanel.Children.Count - 1, selectionLayer);
        }
    }
}
