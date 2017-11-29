using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TK.CustomMap;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace CalloutSample
{
    public class App : Application
    {
        public App()
        {
            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    Children = 
                    {
                        new MyMap
                        {
                            MapCenter = new Position(40.7142700, -74.0059700),
                            CustomPins = new List<TKCustomMapPin>(new[] 
                            {
                                new TKCustomMapPin
                                {
                                    Title = "Custom Callout Sample",
                                    Position = new Position(40.7142700, -74.0059700),
                                    ShowCallout = true
                                }
                            })
                        }
                    }
                }
            };
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
