using TK.CustomMap.Api;
using TK.CustomMap.Api.Google;
using Xamarin.Forms;

namespace TK.CustomMap.Sample
{
    public class App : Application
    {
        public App()
        {
            GmsPlace.Init("YOUR API KEY");
            GmsDirection.Init("AIzaSyCJN3Cd-Sp1a5V5OnkvTR-Gqhx7A3S-b6M");

            // The root page of your application
            var mainPage = new NavigationPage(new SamplePage());
            if (Device.OS == TargetPlatform.iOS)
            {
                mainPage.BarBackgroundColor = Color.FromHex("#f1f1f1");
            }
            MainPage = mainPage;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            TKNativePlacesApi.Instance.DisconnectAndRelease();
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}