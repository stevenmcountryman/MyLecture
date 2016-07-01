using MyLecture.Models;
using MyLecture.Views;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MyLecture
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
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
                    LectureFactory lectureFactory = new LectureFactory();
                    await lectureFactory.OpenExistingLecture(file);
                    this.Frame.Navigate(typeof(DrawingBoard), lectureFactory);
                }
            }
        }

        private async void OpenLectureButton_Click(object sender, RoutedEventArgs e)
        {
            LectureFactory lectureFactory = new LectureFactory();
            await lectureFactory.OpenExistingLecture();
            this.Frame.Navigate(typeof(DrawingBoard), lectureFactory);
        }
    }
}
