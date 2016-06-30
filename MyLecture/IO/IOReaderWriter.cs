using System;
using System.Collections.Generic;
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
        readonly static string SLIDEFILE = "Slide.ink";
        readonly static string TEMPFILE = "Temp{0}.ink";
        private StorageFolder LectureFolder;
        private StorageFolder TempFolder;
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
        
        public async Task<InkStrokeContainer> SaveSnapshot(InkStrokeContainer inkStrokes, int index)
        {
            var fileName = string.Format(TEMPFILE, index);
            await this.saveInkStrokesToFile(inkStrokes, fileName);
            return await this.loadInkStrokesFromFile(fileName);
        }
        public async Task<InkStrokeContainer> SaveSlide(InkStrokeContainer inkStrokes)
        {
            await this.saveInkStrokesToFile(inkStrokes, SLIDEFILE);
            return await this.loadInkStrokesFromFile(SLIDEFILE);
        }
        private async Task saveInkStrokesToFile(InkStrokeContainer inkStrokes, string fileName)
        {
            var file = await this.createFile(this.TempFolder, fileName);
            var writeStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using (writeStream)
            {
                await inkStrokes.SaveAsync(writeStream);
                writeStream.Dispose();
            }
        }
        private async Task<InkStrokeContainer> loadInkStrokesFromFile(string fileName)
        {
            InkStrokeContainer inkStrokes = new InkStrokeContainer();
            var file = await this.getFile(this.TempFolder, fileName);
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
