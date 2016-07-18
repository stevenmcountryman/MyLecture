using System;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace MyLecture.Controls
{
    public sealed partial class Transcriber : UserControl
    {
        public delegate void TranscribeHandler(object sender, EventArgs e);
        public event TranscribeHandler TranscribeInkToText;

        public Transcriber()
        {
            this.InitializeComponent();
        }

        public async Task<string> TranscribeText(InkStrokeContainer inkStrokes)
        {
            var handwritingToText = "";
            if (inkStrokes.GetStrokes().Count > 0)
            {
                var inkRecog = new InkRecognizerContainer();
                var results = await inkRecog.RecognizeAsync(inkStrokes, InkRecognitionTarget.All);
                foreach (var result in results)
                {
                    handwritingToText += result.GetTextCandidates()[0] + " ";
                }
            }
            this.TranscribedText.Text = handwritingToText;
            this.PlayAudioButton.Visibility = Visibility.Visible;
            return handwritingToText;
        }

        private void TranscribeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.TranscribeInkToText(this, new EventArgs());
        }

        private async void PlayAudioButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(this.TranscribedText.Text);

            AudioPlayer.SetSource(stream, stream.ContentType);
            AudioPlayer.Play();            
        }
    }
}
