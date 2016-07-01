using MyLecture.Models;
using MyLecture.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private async void OpenLectureButton_Click(object sender, RoutedEventArgs e)
        {
            LectureFactory lectureFactory = new LectureFactory();
            await lectureFactory.OpenExistingLecture();
            this.Frame.Navigate(typeof(DrawingBoard), lectureFactory);
        }
    }
}
