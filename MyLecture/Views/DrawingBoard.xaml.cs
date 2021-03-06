﻿using MyLecture.Controls;
using MyLecture.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Popups;
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
        readonly static CoreInputDeviceTypes ALL_INPUTS = CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen;
        readonly static CoreInputDeviceTypes PEN_ONLY = CoreInputDeviceTypes.Pen;

        private Boolean isCanvasWhite = true;
        private SolidColorBrush whiteColor = new SolidColorBrush(Colors.White);
        private SolidColorBrush blackColor = new SolidColorBrush(Colors.Black);
        private bool canTouchInk = false;
        private InkToolbarToolButton lastTool;
        private Point lastpoint;

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

            if (e.Parameter != null)
            {
                this.lectureFactory = e.Parameter as LectureFactory;
                this.SlidesView.ShowLoadedSlides(this.lectureFactory.GetAllSlides(), this.lectureFactory.GetSlideBackgroundConfig(), this.lectureFactory.LectureName);
            }
            else
            {
                this.lectureFactory = new LectureFactory();
                this.lectureFactory.CreateNewLecture();
            }
            this.MainCanvas.InkPresenter.StrokeContainer = this.lectureFactory.GetSlideAt(0);
            if (this.lectureFactory.SlideBackgroundIsWhite(0))
            {
                this.InkPanel.Background = this.whiteColor;
            }
            else
            {
                this.InkPanel.Background = this.blackColor;
            }
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
            this.MainCanvas.InkPresenter.StrokeContainer = new InkStrokeContainer();
            this.lectureFactory.ClearSnapshots();
            this.resetSnapshotMemory();
        }
        private async void updateInkCanvasAndTools()
        {
            this.lastTool = this.MainInkToolbar.ActiveTool;
            await this.lectureFactory.SaveSnapshot(this.MainCanvas.InkPresenter.StrokeContainer);
            this.saveSlide();
            this.resetSnapshotMemory();
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
            this.saveSlide();
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
        private void resetSnapshotMemory()
        {
            this.UndoTool.IsChecked = false;
            this.RedoTool.IsChecked = false;
            this.UndoTool.IsEnabled = this.lectureFactory.CanGoBack();
            this.RedoTool.IsEnabled = this.lectureFactory.CanGoForward();
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
        private void processUndo()
        {
            var snapshot = this.lectureFactory.LoadPreviousSnapshot();
            if (snapshot != null)
            {
                this.MainCanvas.InkPresenter.StrokeContainer = snapshot;
            }
            else
            {
                this.MainCanvas.InkPresenter.StrokeContainer = new InkStrokeContainer();
            }
            this.updateUndoRedoTools();
            this.saveSlide();
        }
        private void processRedo()
        {
            var snapshot = this.lectureFactory.LoadNextSnapshot();
            if (snapshot != null)
            {
                this.MainCanvas.InkPresenter.StrokeContainer = snapshot;
            }
            else
            {
                this.MainCanvas.InkPresenter.StrokeContainer = new InkStrokeContainer();
            }
            this.updateUndoRedoTools();
            this.saveSlide();
        }
        private void updateUndoRedoTools()
        {
            this.MainInkToolbar.ActiveTool = this.lastTool;
            this.resetSnapshotMemory();
        }
        private void saveSlide()
        {
            bool isBackgroundWhite = false;
            if ((this.InkPanel.Background as SolidColorBrush).Color == Colors.White)
            {
                isBackgroundWhite = true;
            }

            this.lectureFactory.SaveSlide(this.MainCanvas.InkPresenter.StrokeContainer, isBackgroundWhite, this.SlidesView.SlideIndex);
            this.SlidesView.UpdateSlide(this.MainCanvas.InkPresenter.StrokeContainer, this.InkPanel.Background);
        }
        #endregion

        #region SlidesView Logic
        private void SlideViewButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.openSlidesView();
            this.SlidesView.SlidesClosed += SlidesView_SlidesClosed;
        }
        private async void SlidesView_ChooseSlide(object sender, EventArgs e)
        {
            this.clearCanvas();
            var chosenSlide = this.lectureFactory.GetSlideAt(this.SlidesView.SlideIndex);
            this.MainCanvas.InkPresenter.StrokeContainer = chosenSlide;
            if (this.lectureFactory.SlideBackgroundIsWhite(this.SlidesView.SlideIndex))
            {
                this.InkPanel.Background = this.whiteColor;
            }
            else
            {
                this.InkPanel.Background = this.blackColor;
            }
            await this.lectureFactory.SaveSnapshotBase(this.MainCanvas.InkPresenter.StrokeContainer);
            this.resetSnapshotMemory();
            this.closeSlidesView();
        }
        private void SlidesView_CreateNewSlide(object sender, EventArgs e)
        {
            this.clearCanvas();
            var newSlide = this.lectureFactory.AddNewBlankSlide();
            this.MainCanvas.InkPresenter.StrokeContainer = newSlide;
            this.closeSlidesView();
        }
        private void openSlidesView()
        {
            this.SlidesView.Visibility = Visibility.Visible;
            this.SlidesView.OpenSlidesView();
        }
        private void closeSlidesView()
        {
            this.SlidesView.CloseSlidesView();
        }

        private void SlidesView_SlidesClosed(object sender, EventArgs e)
        {
            this.SlidesView.SlidesClosed -= SlidesView_SlidesClosed;
            this.SlidesView.Visibility = Visibility.Collapsed;
        }
        private async void SlidesView_SaveButtonTapped(object sender, EventArgs e)
        {
            var result = await this.lectureFactory.SaveLectureAs(this.SlidesView.GetTitle());
            if (result == LectureFactory.filePickerResult.SUCCESS)
            {
                this.showDialog("Success", "Lecture saved successfully!");
            }
            else if (result == LectureFactory.filePickerResult.FAILED)
            {
                this.showDialog("Failure", "Error saving lecture. Try again.");
            }
        }
        private async void SlidesView_ExportButtonPressed(object sender, EventArgs e)
        {
            var result = await this.lectureFactory.ExportToImages(this.SlidesView.GetTitle());
            if (result == LectureFactory.filePickerResult.SUCCESS)
            {
                this.showDialog("Success", "Images saved successfully!");
            }
            else if (result == LectureFactory.filePickerResult.FAILED)
            {
                this.showDialog("Failure", "Error saving images. Try again.");
            }
        }
        private async void SlidesView_SlideDeleted(object sender, EventArgs e)
        {
            this.lectureFactory.DeleteSlide(this.SlidesView.SlideIndexToModify);
            this.clearCanvas();
            if (this.lectureFactory.GetAllSlides().Count > 1)
            {
                var chosenSlide = this.lectureFactory.GetSlideAt(this.SlidesView.SlideIndex);
                this.MainCanvas.InkPresenter.StrokeContainer = chosenSlide;
                await this.lectureFactory.SaveSnapshotBase(this.MainCanvas.InkPresenter.StrokeContainer);
                this.resetSnapshotMemory();
            }
            else if (this.lectureFactory.GetAllSlides().Count == 1)
            {
                var chosenSlide = this.lectureFactory.GetSlideAt(0);
                this.MainCanvas.InkPresenter.StrokeContainer = chosenSlide;
                await this.lectureFactory.SaveSnapshotBase(this.MainCanvas.InkPresenter.StrokeContainer);
                this.resetSnapshotMemory();
            }
            else
            {
                this.SlidesView.AddNewBlankSlide();
            }
        }
        private void SlidesView_SlideMoved(object sender, EventArgs e)
        {
            this.lectureFactory.MoveSlide(this.SlidesView.SlideIndexToModify, this.SlidesView.SlideIndex);
        }
        private async void showDialog(string title, string message)
        {
            MessageDialog dialog = new MessageDialog(message);
            dialog.Title = title;
            dialog.Commands.Add(new UICommand("ok"));
            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }
        #endregion

        private void MainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 480 && this.ExpandTool.Visibility == Visibility.Collapsed)
            {
                this.ExpandTool.Visibility = Visibility.Visible;
                this.hideExtraTools();
            }
            else if (e.NewSize.Width >= 480 && this.ExpandTool.Visibility == Visibility.Visible)
            {
                this.ExpandTool.Visibility = Visibility.Collapsed;
                this.showExtraTools();
            }
        }

        private void hideExtraTools()
        {
            this.UndoTool.Visibility = Visibility.Collapsed;
            this.RedoTool.Visibility = Visibility.Collapsed;
            this.ToggleTouchInkingButton.Visibility = Visibility.Collapsed;
            this.ToggleBackgroundColor.Visibility = Visibility.Collapsed;
        }
        private void showExtraTools()
        {
            this.UndoTool.Visibility = Visibility.Visible;
            this.RedoTool.Visibility = Visibility.Visible;
            this.ToggleTouchInkingButton.Visibility = Visibility.Visible;
            this.ToggleBackgroundColor.Visibility = Visibility.Visible;
        }

        private void ExpandTool_Click(object sender, RoutedEventArgs e)
        {
            this.ExpandTool.IsChecked = false;
            this.MainInkToolbar.ActiveTool = this.lastTool;

            if (this.UndoTool.Visibility == Visibility.Visible)
            {
                this.hideExtraTools();
            }
            else
            {
                this.showExtraTools();
            }
        }

        private void MainInkToolbar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > e.PreviousSize.Width)
            {
                this.InkToolbarScroller.ChangeView(this.MainInkToolbar.ActualWidth, 0, 1);
            }
            else
            {
                this.InkToolbarScroller.ChangeView(0, 0, 1);
            }
        }

        private async void SlidesView_ExportAsTextButtonPressed(object sender, EventArgs e)
        {
            var result = await this.lectureFactory.ExportToText(this.SlidesView.GetTitle());
            if (result == LectureFactory.filePickerResult.SUCCESS)
            {
                this.showDialog("Success", "Text file saved successfully!");
            }
            else if (result == LectureFactory.filePickerResult.FAILED)
            {
                this.showDialog("Failure", "Error saving text. Try again.");
            }
        }
    }
}
