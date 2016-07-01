using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
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
        readonly static string TEMPFILE = "Temp{0}.ink";
        private StorageFolder LectureFolder;
        private StorageFolder TempFolder;
        private StorageFolder SaveFolder;
        /// <summary>
        /// The StorageFolder object associated with the Folder used by this object
        /// </summary>
        public StorageFolder Folder
        {
            get;
            private set;
        }

        private string FolderName;

        /// <summary>
        /// Creates a new IOReaderWriter object with the given folder name
        /// </summary>
        public IOReaderWriter()
        {
            this.CreateFolderHierarchy();
        }

        /// <summary>
        /// Creates a new IOReaderWriter object with the given folder name
        /// </summary>
        /// <param name="folderName">The name of the folder associated with the IOReaderWriter object</param>
        public IOReaderWriter(string folderName)
        {
            this.FolderName = folderName;
        }
        
        public async Task SaveAllSlides(List<InkStrokeContainer> allSlides, StorageFile destination)
        {
            this.SaveFolder = await this.LectureFolder.CreateFolderAsync(SAVEFOLDER, CreationCollisionOption.ReplaceExisting);
            int slideIndex = 0;
            foreach (InkStrokeContainer slide in allSlides)
            {
                var fileName = string.Format(SLIDESAVEFILE, slideIndex);
                var file = await this.saveInkStrokesToFile(slide, fileName, this.SaveFolder);
                await Task.Run(() =>
                {
                    using (FileStream stream = new FileStream(destination.Path, FileMode.Open))
                    {
                        using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Update))
                        {
                            archive.CreateEntryFromFile(file.Path, fileName);
                        }
                    }
                });
                slideIndex++;
            }
        }
        public async Task<List<InkStrokeContainer>> OpenAllSlides(StorageFile file)
        {
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
                        }
                        slideIndex++;
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
        public async Task<InkStrokeContainer> SaveSnapshot(InkStrokeContainer inkStrokes, int index)
        {
            var fileName = string.Format(TEMPFILE, index);
            await this.saveInkStrokesToFile(inkStrokes, fileName, this.TempFolder);
            return await this.loadInkStrokesFromFile(fileName, this.TempFolder);
        }
        public async Task<InkStrokeContainer> SaveSlide(InkStrokeContainer inkStrokes)
        {
            await this.saveInkStrokesToFile(inkStrokes, SLIDEFILE, this.TempFolder);
            return await this.loadInkStrokesFromFile(SLIDEFILE, this.TempFolder);
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

        private async void CreateFolderHierarchy()
        {
            this.LectureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LECTUREFOLDER, CreationCollisionOption.ReplaceExisting);
            this.TempFolder = await this.LectureFolder.CreateFolderAsync(TEMPFOLDER, CreationCollisionOption.ReplaceExisting);
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
