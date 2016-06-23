using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MyLecture.Controls
{
    public sealed partial class CopyPasteMovePopup : UserControl
    {
        public delegate void CopySelectionHandler(object sender, EventArgs e);
        public event CopySelectionHandler CopySelection;

        public delegate void MoveSelectionHandler(object sender, EventArgs e);
        public event MoveSelectionHandler MoveSelection;

        public delegate void ClearSelectionHandler(object sender, EventArgs e);
        public event ClearSelectionHandler ClearSelection;

        public bool isMoving
        {
            get;
            private set;
        }

        public CopyPasteMovePopup()
        {
            this.InitializeComponent();

            this.isMoving = false;
        }

        private void CopySelectionButton_Click(object sender, RoutedEventArgs e)
        {
            this.CopySelection(this, new EventArgs());
        }

        private void MoveSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMoving)
            {
                this.isMoving = false;
                this.ButtonPanel.Opacity = 1.0;
                this.MoveSelection(this, new EventArgs());
            }
            else
            {
                this.isMoving = true;
                this.ButtonPanel.Opacity = 0.4;
                this.MoveSelection(this, new EventArgs());
            }
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            this.ClearSelection(this, new EventArgs());
        }
    }
}
