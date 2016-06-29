using MyLecture.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace MyLecture.Views
{
    /// <summary>
    /// This is the main drawing board class. This will be the main UI for the big screen view
    /// </summary>
    public sealed partial class DrawingBoard : Page
    {
        private Boolean isCanvasWhite = true;
        private SolidColorBrush whiteColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush blackColor = new SolidColorBrush(Colors.Black);
        private bool canTouchInk = false;
        private InkToolbarToolButton lastTool;
        private Point lastpoint;
        private int tempFileCount = 0;
        private int maxTempFile = 0;
        private StorageFolder folder;
        private SelectionLayer selectionLayer;

        public DrawingBoard()
        {
            this.InitializeComponent();
            
            this.MainCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            this.MainCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            this.folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("tempFiles", CreationCollisionOption.ReplaceExisting);
        }

        private async void resetTempFolder()
        {
            this.folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("tempFiles", CreationCollisionOption.ReplaceExisting);
            this.tempFileCount = 0;
            this.maxTempFile = 0;
            this.UndoTool.IsEnabled = false;
            this.RedoTool.IsEnabled = false;
            await this.saveTempFile();
        }

        private async Task saveTempFile()
        {
            if (this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                this.tempFileCount++;
                this.maxTempFile = this.tempFileCount;
                StorageFile file = await this.folder.CreateFileAsync("temp" + this.tempFileCount + ".ink", CreationCollisionOption.ReplaceExisting);
                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await this.MainCanvas.InkPresenter.StrokeContainer.SaveAsync(outputStream);
                    outputStream.Dispose();
                }
                this.SlidesView.updateSlide(file, this.InkPanel.Background);
            }
        }

        private async void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            this.UndoTool.IsEnabled = true;
            this.RedoTool.IsEnabled = false;
            this.lastTool = this.MainInkToolbar.ActiveTool;

            await this.saveTempFile();

            if (this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                this.SelectionTool.IsEnabled = true;
            }
            else
            {
                this.SelectionTool.IsEnabled = false;
            }
        }

        private async void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            this.UndoTool.IsEnabled = true;
            this.RedoTool.IsEnabled = false;
            this.lastTool = this.MainInkToolbar.ActiveTool;


            await this.saveTempFile();

            if (this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes().Count > 0)
            {
                this.SelectionTool.IsEnabled = true;
            }
            else
            {
                this.SelectionTool.IsEnabled = false;
            }
        }

        #region InkToolbar Custom Actions

        private void ToggleTouchInkingButton_Checked(object sender, RoutedEventArgs e)
        {
            this.toggleTouchInking();
        }

        private void ToggleTouchInkingButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.toggleTouchInking();
        }

        private void ToggleBackgroundColor_Checked(object sender, RoutedEventArgs e)
        {
            this.isCanvasWhite = false;
            this.invertUIColors();
        }

        private void ToggleBackgroundColor_Unchecked(object sender, RoutedEventArgs e)
        {
            this.isCanvasWhite = true;
            this.invertUIColors();
        }

        #region InkToolbar Custom Helpers

        private void toggleTouchInking()
        {
            if (this.ToggleTouchInkingButton.IsChecked == true)
            {
                this.MainCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
            }
            else
            {
                this.MainCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen;
            }
        }

        private void invertUIColors()
        {
            if (isCanvasWhite)
            {
                this.InkPanel.Background = this.whiteColor;
                this.SlideViewButton.Foreground = this.blackColor;

                var allStrokes = this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes();
                foreach (var stroke in allStrokes)
                {
                    if (stroke.DrawingAttributes.Color == this.whiteColor.Color)
                    {
                        this.modifyStrokeColor(stroke, this.blackColor.Color);
                    }
                }
            }
            else
            {
                this.InkPanel.Background = this.blackColor;
                this.SlideViewButton.Foreground = this.whiteColor;

                var allStrokes = this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes();
                foreach (var stroke in allStrokes)
                {
                    if (stroke.DrawingAttributes.Color == this.blackColor.Color)
                    {
                        this.modifyStrokeColor(stroke, this.whiteColor.Color);
                    }
                }
            }
        }

        private void modifyStrokeColor(InkStroke stroke, Color color)
        {
            var strokeClone = stroke.Clone();
            stroke.DrawingAttributes = new InkDrawingAttributes()
            {
                Color = color,
                FitToCurve = strokeClone.DrawingAttributes.FitToCurve,
                DrawAsHighlighter = strokeClone.DrawingAttributes.DrawAsHighlighter,
                IgnorePressure = strokeClone.DrawingAttributes.IgnorePressure,
                PenTip = strokeClone.DrawingAttributes.PenTip,
                PenTipTransform = strokeClone.DrawingAttributes.PenTipTransform,
                Size = strokeClone.DrawingAttributes.Size
            };
        }

        #endregion

        #endregion

        private void MainInkToolbar_ActiveToolChanged(InkToolbar sender, object args)
        {
            this.removeSelectionLayer();
            if (this.MainInkToolbar.ActiveTool == SelectionTool)
            {
                this.canTouchInk = this.ToggleTouchInkingButton.IsChecked.Value;
                this.ToggleTouchInkingButton.IsChecked = false;
                this.injectSelectionLayer();
            }
            else
            {
                this.ToggleTouchInkingButton.IsChecked = this.canTouchInk;
            }
        }
        private void SelectionLayer_SelectionMade(object sender, EventArgs e)
        {
            var selectionPoints = (sender as SelectionLayer).selectionPoints;
            if (selectionPoints != null)
            {
                this.MainCanvas.InkPresenter.StrokeContainer.SelectWithPolyLine(selectionPoints);
                double horizontalCenter = 0;
                double verticalCenter = 0;
                foreach (Point point in selectionPoints)
                {
                    horizontalCenter += point.X;
                    verticalCenter += point.Y;
                }
                var centerPoint = this.lastpoint = new Point(horizontalCenter / selectionPoints.Count(), verticalCenter / selectionPoints.Count());

                CopyPasteMovePopup cpmpopup = new CopyPasteMovePopup();
                Canvas.SetLeft(cpmpopup, centerPoint.X - 56);
                Canvas.SetTop(cpmpopup, centerPoint.Y - 14);

                cpmpopup.CopySelection += Cpmpopup_CopySelection;
                cpmpopup.MoveSelection += Cpmpopup_MoveSelection;
                cpmpopup.ClearSelection += Cpmpopup_ClearSelection;

                (sender as SelectionLayer).showCopyMovePopup(cpmpopup);
            }
        }

        private void Cpmpopup_ClearSelection(object sender, EventArgs e)
        {
            this.injectSelectionLayer();
        }

        private void Cpmpopup_MoveSelection(object sender, EventArgs e)
        {
            var cpmpopup = sender as CopyPasteMovePopup;
            if (cpmpopup.isMoving)
            {
                this.clickThroughSelectionLayer();
                this.MainCanvas.PointerMoved += MainCanvas_PointerMoved;
            }
        }

        private void clickThroughSelectionLayer()
        {
            if (this.InkPanel.Children.OfType<SelectionLayer>().Count() > 0)
            {
                var layer = this.InkPanel.Children.OfType<SelectionLayer>().First();
                layer.SetValue(Canvas.ZIndexProperty, 0);
                this.MainCanvas.SetValue(Canvas.ZIndexProperty, 1);
                this.MainCanvas.PointerReleased += MainCanvas_PointerReleased;
            }
        }

        private void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            this.MainCanvas.SetValue(Canvas.ZIndexProperty, 0);
            this.injectSelectionLayer();
            this.MainCanvas.PointerMoved -= MainCanvas_PointerMoved;
            this.MainCanvas.PointerReleased -= MainCanvas_PointerReleased;
        }

        private void Cpmpopup_CopySelection(object sender, EventArgs e)
        {
            this.MainCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
            
            this.MainCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(new Point(0,0));

            this.injectSelectionLayer();
        }

        private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this.MainCanvas).Position;
            if (this.lastpoint.X > 0 && this.lastpoint != currentPoint)
            {
                var translation = new Point(currentPoint.X - lastpoint.X, currentPoint.Y - lastpoint.Y);
                this.MainCanvas.InkPresenter.StrokeContainer.MoveSelected(translation);
            }
            this.lastpoint = currentPoint;
        }

        private void removeSelectionLayer()
        {
            if (this.selectionLayer != null)
            {
                this.InkPanel.Children.Remove(this.selectionLayer);
            }
        }

        private void injectSelectionLayer()
        {
            this.removeSelectionLayer();
            this.selectionLayer = new SelectionLayer();
            this.selectionLayer.SelectionMade += SelectionLayer_SelectionMade;
            RelativePanel.SetAlignBottomWithPanel(this.selectionLayer, true);
            RelativePanel.SetAlignLeftWithPanel(this.selectionLayer, true);
            RelativePanel.SetAlignTopWithPanel(this.selectionLayer, true);
            RelativePanel.SetAlignRightWithPanel(this.selectionLayer, true);
            this.InkPanel.Children.Add(this.selectionLayer);
        }

        private async void processUndo()
        {
            this.tempFileCount--;
            if (this.tempFileCount == 0 || (await this.folder.GetFilesAsync()).Count == 1)
            {
                this.MainCanvas.InkPresenter.StrokeContainer.Clear();
                this.SlidesView.updateSlide(null, this.InkPanel.Background);
            }
            else
            {
                StorageFile file = await this.folder.GetFileAsync("temp" + this.tempFileCount + ".ink");
                using (var inputStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    this.MainCanvas.InkPresenter.StrokeContainer.Clear();
                    await this.MainCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
                    inputStream.Dispose();
                }
                this.SlidesView.updateSlide(file, this.InkPanel.Background);
            }

            this.RedoTool.IsEnabled = true;
            if (this.tempFileCount == 0)
            {
                this.UndoTool.IsEnabled = false;
            }
        }

        private async void processRedo()
        {
            this.tempFileCount++;

            StorageFile file = await this.folder.GetFileAsync("temp" + this.tempFileCount + ".ink");
            using (var inputStream = await file.OpenAsync(FileAccessMode.Read))
            {
                this.MainCanvas.InkPresenter.StrokeContainer.Clear();
                await this.MainCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
                inputStream.Dispose();
            }
            this.SlidesView.updateSlide(file, this.InkPanel.Background);

            this.UndoTool.IsEnabled = true;
            if (this.tempFileCount == this.maxTempFile)
            {
                this.RedoTool.IsEnabled = false;
            }
        }

        private void UndoTool_Click(object sender, RoutedEventArgs e)
        {
            this.UndoTool.IsChecked = false;
            this.MainInkToolbar.ActiveTool = this.lastTool;
            this.processUndo();
        }

        private void RedoTool_Click(object sender, RoutedEventArgs e)
        {
            this.RedoTool.IsChecked = false;
            this.MainInkToolbar.ActiveTool = this.lastTool;
            this.processRedo();
        }

        private void SlideViewButton_Click(object sender, RoutedEventArgs e)
        {
            this.SlidesViewAnimation.To = 0;
            this.SlidesViewSlide.Begin();
        }

        private void SlidesView_NewSlideCreated(object sender, EventArgs e)
        {
            this.MainCanvas.InkPresenter.StrokeContainer.Clear();
            this.resetTempFolder();
            this.SlidesViewAnimation.To = 400;
            this.SlidesViewSlide.Begin();
        }

        private async void SlidesView_SelectionMade(object sender, EventArgs e)
        {
            this.MainCanvas.InkPresenter.StrokeContainer.Clear();
            var file = this.SlidesView.getSlide();
            if (file != null)
            {
                using (var inputStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await this.MainCanvas.InkPresenter.StrokeContainer.LoadAsync(inputStream);
                    inputStream.Dispose();
                }
            }
            else
            {
                this.MainCanvas.InkPresenter.StrokeContainer.Clear();
            }
            this.resetTempFolder();
            this.SlidesViewAnimation.To = 400;
            this.SlidesViewSlide.Begin();
        }
    }
}
