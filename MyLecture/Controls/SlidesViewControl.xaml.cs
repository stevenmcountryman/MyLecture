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
        readonly static string FOLDERNAME = "Slides";
        readonly static string FILENAME = "Slide{0}.ink";
        private IOReaderWriter ReaderWriter;
        private List<StorageFile> slides = new List<StorageFile>();
        private int currentSlide = 0;

        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler SelectionMade;

        public delegate void NewSlideHandler(object sender, EventArgs e);
        public event NewSlideHandler NewSlideCreated;

        public SlidesViewControl()
        {
            this.InitializeComponent();

            this.ReaderWriter = new IOReaderWriter(FOLDERNAME);
            this.ReaderWriter.CreateFolder();
            this.initialSetup();
        }

        private void initialSetup()
        {
            this.createAddSlideControl();
            this.createNewBlankSlide();
        }

        private void createAddSlideControl()
        {
            this.slides.Add(null);
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

        private StorageFile getSlide()
        {
            return this.slides[this.currentSlide];
        }

        private void createNewBlankSlide()
        {
            this.slides.Add(null);
            RelativePanel panel = this.createNewSlidePanel(null);
            Viewbox viewbox = new Viewbox();
            viewbox.Child = panel;
            this.SlidesGrid.Items.Insert((this.SlidesGrid.Items.Count() - 1), viewbox);
            this.currentSlide = this.SlidesGrid.Items.Count() - 2;
        }

        public async void updateSlide(IOReaderWriter sourceReaderWriter, string fileName, Brush backgroundColor)
        {
            var viewbox = this.SlidesGrid.Items[this.currentSlide] as Viewbox;
            var panel = viewbox.Child as RelativePanel;
            panel.Background = backgroundColor;
            panel.Children.Clear();
            if (fileName != null)
            {                
                var inkCanvas = new InkCanvas();
                RelativePanel.SetAlignBottomWithPanel(inkCanvas, true);
                RelativePanel.SetAlignLeftWithPanel(inkCanvas, true);
                RelativePanel.SetAlignTopWithPanel(inkCanvas, true);
                RelativePanel.SetAlignRightWithPanel(inkCanvas, true);
                await sourceReaderWriter.LoadInkStrokes(fileName, inkCanvas.InkPresenter.StrokeContainer);
                panel.Children.Add(inkCanvas);
                this.SlidesGrid.Items.RemoveAt(this.currentSlide);
                this.SlidesGrid.Items.Insert(this.currentSlide, viewbox);

                StorageFile slide = await this.ReaderWriter.SaveInkStrokes(this.getCurrentFileName(), inkCanvas.InkPresenter.StrokeContainer);
                this.slides[this.currentSlide] = slide;
            }
            else
            {
                this.slides[this.currentSlide] = null;
            }
        }

        public async void updateCanvas(InkStrokeContainer sourceInkContainer)
        {
            var file = this.getSlide();
            if (file != null)
            {
                await this.ReaderWriter.LoadInkStrokes(file.Name, sourceInkContainer);
            }
        }

        private string getCurrentFileName()
        {
            return string.Format(FILENAME, this.currentSlide);
        }

        private void SlidesGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var itemIndex = this.SlidesGrid.Items.IndexOf(this.SlidesGrid.SelectedItem);
            if (itemIndex == this.SlidesGrid.Items.Count() - 1)
            {
                this.createNewBlankSlide();
                this.NewSlideCreated(sender, new EventArgs());
            }
            else
            {
                if (this.currentSlide != this.SlidesGrid.Items.IndexOf(this.SlidesGrid.SelectedItem))
                {
                    this.currentSlide = this.SlidesGrid.Items.IndexOf(this.SlidesGrid.SelectedItem);
                    this.SelectionMade(sender, new EventArgs());
                }
            }
        }
    }
}
