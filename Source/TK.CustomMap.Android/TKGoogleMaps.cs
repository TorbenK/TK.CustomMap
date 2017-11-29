using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Common;
using Android.Gms.Maps;

namespace TK.CustomMap.Droid
{
    public static class TKGoogleMaps
    {
        public static bool IsInitialized { get;  set; }

        public static Context Context { get;  set; }

        public static void Init(Activity activity, Bundle bundle)
        {
            if (IsInitialized)
                return;

            Context = activity;

            TKCustomMapRenderer.Bundle = bundle;

#pragma warning disable 618
            if (GooglePlayServicesUtil.IsGooglePlayServicesAvailable(Context) == ConnectionResult.Success)
#pragma warning restore 618
            {
                try
                {
                    MapsInitializer.Initialize(Context);
                    IsInitialized = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Google Play Services Not Found");
                    Console.WriteLine("Exception: {0}", e);
                }
            }
        }
    }
}
