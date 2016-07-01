using MyLecture.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input.Inking;
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
        readonly static Color TRANSPARENT = Color.FromArgb(0, 0, 0, 0);
        readonly static Color TRANSLUCENT = Color.FromArgb(100, 0, 0, 0);
        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler ChooseSlide;

        public delegate void NewSlideHandler(object sender, EventArgs e);
        public event NewSlideHandler CreateNewSlide;

        public delegate void SlidesClosedHandler(object sender, EventArgs e);
        public event SlidesClosedHandler SlidesClosed;

        public delegate void SaveButtonTapHandler(object sender, EventArgs e);
        public event SaveButtonTapHandler SaveButtonTapped;

        public int SlideIndex
        {
            get;
            private set;
        }

        public SlidesViewControl()
        {
            this.InitializeComponent();
            this.createAddSlideControl();
            this.createNewBlankSlide();
        }

        public void OpenSlidesView()
        {
            this.SlidesViewAnimation.From = HIDDEN;
            this.SlidesViewAnimation.To = VISIBLE;
            this.BackgroundOpacity.From = TRANSPARENT;
            this.BackgroundOpacity.To = TRANSLUCENT;
            this.SlidesViewSlide.Begin();
        }

        public void CloseSlidesView()
        {
            this.SlidesViewAnimation.From = VISIBLE;
            this.SlidesViewAnimation.To = HIDDEN;
            this.BackgroundOpacity.From = TRANSLUCENT;
            this.BackgroundOpacity.To = TRANSPARENT;
            this.SlidesViewSlide.Completed += SlidesViewSlide_Completed;
            this.SlidesViewSlide.Begin();
        }

        public void ShowLoadedSlides(List<InkStrokeContainer> slides)
        {
            this.UpdateSlide(slides[0], new SolidColorBrush(Colors.White));
            for (int i = 1; i < slides.Count(); i++)
            {
                this.SlideIndex++;
                this.createNewBlankSlide();
                this.UpdateSlide(slides[i], new SolidColorBrush(Colors.White));
            }
            this.SlideIndex = 0;
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
            this.SlidesGrid.Items.Add(viewbox);
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

        private void createNewBlankSlide()
        {
            RelativePanel panel = this.createNewSlidePanel(null);
            Viewbox viewbox = new Viewbox();
            viewbox.Child = panel;
            this.SlidesGrid.Items.Insert((this.SlidesGrid.Items.Count() - 1), viewbox);
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

        private void SlidesGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SlideIndex = this.SlidesGrid.Items.IndexOf(this.SlidesGrid.SelectedItem);
            if (this.SlideIndex == this.SlidesGrid.Items.Count() - 1)
            {
                this.createNewBlankSlide();
                this.CreateNewSlide(sender, new EventArgs());
            }
            else
            {
                this.ChooseSlide(sender, new EventArgs());
            }
        }

        private void BackgroundShade_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.CloseSlidesView();
        }

        private void SaveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.SaveButtonTapped(sender, new EventArgs());
        }
    }
}
