using MyLecture.Models;
using MyLecture.Views;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;
using Windows.Storage.AccessCache;
using Windows.UI.Text;

namespace MyLecture
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private LectureFactory lectureFactory = new LectureFactory();
        public MainPage()
        {
            this.InitializeComponent();

            this.setTitleBar();
            this.setRecentFileList();
        }

        private void setRecentFileList()
        {

            foreach(var entry in StorageApplicationPermissions.FutureAccessList.Entries)
            {
                var entryPanel = new StackPanel()
                {
                    Tag = entry.Token,
                    Orientation = Orientation.Horizontal                                      
                };
                var entryName = new TextBlock()
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    FontWeight = FontWeights.Bold,
                    Text = entry.Token.Substring(0, entry.Token.IndexOf(".smc")),
                    Margin = new Thickness(4)
                };
                var entryDate = new TextBlock()
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    Text = entry.Token.Substring(entry.Token.IndexOf(".smc") + 4),
                    Margin = new Thickness(4)
                };
                entryPanel.Children.Add(entryName);
                entryPanel.Children.Add(entryDate);
                this.RecentFilesList.Items.Add(entryPanel);
            }
        }

        private void setTitleBar()
        {

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                ApplicationView AppView = ApplicationView.GetForCurrentView();
                AppView.TitleBar.BackgroundColor = Color.FromArgb(255, 0, 145, 92);
                AppView.TitleBar.ButtonInactiveBackgroundColor = Colors.Gray;
                AppView.TitleBar.ButtonInactiveForegroundColor = Colors.LightGray;
                AppView.TitleBar.ButtonBackgroundColor = Color.FromArgb(255, 0, 145, 92);
                AppView.TitleBar.ButtonForegroundColor = Colors.White;
                AppView.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 0, 197, 125);
                AppView.TitleBar.ButtonHoverForegroundColor = Colors.White;
                AppView.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 0, 145, 92);
                AppView.TitleBar.ButtonPressedForegroundColor = Colors.White;
                AppView.TitleBar.ForegroundColor = Colors.White;
                AppView.TitleBar.InactiveBackgroundColor = Colors.Gray;
                AppView.TitleBar.InactiveForegroundColor = Colors.LightGray;
            }
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                statusBar.BackgroundColor = Color.FromArgb(255, 0, 145, 92);
                statusBar.ForegroundColor = Colors.White;
            }
        }

        private void NewLectureButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(DrawingBoard));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var args = e.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
            if (args != null)
            {
                if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                    var file = (StorageFile)fileArgs.Files[0];
                    var token = file.Name + file.DateCreated.UtcDateTime;
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, file);
                    lectureFactory = new LectureFactory();
                    await lectureFactory.OpenExistingLecture(token);
                    this.Frame.Navigate(typeof(DrawingBoard), lectureFactory);
                }
            }
        }

        private async void OpenLectureButton_Click(object sender, RoutedEventArgs e)
        {
            lectureFactory = new LectureFactory();
            if (await lectureFactory.OpenExistingLecture())
            {
                this.Frame.Navigate(typeof(DrawingBoard), lectureFactory);
            }
            else
            {
                this.showDialog("Failure", "Error opening lecture file. Try again");
            }
        }
        private async void showDialog(string title, string message)
        {
            MessageDialog dialog = new MessageDialog(message);
            dialog.Title = title;
            dialog.Commands.Add(new UICommand("ok"));
            dialog.DefaultCommandIndex = 0;
            await dialog.ShowAsync();
        }

        private async void RecentFilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var token = (this.RecentFilesList.SelectedItem as StackPanel).Tag.ToString();
            
            await lectureFactory.OpenExistingLecture(token);
            this.Frame.Navigate(typeof(DrawingBoard), lectureFactory);
        }
    }
}
