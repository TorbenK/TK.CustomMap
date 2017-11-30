// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TK.CustomMap.Sample.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            Xamarin.FormsMaps.Init("bUFZdlnCDt36XoMbwK8F~EI7ldg6jS_T-QtvV2adGQA~Aj1hd2qCzOVv_ITTnnbNamU6vHTe4BPWYkTIc1eGDk1uou5XdB-7nbbRLKk4kouA");

            LoadApplication(new Sample.App());
        }
    }
}