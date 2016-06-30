using MyLecture.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;

namespace MyLecture.Models
{
    public class LectureFactory
    {
        private IOReaderWriter ReaderWriter;
        private List<InkStrokeContainer> Slides;
        private List<InkStrokeContainer> TempFiles;
        private int tempFileIndex;

        public LectureFactory()
        {
            this.ReaderWriter = new IOReaderWriter();
            this.TempFiles = new List<InkStrokeContainer>();
            this.tempFileIndex = -1;
        }

        public void OpenExistingLecture()
        {
            //this.Slides = deserialized slides file
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

        public void AddNewBlankSlide()
        {
            this.Slides.Add(new InkStrokeContainer());
        }

        public async void SaveSnapshot(InkStrokeContainer inkStrokes)
        {
            this.tempFileIndex++;
            if (this.TempFiles.Count() > 0)
            {
                this.TempFiles.RemoveRange(this.tempFileIndex, this.TempFiles.Count() - this.tempFileIndex);
            }
            if (inkStrokes != null)
            {
                var tempFile = await this.ReaderWriter.SaveSnapshot(inkStrokes, this.tempFileIndex);                
                this.TempFiles.Add(tempFile);
            }
            else
            {
                this.TempFiles.Add(null);
            }
        }
        public InkStrokeContainer LoadPreviousSnapshot()
        {
            if (this.tempFileIndex - 1 >= 0)
            {
                this.tempFileIndex--;
                return this.TempFiles[this.tempFileIndex];
            }
            else
            {
                return null;
            }
        }
        public InkStrokeContainer LoadNextSnapshot()
        {
            if (this.tempFileIndex + 1 < this.TempFiles.Count())
            {
                this.tempFileIndex++;
                return this.TempFiles[this.tempFileIndex];
            }
            else
            {
                return null;
            }
        }

        public async void SaveSlide(InkStrokeContainer inkStrokes, int slideIndex)
        {
            var slide = await this.ReaderWriter.SaveSlide(inkStrokes);
            this.Slides[slideIndex] = slide;
        }

        public void SaveLecture()
        {
            var serializer = new JsonSerializer();
            using (StreamWriter writer = File.CreateText("Untitled.smc"))
            {
                var jsonWriter = new JsonTextWriter(writer);
                serializer.Serialize(jsonWriter, Slides);
                writer.Dispose();
            }
        }

        public void SaveLectureAs()
        {

        }

        public void ExportToPDF()
        {

        }

        private void createNewSlideCollection()
        {
            this.Slides = new List<InkStrokeContainer>();
            this.AddNewBlankSlide();
        }
    }
}
