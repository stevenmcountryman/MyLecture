using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyLecture.Controls
{
    public sealed partial class SlidesViewControl : UserControl
    {
        private StorageFolder slidesFolder;
        private List<StorageFile> slides = new List<StorageFile>();
        private int currentSlide = 0;

        public delegate void SelectionMadeHandler(object sender, EventArgs e);
        public event SelectionMadeHandler SelectionMade;

        public delegate void NewSlideHandler(object sender, EventArgs e);
        public event NewSlideHandler NewSlideCreated;

        public SlidesViewControl()
        {
            this.InitializeComponent();
            this.createNewSlide();
            this.loadSlidesFolder();
        }

        public StorageFile getSlide()
        {
            return this.slides[this.currentSlide];
        }

        private async void loadSlidesFolder()
        {
            try
            {
                this.slidesFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Slides");
                await this.slidesFolder.DeleteAsync();
                this.slidesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Slides");
            }
            catch
            {
                this.slidesFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Slides");
            }
        }

        private void createNewSlide()
        {
            this.slides.Add(null);
            RelativePanel panel = new RelativePanel()
            {
                Width = 1600,
                Height = 1000,
                Background = new SolidColorBrush(Colors.White)
            };
            Viewbox viewbox = new Viewbox();
            viewbox.Child = panel;
            this.SlidesGrid.Items.Add(viewbox);
            this.currentSlide = this.SlidesGrid.Items.Count() - 1;
        }

        public async void updateSlide(StorageFile file, Brush backgroundColor)
        {
            var viewbox = this.SlidesGrid.Items[this.currentSlide] as Viewbox;
            var panel = viewbox.Child as RelativePanel;
            panel.Background = backgroundColor;
            var inkCanvas = new InkCanvas();
            RelativePanel.SetAlignBottomWithPanel(inkCanvas, true);
            RelativePanel.SetAlignLeftWithPanel(inkCanvas, true);
            RelativePanel.SetAlignTopWithPanel(inkCanvas, true);
            RelativePanel.SetAlignRightWithPanel(inkCanvas, true);
            using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(outputStream);
            }
            panel.Children.Clear();
            panel.Children.Add(inkCanvas);
            this.SlidesGrid.Items.RemoveAt(this.currentSlide);
            this.SlidesGrid.Items.Insert(this.currentSlide, viewbox);


            StorageFile slide = await this.slidesFolder.CreateFileAsync("Slide" + this.currentSlide + ".ink", CreationCollisionOption.ReplaceExisting);
            using (var outputStream = await slide.OpenAsync(FileAccessMode.ReadWrite))
            {
                await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
            }
            this.slides[this.currentSlide] = slide;
        }

        private void SlidesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.currentSlide = this.SlidesGrid.Items.IndexOf(e.ClickedItem);
            this.SelectionMade(sender, new EventArgs());
        }

        private void SlidesGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.currentSlide = this.SlidesGrid.Items.IndexOf(this.SlidesGrid.SelectedItem);
            this.SelectionMade(sender, new EventArgs());
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.createNewSlide();
            this.NewSlideCreated(sender, new EventArgs());
        }
    }
}
