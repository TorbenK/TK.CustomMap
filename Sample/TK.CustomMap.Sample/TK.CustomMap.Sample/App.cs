using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            MainPage = new SamplePage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
