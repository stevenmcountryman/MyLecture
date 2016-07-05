using Microsoft.Graphics.Canvas;
using MyLecture.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace MyLecture.Controls
{
    public sealed partial class SlidesViewControl : UserControl
    {
        readonly static int HIDDEN = -400;
        readonly static int VISIBLE = 0;

        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler ChooseSlide;

        public delegate void NewSlideHandler(object sender, EventArgs e);
        public event NewSlideHandler CreateNewSlide;

        public delegate void SlidesClosedHandler(object sender, EventArgs e);
        public event SlidesClosedHandler SlidesClosed;

        public delegate void SaveButtonTapHandler(object sender, EventArgs e);
        public event SaveButtonTapHandler SaveButtonTapped;

        public delegate void ExportHandler(object sender, EventArgs e);
        public event ExportHandler ExportButtonPressed;

        public delegate void SlideDeleteHandler(object sender, EventArgs e);
        public event SlideDeleteHandler SlideDeleted;

        public delegate void SlideMovedHandler(object sender, EventArgs e);
        public event SlideMovedHandler SlideMoved;

        public int SlideIndex
        {
            get;
            private set;
        }
        public int SlideIndexToDelete
        {
            get;
            private set;
        }

        public string GetTitle()
        {
            return this.TitleText.Text;
        }

        public SlidesViewControl()
        {
            this.InitializeComponent();
            this.createAddSlideControl();
            this.createNewBlankSlide();
            this.SlideIndex = 0;
        }

        public void OpenSlidesView()
        {
            this.SlidesViewAnimation.To = VISIBLE;
            this.BackgroundOpacity.To = 0.75;
            this.SlidesViewSlide.Begin();
        }

        public void CloseSlidesView()
        {
            this.SlidesViewAnimation.To = HIDDEN;
            this.BackgroundOpacity.To = 0;
            this.SlidesViewSlide.Completed += SlidesViewSlide_Completed;
            this.SlidesViewSlide.Begin();
        }

        public void ShowLoadedSlides(List<InkStrokeContainer> slides, string lectureName)
        {
            this.UpdateSlide(slides[0], new SolidColorBrush(Colors.White));
            for (int i = 1; i < slides.Count(); i++)
            {
                this.SlideIndex++;
                this.createNewBlankSlide();
                this.UpdateSlide(slides[i], new SolidColorBrush(Colors.White));
            }
            this.SlideIndex = 0;
            this.TitleText.Text = lectureName;
        }

        private void SlidesViewSlide_Completed(object sender, object e)
        {
            this.SlidesViewSlide.Completed -= SlidesViewSlide_Completed;
            this.SlidesClosed(this, new EventArgs());
        }

        private void createAddSlideControl()
        {
            SymbolIcon icon = new SymbolIcon(Symbol.Add);
            Viewbox iconBox = new Viewbox()
            {
                Width = 100,
                Height = 100,
                Child = icon
            };
            RelativePanel.SetAlignHorizontalCenterWithPanel(iconBox, true);
            RelativePanel.SetAlignVerticalCenterWithPanel(iconBox, true);
            RelativePanel panel = this.createNewSlidePanel(iconBox);
            Viewbox viewbox = new Viewbox();
            viewbox.Child = panel;
            viewbox.Tapped += AddSlideButton_Tapped;
            this.SlidesGrid.Items.Add(viewbox);
        }

        private void createNewBlankSlide()
        {
            RelativePanel panel = this.createNewSlidePanel(null);
            Viewbox viewbox = new Viewbox();
            viewbox.Child = panel;
            viewbox.Tapped += Slide_Tapped;
            viewbox.RightTapped += Slide_RightTapped;
            this.SlidesGrid.Items.Insert((this.SlidesGrid.Items.Count() - 1), viewbox);
        }
        public void AddNewBlankSlide()
        {
            this.SlideIndex = this.SlidesGrid.Items.Count - 1;
            this.createNewBlankSlide();
            this.CreateNewSlide(this, new EventArgs());
        }
        private void AddSlideButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.AddNewBlankSlide();
        }
        private void Slide_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SlideIndex = this.SlidesGrid.Items.IndexOf(sender as Viewbox);
            this.ChooseSlide(sender, new EventArgs());
        }

        private void Slide_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            this.SlideIndexToDelete = this.SlidesGrid.Items.IndexOf(sender as Viewbox);
            MenuFlyout popUp = new MenuFlyout();
            MenuFlyoutItem deleteItem = new MenuFlyoutItem();
            deleteItem.Text = "Delete";
            deleteItem.Click += DeleteItem_Click;
            popUp.Items.Add(deleteItem);
            popUp.ShowAt(sender as Viewbox);
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            this.SlidesGrid.Items.RemoveAt(this.SlideIndexToDelete);
            if (this.SlideIndex == this.SlideIndexToDelete)
            {
                this.SlideIndex = 0;
            }
            this.SlideDeleted(this, new EventArgs());
        }

        private RelativePanel createNewSlidePanel(UIElement element)
        {
            RelativePanel panel = new RelativePanel()
            {
                Width = 1600,
                Height = 1000,
                Background = new SolidColorBrush(Colors.White)
            };
            if (element != null)
            {
                panel.Children.Add(element);
            }
            return panel;
        }

        public void UpdateSlide(InkStrokeContainer inkStrokes, Brush backgroundColor)
        {
            var viewbox = this.SlidesGrid.Items[this.SlideIndex] as Viewbox;
            var panel = viewbox.Child as RelativePanel;
            panel.Background = backgroundColor;
            panel.Children.Clear();
            var inkCanvas = new InkCanvas();
            RelativePanel.SetAlignBottomWithPanel(inkCanvas, true);
            RelativePanel.SetAlignLeftWithPanel(inkCanvas, true);
            RelativePanel.SetAlignTopWithPanel(inkCanvas, true);
            RelativePanel.SetAlignRightWithPanel(inkCanvas, true);
            inkCanvas.InkPresenter.StrokeContainer = inkStrokes;
            panel.Children.Add(inkCanvas);            
        }

        private void BackgroundShade_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSlidesView();
        }

        private void SaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SaveButtonTapped(sender, new EventArgs());
        }

        private void ExportButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ExportButtonPressed(this, new EventArgs());
        }

        private void SlidesGrid_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var draggedItem = e.Items[0];
            var listIndex = this.SlidesGrid.Items.IndexOf(draggedItem);
            this.DeleteIconVisibility.From = 0.0;
            this.DeleteIconVisibility.To = 1.0;
            this.DeleteIconAppear.RepeatBehavior = RepeatBehavior.Forever;
            this.DeleteIconAppear.Begin();
        }

        private void BackgroundShade_DragEnter(object sender, DragEventArgs e)
        {
            this.DeleteIconVisibility.From = 0.1;
            this.DeleteIconVisibility.To = 1.0;
            this.DeleteIconAppear.RepeatBehavior = RepeatBehavior.Forever;
            this.DeleteIconAppear.Begin();

            if (e.DataView.GetType() == typeof(GridViewItem))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }

            // Add this
            e.Handled = true;
        }

        private void BackgroundShade_Drop(object sender, DragEventArgs e)
        {

        }

        private void BackgroundShade_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void SlideViewButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSlidesView();
        }
    }
}
