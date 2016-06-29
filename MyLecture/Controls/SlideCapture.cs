using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyLecture.Controls
{
    public class SlideCapture
    {
        public StorageFile SlideFile
        {
            get;
            private set;
        }
        private InkCanvas canvas;
        public SlideCapture(InkCanvas canvas)
        {
            this.canvas = canvas;
        }

        public async Task exportToBitmap()
        {
            StorageFolder folder;
            try
            {
                folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Slides");
            }
            catch (Exception s)
            {
                folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Slides");
            }
            this.SlideFile = await folder.CreateFileAsync("test.ink", CreationCollisionOption.ReplaceExisting);
            using (var outputStream = await this.SlideFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await canvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
            }
        }
    }
}
