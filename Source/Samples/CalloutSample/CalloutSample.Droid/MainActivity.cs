using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using TK.CustomMap;
using TK.CustomMap.Droid;

namespace CalloutSample.Droid
{
    [Activity(Label = "CalloutSample", Theme="@style/MyTheme", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            ToolbarResource = Resource.Layout.toolbar;
            TabLayoutResource = Resource.Layout.tabs;

            // Xamarin.FormsMaps.Init(this, bundle);
            TKGoogleMaps.Init(this, bundle);

            LoadApplication(new App());
        }
    }
}

