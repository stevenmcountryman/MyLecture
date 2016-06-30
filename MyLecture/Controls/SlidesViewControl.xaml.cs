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

namespace MyLecture.Controls
{
    public sealed partial class SlidesViewControl : UserControl
    {
        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler ChooseSlide;

        public delegate void NewSlideHandler(object sender, EventArgs e);
        public event NewSlideHandler CreateNewSlide;

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
    }
}
