using MyLecture.Controls;
using MyLecture.IO;
using MyLecture.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MyLecture.Views
{
    /// <summary>
    /// This is the main drawing board class. This will be the main UI for the big screen view
    /// </summary>
    public sealed partial class DrawingBoard : Page
    {
        private LectureFactory lectureFactory;
        readonly static string FOLDERNAME = "TempFiles";
        readonly static string FILENAME = "Temp{0}.ink";
        readonly static CoreInputDeviceTypes ALL_INPUTS = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
        readonly static CoreInputDeviceTypes PEN_ONLY = CoreInputDeviceTypes.Pen;
        private IOReaderWriter ReaderWriter;

        private Boolean isCanvasWhite = true;
        private SolidColorBrush whiteColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush blackColor = new SolidColorBrush(Colors.Black);
        private bool canTouchInk = false;
        private InkToolbarToolButton lastTool;
        private Point lastpoint;
        private int tempFileCount = 0;
        private int maxTempFile = 0;

        /// <summary>
        /// Creates a new DrawingBoard object for inking
        /// </summary>
        public DrawingBoard()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.lectureFactory = new LectureFactory();
            this.lectureFactory.CreateNewLecture();
            this.MainCanvas.InkPresenter.StrokeContainer = this.lectureFactory.OpenSlides();
            this.ReaderWriter = new IOReaderWriter(FOLDERNAME);
            this.ReaderWriter.CreateFolder();
            this.MainCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            this.MainCanvas.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
        }

        #region UI Event Handlers
        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            this.updateInkCanvasAndTools();
        }
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            this.updateInkCanvasAndTools();
        }
        private void ToggleTouchInkingButton_Checked(object sender, RoutedEventArgs e)
        {
            this.toggleTouchInking(true);
        }
        private void ToggleTouchInkingButton_Unchecked(object sender, RoutedEventArgs e)
        {
            this.toggleTouchInking(false);
        }
        private void ToggleBackgroundColor_Checked(object sender, RoutedEventArgs e)
        {
            this.invertUIColors();
        }
        private void ToggleBackgroundColor_Unchecked(object sender, RoutedEventArgs e)
        {
            this.invertUIColors();
        }
        private void MainInkToolbar_ActiveToolChanged(InkToolbar sender, object args)
        {
            if (this.MainInkToolbar.ActiveTool == SelectionTool)
            {
                this.canTouchInk = this.ToggleTouchInkingButton.IsChecked.Value;
                this.toggleTouchInking(false);
                this.SelectionCanvas.Visibility = Visibility.Visible;
            }
            else
            {
                this.toggleTouchInking(this.canTouchInk);
                this.SelectionCanvas.Visibility = Visibility.Collapsed;
            }
        }
        private void SelectionLayer_SelectionMade(object sender, EventArgs e)
        {
            this.selectInkStrokes();
        }
        private void UndoTool_Click(object sender, RoutedEventArgs e)
        {
            this.processUndo();
        }

        private void RedoTool_Click(object sender, RoutedEventArgs e)
        {
            this.processRedo();
        }
        #endregion

        #region Canvas+Tools Helpers
        private bool isMainCanvasEmpty()
        {
            return this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes().Count == 0;
        }
        private void clearCanvas()
        {
            this.MainCanvas.InkPresenter.StrokeContainer.Clear();
            this.resetTempFolder();
        }
        private async void updateInkCanvasAndTools()
        {
            this.UndoTool.IsEnabled = true;
            this.RedoTool.IsEnabled = false;
            this.lastTool = this.MainInkToolbar.ActiveTool;
            if (!this.isMainCanvasEmpty())
            {
                this.SelectionTool.IsEnabled = true;
            }
            else
            {
                this.SelectionTool.IsEnabled = false;
            }
            await this.saveTempFile();
        }
        private void toggleTouchInking(bool canTouch)
        {
            this.ToggleTouchInkingButton.IsChecked = canTouch;
            if (canTouch)
            {
                this.MainCanvas.InkPresenter.InputDeviceTypes = ALL_INPUTS;
            }
            else
            {
                this.MainCanvas.InkPresenter.InputDeviceTypes = PEN_ONLY;
            }
        }
        private void invertUIColors()
        {
            this.isCanvasWhite = !this.isCanvasWhite;
            if (isCanvasWhite)
            {
                this.changeUIColor(this.whiteColor, this.blackColor);
            }
            else
            {
                this.changeUIColor(this.blackColor, this.whiteColor);
            }
        }
        private void changeUIColor(SolidColorBrush brush1, SolidColorBrush brush2)
        {
            this.InkPanel.Background = brush1;
            this.SlideViewButton.Foreground = brush2;
            var allStrokes = this.MainCanvas.InkPresenter.StrokeContainer.GetStrokes();
            foreach (var stroke in allStrokes)
            {
                if (stroke.DrawingAttributes.Color == brush1.Color)
                {
                    this.modifyStrokeColor(stroke, brush2.Color);
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

        #region IOReadWrite Helpers
        private async Task saveTempFile()
        {
            if (!this.isMainCanvasEmpty())
            {
                this.tempFileCount++;
                this.maxTempFile = this.tempFileCount;
                await this.ReaderWriter.SaveInkStrokes(this.getCurrentFileName(), this.MainCanvas.InkPresenter.StrokeContainer);
                this.SlidesView.updateSlide(this.ReaderWriter, this.getCurrentFileName(), this.InkPanel.Background);
            }
        }
        private string getCurrentFileName()
        {
            return string.Format(FILENAME, this.tempFileCount);
        }
        private async Task loadTempFile()
        {
            await this.ReaderWriter.LoadInkStrokes(this.getCurrentFileName(), this.MainCanvas.InkPresenter.StrokeContainer);
            this.SlidesView.updateSlide(this.ReaderWriter, this.getCurrentFileName(), this.InkPanel.Background);
        }
        private async void resetTempFolder()
        {
            this.ReaderWriter.CreateFolder();
            this.tempFileCount = 0;
            this.maxTempFile = 0;
            this.UndoTool.IsEnabled = false;
            this.RedoTool.IsEnabled = false;
            await this.saveTempFile();
        }
        #endregion

        #region InkStroke Helpers
        private void selectInkStrokes()
        {
            var selectionPoints = this.SelectionCanvas.selectionPoints;
            this.MainCanvas.InkPresenter.StrokeContainer.SelectWithPolyLine(selectionPoints);
            if (selectionPoints != null)
            {
                var centerPoint = this.lastpoint = this.findCenterPoint(selectionPoints);
                this.createCopyPasteDialogue(centerPoint);                
            }
        }
        private void createCopyPasteDialogue(Point center)
        {
            CopyPasteMovePopup cpmpopup = new CopyPasteMovePopup();
            Canvas.SetLeft(cpmpopup, center.X - 56);
            Canvas.SetTop(cpmpopup, center.Y - 14);

            cpmpopup.CopySelection += Cpmpopup_CopySelection;
            cpmpopup.MoveSelection += Cpmpopup_MoveSelection;
            cpmpopup.ClearSelection += Cpmpopup_ClearSelection;

            this.SelectionCanvas.showCopyMovePopup(cpmpopup);
        }
        private Point findCenterPoint(List<Point> points)
        {
            double horizontalCenter = 0;
            double verticalCenter = 0;
            int pointCount = points.Count();
            foreach (Point point in points)
            {
                horizontalCenter += point.X;
                verticalCenter += point.Y;
            }
            return new Point(horizontalCenter / pointCount, verticalCenter / pointCount);
        }
        private void Cpmpopup_ClearSelection(object sender, EventArgs e)
        {
            this.SelectionCanvas.resetSelection();
        }
        private void Cpmpopup_MoveSelection(object sender, EventArgs e)
        {
            this.trackSelectedInkMovement();
        }
        private void Cpmpopup_CopySelection(object sender, EventArgs e)
        {
            this.copySelectedInkStrokes();
        }
        private void toggleClickThroughSelectionLayer()
        {
            this.InkPanel.Children.Move(1, 0);
        }
        private void MainCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            this.placeSelectedInkStrokes();
        }
        private void MainCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(this.MainCanvas).Position;
            this.moveSelectedInkStrokes(currentPoint);
        }
        private void copySelectedInkStrokes()
        {
            this.MainCanvas.InkPresenter.StrokeContainer.CopySelectedToClipboard();
            this.MainCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(this.lastpoint);
            this.SelectionCanvas.resetSelection();
        }
        private void trackSelectedInkMovement()
        {
            this.MainCanvas.PointerMoved += MainCanvas_PointerMoved;
            this.MainCanvas.PointerReleased += MainCanvas_PointerReleased;
            this.toggleClickThroughSelectionLayer();
        }
        private void moveSelectedInkStrokes(Point newPoint)
        {
            if (this.lastpoint.X > 0 && this.lastpoint != newPoint)
            {
                var translation = new Point(newPoint.X - lastpoint.X, newPoint.Y - lastpoint.Y);
                this.MainCanvas.InkPresenter.StrokeContainer.MoveSelected(translation);
            }
            this.lastpoint = newPoint;
        }
        private void placeSelectedInkStrokes()
        {
            this.SelectionCanvas.resetSelection();
            this.toggleClickThroughSelectionLayer();
            this.MainCanvas.PointerMoved -= MainCanvas_PointerMoved;
            this.MainCanvas.PointerReleased -= MainCanvas_PointerReleased;
        }
        #endregion

        #region Undo/Redo Logic
        private async void processUndo()
        {
            this.tempFileCount--;
            this.toggleUndoAndRedoButtons();
            if (this.tempFileCount == 0 || (await this.ReaderWriter.Folder.GetFilesAsync()).Count == 1)
            {
                this.MainCanvas.InkPresenter.StrokeContainer.Clear();
                this.SlidesView.updateSlide(this.ReaderWriter, null, this.InkPanel.Background);
            }
            else
            {
                await this.loadTempFile();
            }
        }
        private async void processRedo()
        {
            this.tempFileCount++;
            this.toggleUndoAndRedoButtons();
            await this.loadTempFile();
        }
        private void toggleUndoAndRedoButtons()
        {
            this.UndoTool.IsChecked = false;
            this.RedoTool.IsChecked = false;
            this.MainInkToolbar.ActiveTool = this.lastTool;

            if (this.tempFileCount == 0)
            {
                this.UndoTool.IsEnabled = false;
            }
            else if (this.tempFileCount == this.maxTempFile)
            {
                this.RedoTool.IsEnabled = false;
            }
            else
            {
                this.RedoTool.IsEnabled = true;
                this.UndoTool.IsEnabled = true;
            }
        }
        #endregion

        #region SlidesView Logic
        private void SlideViewButton_Click(object sender, RoutedEventArgs e)
        {
            this.openSlidesView();
        }
        private void SlidesView_NewSlideCreated(object sender, EventArgs e)
        {
            this.clearCanvas();
            this.closeSlidesView();
        }
        private void SlidesView_SelectionMade(object sender, EventArgs e)
        {
            this.clearCanvas();
            this.SlidesView.updateCanvas(this.MainCanvas.InkPresenter.StrokeContainer);
            this.closeSlidesView();
        }
        private void openSlidesView()
        {
            this.SlidesViewAnimation.To = 0;
            this.SlidesViewSlide.Begin();
        }
        private void closeSlidesView()
        {
            this.SlidesViewAnimation.To = 400;
            this.SlidesViewSlide.Begin();
        }
        #endregion
    }
}
