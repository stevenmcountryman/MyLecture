using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI;
using Windows.UI.Input.Inking;

namespace MyLecture.IO
{
    /// <summary>
    /// IOReaderWriter is used to read and write files to a given folder
    /// </summary>
    public class IOReaderWriter
    {
        readonly static string LECTUREFOLDER = "Lecture_Root";
        readonly static string TEMPFOLDER = "Temp";
        readonly static string SAVEFOLDER = "Save";
        readonly static string SLIDEFILE = "Slide.ink";
        readonly static string SLIDESAVEFILE = "Slide{0}.ink";
        readonly static string TEMPFILE = "Temp.ink";
        readonly static string IMAGEFILE = "Slide{0}.jpg";
        private StorageFolder LectureFolder;
        private StorageFolder TempFolder;
        private StorageFolder SaveFolder;
        private StorageFolder ImagesFolder;
        /// <summary>
        /// The StorageFolder object associated with the Folder used by this object
        /// </summary>
        public StorageFolder Folder
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new IOReaderWriter object with the given folder name
        /// </summary>
        public IOReaderWriter()
        {
            this.setUp();
        }

        private async void setUp()
        {
            await this.CreateFolderHierarchy();
        }

        public async Task SaveAllSlidesToImages(List<InkStrokeContainer> allSlides, List<bool> slideBackgroundConfig, StorageFolder destination, string folderTitle)
        {
            this.ImagesFolder = await destination.CreateFolderAsync(folderTitle, CreationCollisionOption.ReplaceExisting);
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            int imageIndex = 1;
            foreach (var slide in allSlides)
            {
                CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, 1600, 1000, 96);

                using (var drawingSession = renderTarget.CreateDrawingSession())
                {
                    if (slideBackgroundConfig[imageIndex - 1] == true)
                    {
                        drawingSession.Clear(Colors.White);
                    }
                    else
                    {
                        drawingSession.Clear(Colors.Black);
                    }
                    drawingSession.DrawInk(slide.GetStrokes());
                }
                var file = await this.createFile(this.ImagesFolder, string.Format(IMAGEFILE, imageIndex));
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
                }
                imageIndex++;
            }
        }
        public async Task SaveAllSlides(List<InkStrokeContainer> allSlides, List<bool> backgroundConfig, string token)
        {
            this.SaveFolder = await this.LectureFolder.CreateFolderAsync(SAVEFOLDER, CreationCollisionOption.ReplaceExisting);
            int slideIndex = 0;
            StorageFile tempZip = await this.createFile(this.SaveFolder, "TempZip");
            foreach (InkStrokeContainer slide in allSlides)
            {
                var fileName = string.Format(SLIDESAVEFILE, slideIndex);
                var file = await this.saveInkStrokesToFile(slide, fileName, this.SaveFolder);
                await Task.Run(() =>
                {
                    using (FileStream stream = new FileStream(tempZip.Path, FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Update))
                        {                            
                            archive.CreateEntryFromFile(file.Path, fileName);
                        }
                    }
                });
                slideIndex++;
            }
            var config = await this.SaveAllSlidesConfig(backgroundConfig);
            await Task.Run(() =>
            {
                using (FileStream stream = new FileStream(tempZip.Path, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Update))
                    {

                        archive.CreateEntryFromFile(config.Path, config.Name);
                    }
                }
            });
            StorageFile destination = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            await tempZip.CopyAndReplaceAsync(destination);
        }
        public async Task<List<InkStrokeContainer>> OpenAllSlides(string token)
        {
            StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            await this.CreateFolderHierarchy();
            List<InkStrokeContainer> allSlides = new List<InkStrokeContainer>();
            this.SaveFolder = await this.LectureFolder.CreateFolderAsync(SAVEFOLDER, CreationCollisionOption.ReplaceExisting);
            int slideIndex = 0;
            await Task.Run(() =>
            {
                using (ZipArchive archive = ZipFile.OpenRead(file.Path))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".ink", StringComparison.OrdinalIgnoreCase))
                        {
                            var fileName = string.Format(SLIDESAVEFILE, slideIndex);
                            entry.ExtractToFile(Path.Combine(this.SaveFolder.Path, fileName));
                            slideIndex++;
                        }
                    }
                }
            });
            for(int i = 0; i < slideIndex; i++)
            {
                InkStrokeContainer inkStrokes = await this.loadInkStrokesFromFile(string.Format(SLIDESAVEFILE, i), this.SaveFolder);
                allSlides.Add(inkStrokes);
            }
            return allSlides;
        }
        public async Task<List<bool>> OpenSlideConfig(string token)
        {
            StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            List<bool> slideBackgroundWhite = new List<bool>();
            await Task.Run(() =>
            {
                using (ZipArchive archive = ZipFile.OpenRead(file.Path))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                        {
                            entry.ExtractToFile(Path.Combine(this.SaveFolder.Path, "Config.txt"));
                        }
                    }
                }
            });
            StorageFile config = await StorageFile.GetFileFromPathAsync(Path.Combine(this.SaveFolder.Path, "Config.txt"));
            using (var stream = await config.OpenStreamForReadAsync())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string entry = "";
                    while ((entry = reader.ReadLine()) != null)
                    {
                        if (entry == "white")
                        {
                            slideBackgroundWhite.Add(true);
                        }
                        else
                        {
                            slideBackgroundWhite.Add(false);
                        }
                    }
                }
            }
            return slideBackgroundWhite;
        }
        public async Task<InkStrokeContainer> SaveSnapshot(InkStrokeContainer inkStrokes)
        {
            await this.saveInkStrokesToFile(inkStrokes, TEMPFILE, this.TempFolder);
            return await this.loadInkStrokesFromFile(TEMPFILE, this.TempFolder);
        }
        public async Task<InkStrokeContainer> SaveSlide(InkStrokeContainer inkStrokes)
        {
            await this.saveInkStrokesToFile(inkStrokes, SLIDEFILE, this.TempFolder);
            return await this.loadInkStrokesFromFile(SLIDEFILE, this.TempFolder);
        }
        public List<string> GetRecentList()
        {
            if (ApplicationData.Current.LocalSettings.Values["RecentList"] != null)
            {
                var recentFileList = (ApplicationData.Current.LocalSettings.Values["RecentList"] as string[]).ToList<string>();
                return recentFileList;
            }
            else return null;
        }
        public void SaveRecentList(string[] recentFileList)
        {
            if (ApplicationData.Current.LocalSettings.Values["RecentList"] != null)
            {
                ApplicationData.Current.LocalSettings.Values["RecentList"] = recentFileList;
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add("RecentList", recentFileList);
            }
        }
        private async Task<StorageFile> saveInkStrokesToFile(InkStrokeContainer inkStrokes, string fileName, StorageFolder folder)
        {
            var file = await this.createFile(folder, fileName);
            var writeStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using (writeStream)
            {
                await inkStrokes.SaveAsync(writeStream);
                writeStream.Dispose();
            }
            return file;
        }
        private async Task<InkStrokeContainer> loadInkStrokesFromFile(string fileName, StorageFolder folder)
        {
            try
            {
                InkStrokeContainer inkStrokes = new InkStrokeContainer();
                var file = await this.getFile(folder, fileName);
                var readStream = await file.OpenAsync(FileAccessMode.Read);
                using (readStream)
                {
                    await inkStrokes.LoadAsync(readStream);
                    readStream.Dispose();
                }
                return inkStrokes;
            }
            catch
            {
                return null;
            }
        }

        private async Task CreateFolderHierarchy()
        {
            this.LectureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LECTUREFOLDER, CreationCollisionOption.ReplaceExisting);
            this.TempFolder = await this.LectureFolder.CreateFolderAsync(TEMPFOLDER, CreationCollisionOption.ReplaceExisting);
        }

        public async Task SaveAllSlidesToText(List<InkStrokeContainer> allSlides, string token)
        {
            this.SaveFolder = await this.LectureFolder.CreateFolderAsync(SAVEFOLDER, CreationCollisionOption.ReplaceExisting);
            StorageFile tempText = await this.createFile(this.SaveFolder, "TempText");
            int slideIndex = 1;
            using (var stream = await tempText.OpenStreamForWriteAsync())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach (InkStrokeContainer slide in allSlides)
                    {
                        var title = string.Format("Slide {0}:", slideIndex);
                        var text = "";
                        if (slide.GetStrokes().Count > 0)
                        {
                            text = await this.transcribeSlideToText(slide);
                        }
                        await writer.WriteLineAsync(title);
                        await writer.WriteLineAsync(text);
                        await writer.WriteLineAsync("-----");
                        slideIndex++;
                    }
                }
            }
            StorageFile destination = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            await tempText.CopyAndReplaceAsync(destination);
        }
        private async Task<StorageFile> SaveAllSlidesConfig(List<bool> backgroundConfig)
        {
            StorageFile config = await this.createFile(this.SaveFolder, "Config.txt");
            using (var stream = await config.OpenStreamForWriteAsync())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    foreach(var entry in backgroundConfig)
                    {
                        if (entry == true)
                        {
                            await writer.WriteLineAsync("white");
                        }
                        else
                        {
                            await writer.WriteLineAsync("black");
                        }
                    }
                }
            }
            return config;
        }

        private async Task<string> transcribeSlideToText(InkStrokeContainer inkStrokes)
        {
            var handwritingToText = "";
            if (inkStrokes.GetStrokes().Count > 0)
            {
                var inkRecog = new InkRecognizerContainer();
                var results = await inkRecog.RecognizeAsync(inkStrokes, InkRecognitionTarget.All);
                foreach (var result in results)
                {
                    handwritingToText += result.GetTextCandidates()[0] + " ";
                }
            }
            return handwritingToText;
        }

        private async Task<StorageFile> createFile(StorageFolder folder, string fileName)
        {
            return await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        }

        private async Task<StorageFile> getFile(StorageFolder folder, string fileName)
        {
            return await folder.GetFileAsync(fileName);
        }
    }
}
