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

            Xamarin.FormsMaps.Init("INSERT_AUTHENTICATION_TOKEN_HERE");

            LoadApplication(new Sample.App());
        }
    }
}