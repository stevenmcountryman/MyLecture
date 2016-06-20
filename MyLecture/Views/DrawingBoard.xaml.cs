using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        public DrawingBoard()
        {
            this.InitializeComponent();
        }

        #region InkToolbar Custom Actions

        private void ToggleTouchInkingButton_Checked(object sender, RoutedEventArgs e)
        {
            this.canTouchInk = true;
            this.toggleTouchInking();
        }

        private void ToggleTouchInkingButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.canTouchInk = false;
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
            if (this.canTouchInk)
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
            }
            else
            {
                this.MainPanel.Background = this.blackColor;
                this.SlideViewButton.Foreground = this.whiteColor;
            }
        }

        #endregion

        #endregion
    }
}
