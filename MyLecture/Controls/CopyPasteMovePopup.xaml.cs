using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        public CopyPasteMovePopup()
        {
            this.InitializeComponent();
        }

        private void CopySelectionButton_Click(object sender, RoutedEventArgs e)
        {
            this.CopySelection(this, new EventArgs());
        }

        private void MoveSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            this.ButtonPanel.Opacity = 0.4;
            this.MoveSelection(this, new EventArgs());
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            this.ClearSelection(this, new EventArgs());
        }
    }
}
