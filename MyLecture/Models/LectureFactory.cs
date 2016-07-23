using MyLecture.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Input.Inking;

namespace MyLecture.Models
{
    public class LectureFactory
    {
        private IOReaderWriter ReaderWriter;
        private List<InkStrokeContainer> Slides;
        private List<InkStrokeContainer> UndoMemory;
        private List<InkStrokeContainer> RedoMemory;
        private InkStrokeContainer currentMemory;
        public string LectureName
        {
            get;
            private set;
        }

        public LectureFactory()
        {
            this.ReaderWriter = new IOReaderWriter();
            this.UndoMemory = new List<InkStrokeContainer>();
            this.RedoMemory = new List<InkStrokeContainer>();
            this.currentMemory = new InkStrokeContainer();
        }

        public void CreateNewLecture()
        {
            this.createNewSlideCollection();
        }

        public InkStrokeContainer OpenSlides()
        {
            return this.GetSlideAt(0);
        }

        public InkStrokeContainer GetSlideAt(int index)
        {
            return this.Slides[index];
        }

        public void DeleteSlide(int slideIndexToDelete)
        {
            this.Slides.RemoveAt(slideIndexToDelete);
        }

        public void MoveSlide(int slideIndexToMove, int destinationIndex)
        {
            var tempSlide = this.Slides[slideIndexToMove];
            this.Slides.RemoveAt(slideIndexToMove);
            this.Slides.Insert(destinationIndex, tempSlide);
        }

        public List<InkStrokeContainer> GetAllSlides()
        {
            return this.Slides;
        }

        public InkStrokeContainer AddNewBlankSlide()
        {
            this.Slides.Add(new InkStrokeContainer());
            return this.Slides.Last();
        }

        public void ClearSnapshots()
        {
            this.UndoMemory.Clear();
            this.RedoMemory.Clear();
            this.currentMemory = new InkStrokeContainer();
        }
        public async void SaveSnapshot(InkStrokeContainer inkStrokes)
        {
            this.UndoMemory.Add(this.currentMemory);
            this.RedoMemory.Clear();

            if (inkStrokes.GetStrokes().Count() > 0)
            {
                this.currentMemory = await this.ReaderWriter.SaveSnapshot(inkStrokes, this.UndoMemory.Count - 1);
            }
            else
            {
                this.currentMemory = new InkStrokeContainer();
            }
        }
        public bool CanGoBack()
        {
            return this.UndoMemory.Count > 0;
        }
        public bool CanGoForward()
        {
            return this.RedoMemory.Count > 0;
        }
        public InkStrokeContainer LoadPreviousSnapshot()
        {
            var inks = this.UndoMemory.Last();
            this.UndoMemory.RemoveAt(this.UndoMemory.Count() - 1);
            this.RedoMemory.Add(this.currentMemory);
            this.currentMemory = inks;
            return inks;
        }
        public InkStrokeContainer LoadNextSnapshot()
        {
            var inks = this.RedoMemory.Last();
            this.RedoMemory.RemoveAt(this.RedoMemory.Count() - 1);
            this.UndoMemory.Add(this.currentMemory);
            this.currentMemory = inks;
            return inks;
        }
        public void UpdateRecentFiles(string fileLocation)
        {
            var recentFiles = this.ReaderWriter.GetRecentList();
            if (recentFiles != null)
            {
                if (recentFiles.Contains(fileLocation))
                {
                    recentFiles.Remove(fileLocation);
                }
                recentFiles.Insert(0, fileLocation);
                var updatedList = recentFiles.GetRange(0, 5).ToArray();
                this.ReaderWriter.SaveRecentList(updatedList);
            }
            else
            {
                string[] newList = { fileLocation };
                this.ReaderWriter.SaveRecentList(newList);
            }
        }
        public List<string> GetRecentFilesList()
        {
            var recentFiles = this.ReaderWriter.GetRecentList();
            if (recentFiles != null)
            {
                return recentFiles;
            }
            else
            {
                return new List<string>();
            }
        }

        public async void SaveSlide(InkStrokeContainer inkStrokes, int slideIndex)
        {
            if (inkStrokes.GetStrokes().Count() > 0)
            {
                var slide = await this.ReaderWriter.SaveSlide(inkStrokes);
                this.Slides[slideIndex] = slide;
            }
            else
            {
                this.Slides[slideIndex] = inkStrokes;
            }
        }

        public void SaveLecture()
        {

        }

        public async Task<bool> SaveLectureAs(string titleText)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("InkLecture", new List<string>() { ".smc" });
                if (titleText.EndsWith(".smc"))
                {
                    savePicker.SuggestedFileName = titleText;
                }
                else
                {
                    savePicker.SuggestedFileName = titleText + ".smc";
                }

                StorageFile file = await savePicker.PickSaveFileAsync();
                string token = file.Name + file.DateCreated.UtcDateTime;
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
                await this.ReaderWriter.SaveAllSlides(this.Slides, token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> OpenExistingLecture()
        {
            try
            {
                var openPicker = new FileOpenPicker();
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".smc");
                StorageFile file = await openPicker.PickSingleFileAsync();
                this.LectureName = file.Name.Replace(".smc", "");
                string token = file.Name + file.DateCreated.UtcDateTime;
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
                this.Slides = await this.ReaderWriter.OpenAllSlides(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task OpenExistingLecture(string token)
        {
            this.LectureName = token.Substring(0, token.IndexOf(".smc"));
            this.Slides = await this.ReaderWriter.OpenAllSlides(token);
        }

        public async Task<bool> ExportToImages(string title)
        {
            try
            {
                var savePicker = new FolderPicker();
                savePicker.FileTypeFilter.Add("*");
                savePicker.ViewMode = PickerViewMode.List;
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                StorageFolder folder = await savePicker.PickSingleFolderAsync();
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("ImagesFolderToken", folder);
                await this.ReaderWriter.SaveAllSlidesToImages(this.Slides, folder, title);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExportToText(string titleText)
        {
            try
            {
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
                if (titleText.EndsWith(".txt"))
                {
                    savePicker.SuggestedFileName = titleText;
                }
                else
                {
                    savePicker.SuggestedFileName = titleText + ".txt";
                }

                StorageFile file = await savePicker.PickSaveFileAsync();
                string token = file.Name + file.DateCreated.UtcDateTime;
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
                await this.ReaderWriter.SaveAllSlidesToText(this.Slides, token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void createNewSlideCollection()
        {
            this.Slides = new List<InkStrokeContainer>();
            this.AddNewBlankSlide();
        }
    }
}
