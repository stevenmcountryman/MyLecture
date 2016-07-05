﻿using MyLecture.IO;
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
        private List<InkStrokeContainer> TempFiles;
        private int tempFileIndex;
        public string LectureName
        {
            get;
            private set;
        }

        public LectureFactory()
        {
            this.ReaderWriter = new IOReaderWriter();
            this.TempFiles = new List<InkStrokeContainer>();
            this.tempFileIndex = -1;
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
            this.TempFiles.Clear();
            this.tempFileIndex = -1;
        }
        public async void SaveSnapshot(InkStrokeContainer inkStrokes)
        {
            this.tempFileIndex++;
            if (this.TempFiles.Count() > 0)
            {
                this.TempFiles.RemoveRange(this.tempFileIndex, this.TempFiles.Count() - this.tempFileIndex);
            }
            if (inkStrokes.GetStrokes().Count() > 0)
            {
                var tempFile = await this.ReaderWriter.SaveSnapshot(inkStrokes, this.tempFileIndex);                
                this.TempFiles.Add(tempFile);
            }
            else
            {
                this.TempFiles.Add(null);
            }
        }
        public bool CanGoBack()
        {
            return tempFileIndex > 0;
        }
        public bool CanGoForward()
        {
            return this.tempFileIndex < this.TempFiles.Count() - 1;
        }
        public InkStrokeContainer LoadPreviousSnapshot()
        {
            this.tempFileIndex--;
            if (tempFileIndex >= 0)
            {
                return this.TempFiles[this.tempFileIndex];
            }
            else
            {
                return null;
            }
        }
        public InkStrokeContainer LoadNextSnapshot()
        {
            this.tempFileIndex++;
            if (this.tempFileIndex < this.TempFiles.Count())
            {
                return this.TempFiles[this.tempFileIndex];
            }
            else
            {
                return null;
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
                savePicker.SuggestedFileName = "UntitledLecture";

                StorageFile file = await savePicker.PickSaveFileAsync();
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("SaveFileToken", file);
                await this.ReaderWriter.SaveAllSlides(this.Slides, file);
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
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("OpenFileToken", file);
                this.Slides = await this.ReaderWriter.OpenAllSlides(file);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task OpenExistingLecture(StorageFile file)
        {
            this.LectureName = file.Name.Replace(".smc", "");
            this.Slides = await this.ReaderWriter.OpenAllSlides(file);
        }

        public async Task<bool> ExportToImages()
        {
            try
            {
                var savePicker = new FolderPicker();
                savePicker.FileTypeFilter.Add("*");
                savePicker.ViewMode = PickerViewMode.List;
                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

                StorageFolder folder = await savePicker.PickSingleFolderAsync();
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("ImagesFolderToken", folder);
                await this.ReaderWriter.SaveAllSlidesToImages(this.Slides, folder);
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
