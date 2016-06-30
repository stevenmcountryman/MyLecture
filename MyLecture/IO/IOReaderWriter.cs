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
        /// <param name="folderName">The name of the folder associated with the IOReaderWriter object</param>
        public IOReaderWriter(string folderName)
        {
            this.FolderName = folderName;
        }
                
        /// <summary>
        /// Creates the folder for reading and writing to
        /// </summary>
        public async void CreateFolder()
        {
            this.Folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(this.FolderName, CreationCollisionOption.ReplaceExisting);
        }


        /// <summary>
        /// Saves the specified InkStrokeContainer to a given file name
        /// </summary>
        /// <param name="fileName">The name of the file to create for the Ink Strokes</param>
        /// <param name="strokes">The InkStrokeContainer object needed to save</param>
        /// <returns>The StorageFile object associated with the created file</returns>
        public async Task<StorageFile> SaveInkStrokes(string fileName, InkStrokeContainer strokes)
        {
            var file = await this.createFile(fileName);
            var writeStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            using (writeStream)
            {
                await strokes.SaveAsync(writeStream);
                writeStream.Dispose();
            }
            return file;
        }

        /// <summary>
        /// Saves the specified InkStrokeContainer to a given file name
        /// </summary>
        /// <param name="fileName">The name of the file to create for the Ink Strokes</param>
        /// <param name="strokes">The InkStrokeContainer object needed to save</param>
        /// <returns>The StorageFile object associated with the created file</returns>
        public async Task<StorageFile> LoadInkStrokes(string fileName, InkStrokeContainer strokes)
        {
            var file = await this.getFile(fileName);
            var readStream = await file.OpenAsync(FileAccessMode.Read);
            using (readStream)
            {
                await strokes.LoadAsync(readStream);
                readStream.Dispose();
            }
            return file;
        }

        private async Task<StorageFile> createFile(string fileName)
        {
            return await this.Folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
        }

        private async Task<StorageFile> getFile(string fileName)
        {
            return await this.Folder.GetFileAsync(fileName);
        }
    }
}
